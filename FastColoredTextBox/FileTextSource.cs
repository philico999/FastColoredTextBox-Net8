//#define debug

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// This class contains the source text (chars and styles).
    /// It stores a text lines, the manager of commands, undo/redo stack, styles.
    /// </summary>
    public class FileTextSource : TextSource, IDisposable
    {
        List<int> sourceFileLinePositions = new List<int>();
        FileStream fs;
        Encoding fileEncoding;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        /// <summary>
        /// Occurs when need to display line in the textbox
        /// </summary>
        public event EventHandler<LineNeededEventArgs> LineNeeded;

        /// <summary>
        /// Occurs when need to save line in the file
        /// </summary>
        public event EventHandler<LinePushedEventArgs> LinePushed;

        public FileTextSource(FastColoredTextBox currentTB)
            : base(currentTB)
        {
            timer.Interval = 10000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Enabled = true;

            SaveEOL = Environment.NewLine;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Enabled = false;
            try
            {
                UnloadUnusedLines();
            }
            finally
            {
                timer.Enabled = true;
            }
        }

        private void UnloadUnusedLines()
        {
            const int margin = 2000;
            var iStartVisibleLine = CurrentTB.VisibleRange.Start.iLine;
            var iFinishVisibleLine = CurrentTB.VisibleRange.End.iLine;

            int count = 0;
            for (int i = 0; i < Count; i++)
                if (base.lines[i] != null && !base.lines[i].IsChanged && Math.Abs(i - iFinishVisibleLine) > margin)
                {
                    base.lines[i] = null;
                    count++;
                }
            #if debug
            Console.WriteLine("UnloadUnusedLines: " + count);
            #endif
        }

        public void OpenFile(string fileName, Encoding enc)
        {
            Clear();

            if (fs != null)
                fs.Dispose();

            SaveEOL = Environment.NewLine;

            // read lines of file
            fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var length = fs.Length;

            // detect encoding / consume BOM
            enc = DefineEncoding(enc, fs);
            int shift = DefineShift(enc); // kept for compatibility if other code expects it

            // first line starts at current byte position (after BOM, if any)
            sourceFileLinePositions.Add((int)fs.Position);
            base.lines.Add(null);

            // pre-size the positions list
            sourceFileLinePositions.Capacity = (int)(length / 7 + 1000);

            // Fast, encoding-aware newline scan without decoding whole chars
            int codePage = enc.CodePage;

            // Common helpers
            const int BUFSZ = 64 * 1024;

            // Track if last seen code unit was CR and where that CR ended (byte position after CR)
            bool prevWasCR = false;
            int prevCREndPos = 0;

            long startPos = fs.Position;

            if (codePage == 65001 || enc.IsSingleByte) // UTF-8 or single-byte encodings
            {
                byte[] buf = new byte[BUFSZ];
                int read;
                while ((read = fs.Read(buf, 0, buf.Length)) > 0)
                {
                    for (int i = 0; i < read; i++)
                    {
                        byte b = buf[i];
                        int posAfter = (int)(fs.Position - (read - i - 1)); // byte pos *after* consuming this byte

                        if (b == 0x0A) // \n
                        {
                            sourceFileLinePositions.Add(posAfter /* + shift if you used it before */);
                            base.lines.Add(null);
                            prevWasCR = false; // reset
                        }
                        else
                        {
                            if (prevWasCR) // standalone CR line ending (old Mac)
                            {
                                sourceFileLinePositions.Add(prevCREndPos /* + shift */);
                                base.lines.Add(null);
                                SaveEOL = "\r";
                                prevWasCR = false;
                            }
                            if (b == 0x0D) // \r
                            {
                                prevWasCR = true;
                                prevCREndPos = posAfter; // CR ends here
                            }
                        }
                    }
                }
            }
            else if (codePage == 1200) // UTF-16 LE (Unicode)
            {
                byte[] buf = new byte[BUFSZ - (BUFSZ % 2)];
                int read;
                while ((read = fs.Read(buf, 0, buf.Length)) > 0)
                {
                    for (int i = 0; i + 1 < read; i += 2)
                    {
                        ushort cu = (ushort)(buf[i] | (buf[i + 1] << 8));
                        int posAfter = (int)(fs.Position - (read - (i + 2))); // after these 2 bytes

                        if (cu == 0x000A) // \n
                        {
                            sourceFileLinePositions.Add(posAfter /* + shift */);
                            base.lines.Add(null);
                            prevWasCR = false;
                        }
                        else
                        {
                            if (prevWasCR) // standalone CR
                            {
                                sourceFileLinePositions.Add(prevCREndPos /* + shift */);
                                base.lines.Add(null);
                                SaveEOL = "\r";
                                prevWasCR = false;
                            }
                            if (cu == 0x000D) // \r
                            {
                                prevWasCR = true;
                                prevCREndPos = posAfter;
                            }
                        }
                    }
                }
            }
            else if (codePage == 1201) // UTF-16 BE (BigEndianUnicode)
            {
                byte[] buf = new byte[BUFSZ - (BUFSZ % 2)];
                int read;
                while ((read = fs.Read(buf, 0, buf.Length)) > 0)
                {
                    for (int i = 0; i + 1 < read; i += 2)
                    {
                        ushort cu = (ushort)((buf[i] << 8) | buf[i + 1]);
                        int posAfter = (int)(fs.Position - (read - (i + 2)));

                        if (cu == 0x000A)
                        {
                            sourceFileLinePositions.Add(posAfter /* + shift */);
                            base.lines.Add(null);
                            prevWasCR = false;
                        }
                        else
                        {
                            if (prevWasCR)
                            {
                                sourceFileLinePositions.Add(prevCREndPos /* + shift */);
                                base.lines.Add(null);
                                SaveEOL = "\r";
                                prevWasCR = false;
                            }
                            if (cu == 0x000D)
                            {
                                prevWasCR = true;
                                prevCREndPos = posAfter;
                            }
                        }
                    }
                }
            }
            else if (codePage == 12000) // UTF-32 LE
            {
                byte[] buf = new byte[BUFSZ - (BUFSZ % 4)];
                int read;
                while ((read = fs.Read(buf, 0, buf.Length)) > 0)
                {
                    for (int i = 0; i + 3 < read; i += 4)
                    {
                        uint cp = (uint)(buf[i] | (buf[i + 1] << 8) | (buf[i + 2] << 16) | (buf[i + 3] << 24));
                        int posAfter = (int)(fs.Position - (read - (i + 4)));

                        if (cp == 0x0000000A) // \n
                        {
                            sourceFileLinePositions.Add(posAfter /* + shift */);
                            base.lines.Add(null);
                            prevWasCR = false;
                        }
                        else
                        {
                            if (prevWasCR)
                            {
                                sourceFileLinePositions.Add(prevCREndPos /* + shift */);
                                base.lines.Add(null);
                                SaveEOL = "\r";
                                prevWasCR = false;
                            }
                            if (cp == 0x0000000D) // \r
                            {
                                prevWasCR = true;
                                prevCREndPos = posAfter;
                            }
                        }
                    }
                }
            }
            else if (codePage == 12001) // UTF-32 BE
            {
                byte[] buf = new byte[BUFSZ - (BUFSZ % 4)];
                int read;
                while ((read = fs.Read(buf, 0, buf.Length)) > 0)
                {
                    for (int i = 0; i + 3 < read; i += 4)
                    {
                        uint cp = (uint)((buf[i] << 24) | (buf[i + 1] << 16) | (buf[i + 2] << 8) | buf[i + 3]);
                        int posAfter = (int)(fs.Position - (read - (i + 4)));

                        if (cp == 0x0000000A)
                        {
                            sourceFileLinePositions.Add(posAfter /* + shift */);
                            base.lines.Add(null);
                            prevWasCR = false;
                        }
                        else
                        {
                            if (prevWasCR)
                            {
                                sourceFileLinePositions.Add(prevCREndPos /* + shift */);
                                base.lines.Add(null);
                                SaveEOL = "\r";
                                prevWasCR = false;
                            }
                            if (cp == 0x0000000D)
                            {
                                prevWasCR = true;
                                prevCREndPos = posAfter;
                            }
                        }
                    }
                }
            }
            else
            {
                // Fallback: treat like UTF-8/single-byte (works for most legacy single-byte encodings).
                // If you truly have a rare multi-byte legacy code page, add a branch above.
                byte[] buf = new byte[BUFSZ];
                int read;
                while ((read = fs.Read(buf, 0, buf.Length)) > 0)
                {
                    for (int i = 0; i < read; i++)
                    {
                        byte b = buf[i];
                        int posAfter = (int)(fs.Position - (read - i - 1));

                        if (b == 0x0A)
                        {
                            sourceFileLinePositions.Add(posAfter);
                            base.lines.Add(null);
                            prevWasCR = false;
                        }
                        else
                        {
                            if (prevWasCR)
                            {
                                sourceFileLinePositions.Add(prevCREndPos);
                                base.lines.Add(null);
                                SaveEOL = "\r";
                                prevWasCR = false;
                            }
                            if (b == 0x0D)
                            {
                                prevWasCR = true;
                                prevCREndPos = posAfter;
                            }
                        }
                    }
                }
            }

            // File ended right after a CR (no LF)
            if (prevWasCR)
            {
                sourceFileLinePositions.Add(prevCREndPos /* + shift */);
                base.lines.Add(null);
                SaveEOL = "\r";
            }

            if (length > 2000000)
                GC.Collect();

            Line[] temp = new Line[100];

            var c = base.lines.Count;
            base.lines.AddRange(temp);
            base.lines.TrimExcess();
            base.lines.RemoveRange(c, temp.Length);

            int[] temp2 = new int[100];
            c = base.lines.Count;
            sourceFileLinePositions.AddRange(temp2);
            sourceFileLinePositions.TrimExcess();
            sourceFileLinePositions.RemoveRange(c, temp.Length);

            fileEncoding = enc;

            OnLineInserted(0, Count);
            // load first lines for calc width of the text
            var linesCount = Math.Min(lines.Count, CurrentTB.ClientRectangle.Height / CurrentTB.CharHeight);
            for (int i = 0; i < linesCount; i++)
                LoadLineFromSourceFile(i);

            NeedRecalc(new TextChangedEventArgs(0, linesCount - 1));
            if (CurrentTB.WordWrap)
                OnRecalcWordWrap(new TextChangedEventArgs(0, linesCount - 1));
        }

        private int DefineShift(Encoding enc)
        {
            if (enc.IsSingleByte)
                return 0;

            if (enc.HeaderName == "unicodeFFFE")
                return 0;//UTF16 BE

            if (enc.HeaderName == "utf-16")
                return 1;//UTF16 LE

            if (enc.HeaderName == "utf-32BE")
                return 0;//UTF32 BE

            if (enc.HeaderName == "utf-32")
                return 3;//UTF32 LE

            return 0;
        }

        private static Encoding DefineEncoding(Encoding enc, FileStream fs)
        {
            int bytesPerSignature = 0;
            byte[] signature = new byte[4];
            int c = fs.Read(signature, 0, 4);
            if (signature[0] == 0xFF && signature[1] == 0xFE && signature[2] == 0x00 && signature[3] == 0x00 && c >= 4)
            {
                enc = Encoding.UTF32;//UTF32 LE
                bytesPerSignature = 4;
            }
            else
            if (signature[0] == 0x00 && signature[1] == 0x00 && signature[2] == 0xFE && signature[3] == 0xFF)
            {
                enc = new UTF32Encoding(true, true);//UTF32 BE
                bytesPerSignature = 4;
            }
            else
            if (signature[0] == 0xEF && signature[1] == 0xBB && signature[2] == 0xBF)
            {
                enc = Encoding.UTF8;//UTF8
                bytesPerSignature = 3;
            }
            else
            if (signature[0] == 0xFE && signature[1] == 0xFF)
            {
                enc = Encoding.BigEndianUnicode;//UTF16 BE
                bytesPerSignature = 2;
            }
            else
            if (signature[0] == 0xFF && signature[1] == 0xFE)
            {
                enc = Encoding.Unicode;//UTF16 LE
                bytesPerSignature = 2;
            }

            fs.Seek(bytesPerSignature, SeekOrigin.Begin);

            return enc;
        }

        public void CloseFile()
        {
            if(fs!=null)
                try
                {
                    fs.Dispose();
                }
                catch
                {
                    ;
                }
            fs = null;
        }

        /// <summary>
        /// End Of Line characters used for saving
        /// </summary>
        public string SaveEOL { get; set; }

        public override void SaveToFile(string fileName, Encoding enc)
        {
            //
            var newLinePos = new List<int>(Count);
            //create temp file
            var dir = Path.GetDirectoryName(fileName);
            var tempFileName = Path.Combine(dir, Path.GetFileNameWithoutExtension(fileName) + ".tmp");

            StreamReader sr = new StreamReader(fs, fileEncoding);
            using (FileStream tempFs = new FileStream(tempFileName, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(tempFs, enc))
            {
                sw.Flush();

                for (int i = 0; i < Count; i++)
                {
                    newLinePos.Add((int)tempFs.Length);

                    var sourceLine = ReadLine(sr, i);//read line from source file
                    string line;

                    bool lineIsChanged = lines[i] != null && lines[i].IsChanged;

                    if (lineIsChanged)
                        line = lines[i].Text;
                    else
                        line = sourceLine;

                    //call event handler
                    if (LinePushed != null)
                    {
                        var args = new LinePushedEventArgs(sourceLine, i, lineIsChanged ? line : null);
                        LinePushed(this, args);

                        if(args.SavedText != null)
                            line = args.SavedText;
                    }

                    //save line to file
                    sw.Write(line);

                    if (i < Count - 1)
                        sw.Write(SaveEOL);

                    sw.Flush();
                }
            }

            //clear lines buffer
            for (int i = 0; i < Count; i++)
                lines[i] = null;
            //deattach from source file
            sr.Dispose();
            fs.Dispose();
            //delete target file
            if (File.Exists(fileName))
                File.Delete(fileName);
            //rename temp file
            File.Move(tempFileName, fileName);

            //binding to new file
            sourceFileLinePositions = newLinePos;
            fs = new FileStream(    
                fileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            this.fileEncoding = enc;
        }

        // Write editor content to a temp file efficiently.
        private static void SaveToTemp(string tempPath, ReadOnlySpan<char> text, Encoding encoding)
        {
            // 1–4 MB buffer sizes are reasonable for large files.
            var opts = new FileStreamOptions
            {
                Mode = FileMode.Create,
                Access = FileAccess.Write,
                Share = FileShare.Read, // allow readers while writing the temp
                Options = FileOptions.SequentialScan,
                BufferSize = 1024 * 1024 // 1 MB
            };

            using var fs = new FileStream(tempPath, opts);
            using var writer = new StreamWriter(fs, encoding, bufferSize: 1024 * 16, leaveOpen: false);
            writer.NewLine = Environment.NewLine;
            writer.Write(text);
        }

        private string ReadLine(StreamReader sr, int i)
        {
            string line;
            var filePos = sourceFileLinePositions[i];
            if (filePos < 0)
                return "";
            fs.Seek(filePos, SeekOrigin.Begin);
            sr.DiscardBufferedData();
            line = sr.ReadLine();
            return line;
        }

        public override void ClearIsChanged()
        {
            foreach (var line in lines)
                if(line!=null)
                    line.IsChanged = false;
        }

        public override Line this[int i]
        {
            get 
            {
                if (base.lines[i] != null)
                    return lines[i];
                else
                    LoadLineFromSourceFile(i);

                return lines[i];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private void LoadLineFromSourceFile(int i)
        {
            var line = CreateLine();
            fs.Seek(sourceFileLinePositions[i], SeekOrigin.Begin);
            using (var sr = new StreamReader(fs, fileEncoding, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
            {
                var s = sr.ReadLine();
                if (s == null)
                    s = "";

                //call event handler
                if (LineNeeded != null)
                {
                    var args = new LineNeededEventArgs(s, i);
                    LineNeeded(this, args);
                    s = args.DisplayedLineText;
                    if (s == null)
                        return;
                }

                foreach (var c in s)
                    line.Add(new Char(c));
                base.lines[i] = line;

                if (CurrentTB.WordWrap)
                    OnRecalcWordWrap(new TextChangedEventArgs(i, i));
            }
        }

        public override void InsertLine(int index, Line line)
        {
            sourceFileLinePositions.Insert(index, -1);
            base.InsertLine(index, line);
        }

        public override void RemoveLine(int index, int count)
        {
            sourceFileLinePositions.RemoveRange(index, count);
            base.RemoveLine(index, count);
        }

        public override void Clear()
        {
            base.Clear();
        }

        public override int GetLineLength(int i)
        {
            if (base.lines[i] == null)
                return 0;
            else
                return base.lines[i].Count;
        }

        public override bool LineHasFoldingStartMarker(int iLine)
        {
            if (lines[iLine] == null)
                return false;
            else
                return !string.IsNullOrEmpty(lines[iLine].FoldingStartMarker);
        }

        public override bool LineHasFoldingEndMarker(int iLine)
        {
            if (lines[iLine] == null)
                return false;
            else
                return !string.IsNullOrEmpty(lines[iLine].FoldingEndMarker);
        }

        public override void Dispose()
        {
            if (fs != null)
                fs.Dispose();

            timer.Dispose();
        }

        internal void UnloadLine(int iLine)
        {
            if (lines[iLine] != null && !lines[iLine].IsChanged)
                lines[iLine] = null;
        }
    }

    public class LineNeededEventArgs : EventArgs
    {
        public string SourceLineText { get; private set; }
        public int DisplayedLineIndex { get; private set; }
        /// <summary>
        /// This text will be displayed in textbox
        /// </summary>
        public string DisplayedLineText { get; set; }

        public LineNeededEventArgs(string sourceLineText, int displayedLineIndex)
        {
            this.SourceLineText = sourceLineText;
            this.DisplayedLineIndex = displayedLineIndex;
            this.DisplayedLineText = sourceLineText;
        }
    }

    public class LinePushedEventArgs : EventArgs
    {
        public string SourceLineText { get; private set; }
        public int DisplayedLineIndex { get; private set; }
        /// <summary>
        /// This property contains only changed text.
        /// If text of line is not changed, this property contains null.
        /// </summary>
        public string DisplayedLineText { get; private set; }
        /// <summary>
        /// This text will be saved in the file
        /// </summary>
        public string SavedText { get; set; }

        public LinePushedEventArgs(string sourceLineText, int displayedLineIndex, string displayedLineText)
        {
            this.SourceLineText = sourceLineText;
            this.DisplayedLineIndex = displayedLineIndex;
            this.DisplayedLineText = displayedLineText;
            this.SavedText = displayedLineText;
        }
    }

    class CharReader : TextReader
    {
        public override int Read()
        {
            return base.Read();
        }
    }
}
