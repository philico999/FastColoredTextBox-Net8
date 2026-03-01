using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FastColoredTextBoxNS
{
    public enum EditorStyle
    {
        Default,
        Dark,
        DeepBlack,
        Khaki,
        Light,
        NavajoRed,
        RubyBlueDark,
        RubyBlueLight,
        Twilight,
        Custom1,
        Custom2,
        Custom3
    }

    public static class EditorStyleColors
    {
        // Internal dictionary for colors — can be updated at runtime
        public static readonly Dictionary<string, Color> EditorBackColor = new Dictionary<string, Color>
    {
        { "Dark", Color.FromArgb(37, 37, 48) },
        { "DeepBlack", Color.FromArgb(15, 15, 15) },
        { "Default", Color.FromArgb(60, 60, 60) },
        { "Khaki", Color.FromArgb(240, 255, 202) },
        { "Light", Color.FromArgb(255, 253, 247) },
        { "NavajoRed", Color.FromArgb(255, 248, 224) },
        { "RubyBlueDark", Color.FromArgb(20, 30, 40) },
        { "RubyBlueLight", Color.FromArgb(223, 242, 255) },
        { "Twilight", Color.FromArgb(205, 205, 205) },
        { "Custom1", Color.FromArgb(37, 37, 48) },
        { "Custom2", Color.FromArgb(255, 253, 247) },
        { "Custom3", Color.FromArgb(20, 30, 40) }
    };

        // Internal dictionary for colors — can be updated at runtime
        public static readonly Dictionary<string, Color> EditorSelectionColor = new Dictionary<string, Color>
    {
        { "Dark", Color.FromArgb(120, 255, 255, 255) },
        { "DeepBlack", Color.FromArgb(120, 255, 255, 255) },
        { "Default", Color.FromArgb(120, 255, 255, 255) },
        { "Khaki", Color.FromArgb(60, 0, 0, 255) },
        { "Light", Color.FromArgb(60, 0, 0, 255) },
        { "NavajoRed", Color.FromArgb(60, 0, 0, 255) },
        { "RubyBlueDark", Color.FromArgb(120, 255, 255, 255) },
        { "RubyBlueLight", Color.FromArgb(60, 0, 0, 255) },
        { "Twilight", Color.FromArgb(60, 0, 0, 255) },
        { "Custom1", Color.FromArgb(120, 255, 255, 255) },
        { "Custom2", Color.FromArgb(60, 0, 0, 255) },
        { "Custom3", Color.FromArgb(60, 0, 0, 255) }
    };

        /// <summary>
        /// Updates a color by name, if it exists in the color map.
        /// </summary>
        public static bool UpdateColor(string name, Color value)
        {
            if (EditorBackColor.ContainsKey(name))
            {
                EditorBackColor[name] = value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all style names and colors as a dictionary.
        /// </summary>
        public static IReadOnlyDictionary<string, Color> GetAll() => EditorBackColor;
    }

    /// <summary>
    /// Style of chars
    /// </summary>
    /// <remarks>This is base class for all text and design renderers</remarks>
    public abstract class Style : IDisposable
    {
        /// <summary>
        /// This style is exported to outer formats (HTML for example)
        /// </summary>
        public virtual bool IsExportable { get; set; }
        /// <summary>
        /// Occurs when user click on StyleVisualMarker joined to this style 
        /// </summary>
        public event EventHandler<VisualMarkerEventArgs> VisualMarkerClick;

        /// <summary>
        /// Constructor
        /// </summary>
        public Style()
        {
            IsExportable = true;
        }

        /// <summary>
        /// Renders given range of text
        /// </summary>
        /// <param name="gr">Graphics object</param>
        /// <param name="position">Position of the range in absolute control coordinates</param>
        /// <param name="range">Rendering range of text</param>
        public abstract void Draw(Graphics gr, Point position, Range range);

        /// <summary>
        /// Occurs when user click on StyleVisualMarker joined to this style 
        /// </summary>
        public virtual void OnVisualMarkerClick(FastColoredTextBox tb, VisualMarkerEventArgs args)
        {
            if (VisualMarkerClick != null)
                VisualMarkerClick(tb, args);
        }

        /// <summary>
        /// Shows VisualMarker
        /// Call this method in Draw method, when you need to show VisualMarker for your style
        /// </summary>
        protected virtual void AddVisualMarker(FastColoredTextBox tb, StyleVisualMarker marker)
        {
            tb.AddVisualMarker(marker);
        }

        /// <summary>
        /// Applies visual styles to the editor
        /// Call this method in Draw method, when you need to use different visual styles
        /// </summary>
        public static void ApplyEditorStyle(Control ctl)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return; // Do nothing in Designer

            if (ctl is FastColoredTextBox fctb)
            {
                // Default
                if (fctb.EditorStyle == EditorStyle.Default)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["Default"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Default"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["Default"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Dark
                else if (fctb.EditorStyle == EditorStyle.Dark)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["Dark"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Dark"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["Dark"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Deep Black
                else if (fctb.EditorStyle == EditorStyle.DeepBlack)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["DeepBlack"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["DeepBlack"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["DeepBlack"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Light
                else if (fctb.EditorStyle == EditorStyle.Light)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["Light"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Light"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["Light"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Khaki
                else if (fctb.EditorStyle == EditorStyle.Khaki)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["Khaki"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Khaki"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["Khaki"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Navajo Red
                else if (fctb.EditorStyle == EditorStyle.NavajoRed)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["NavajoRed"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["NavajoRed"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["NavajoRed"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: 120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Ruby Blue Light
                else if (fctb.EditorStyle == EditorStyle.RubyBlueLight)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["RubyBlueLight"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["RubyBlueLight"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["RubyBlueLight"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Ruby Blue Dark
                else if (fctb.EditorStyle == EditorStyle.RubyBlueDark)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["RubyBlueDark"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["RubyBlueDark"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["RubyBlueDark"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Twilight
                else if (fctb.EditorStyle == EditorStyle.Twilight)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["Twilight"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Twilight"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["Twilight"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Custom1
                else if (fctb.EditorStyle == EditorStyle.Custom1)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["Custom1"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Custom1"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["Custom1"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Custom2
                else if (fctb.EditorStyle == EditorStyle.Custom2)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["Custom2"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Custom2"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["Custom2"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }
                // Custom3
                else if (fctb.EditorStyle == EditorStyle.Custom3)
                {
                    fctb.BackColor = EditorStyleColors.EditorBackColor["Custom3"];
                    fctb.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Custom3"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                    fctb.SelectionColor = EditorStyleColors.EditorSelectionColor["Custom3"];
                    fctb.BorderStyle = BorderStyle.None;

                    SetGutterStyle(fctb, foreColorShift: -120, backColorShift: -40); // Set gutter style for light theme  
                }

                if (ColorLuminance(fctb.BackColor) > ColorHelper.LuminanceThreshold)
                {
                    fctb.ForeColor = fctb.ForeColor == Color.White ? Color.Black : fctb.ForeColor; // Set text color conditionally to black for light backgrounds  
                }
                else
                {
                    fctb.ForeColor = fctb.ForeColor == Color.Black ? Color.White : fctb.ForeColor; // Set text color conditionally to white for dark backgrounds  
                }

                fctb.Invalidate(); // Force a repaint if needed.

                AdjustLeftPaddingBasedOnCharacterWidth(fctb);
            }
            else if (ctl is DocumentMap dm)
            {
                if (dm.EditorStyle == EditorStyle.Default)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["Default"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Default"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.Light)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["Light"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Light"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.Khaki)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["Khaki"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Khaki"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.NavajoRed)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["NavajoRed"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["NavajoRed"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.RubyBlueLight)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["RubyBlueLight"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["RubyBlueLight"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.RubyBlueDark)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["RubyBlueDark"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["RubyBlueDark"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.Twilight)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["Twilight"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Twilight"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.Dark)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["Dark"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Dark"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.DeepBlack)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["DeepBlack"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["DeepBlack"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.Custom1)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["Custom1"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Custom1"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.Custom2)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["Custom2"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Custom2"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }
                else if (dm.EditorStyle == EditorStyle.Custom3)
                {
                    dm.BackColor = EditorStyleColors.EditorBackColor["Custom3"];
                    dm.ForeColor = ColorHelper.ColorLuminance(EditorStyleColors.EditorBackColor["Custom3"]) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;
                }

                dm.Invalidate(); // Force a repaint if needed.
            }
            else
            {
                return;
            }
        }

        //private static Language GetFastColoredTextBoxLanguage(FastColoredTextBox fctb)
        //{
        //    return fctb.findForm
        //}


        /// <summary>
        /// Adjusts the <c>LeftPadding</c> of a <see cref="FastColoredTextBox"/> control based on the character width,
        /// if it hasn't been set explicitly. Also synchronizes the <c>PaddingBackColor</c> with the control's <c>BackColor</c>.
        /// </summary>
        /// <param name="fctb">The <see cref="FastColoredTextBox"/> control to adjust.</param>
        public static void AdjustLeftPaddingBasedOnCharacterWidth(FastColoredTextBox fctb)
        {
            if (fctb.LeftPadding == 0)
                fctb.LeftPadding = GetCharacterWidth(fctb);

            fctb.Invalidate(); // Force a repaint if needed.
        }

        /// <summary>
        /// Calculates the perceived luminance of a color on a scale from 0 (darkest) to 255 (lightest),
        /// based on the standard luminance formula using human visual sensitivity.
        /// </summary>
        /// <param name="color">The color to evaluate.</param>
        /// <returns>A value between 0 and 255 representing the perceived brightness of the color.</returns>
        public static int ColorLuminance(Color color)
        {
            double luminance = (0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B);
            return (int)Math.Round(luminance);
        }

        /// <summary>
        /// Gets the width of a single character for various text controls.
        /// Supports FastColoredTextBox, TextBox, and RichTextBox.
        /// </summary>
        /// <param name="control">The text control (FastColoredTextBox, TextBox, or RichTextBox)</param>
        /// <returns>Width in pixels of one character</returns>
        private static int GetCharacterWidth(Control control)
        {
            try
            {
                // Validate that it's a supported text control
                if (!(control is FastColoredTextBox || control is TextBox || control is RichTextBox))
                {
                    throw new ArgumentException($"Control type '{control.GetType().Name}' is not supported. Only FastColoredTextBox, TextBox, and RichTextBox are supported.");
                }

                // Method 1: Try FastColoredTextBox's CharWidth first (if it's FCTB)
                if (control is FastColoredTextBox fctb)
                {
                    if (fctb.CharWidth > 0)
                    {
                        return fctb.CharWidth;
                    }
                }

                // Method 2: Measure average width of common characters using Graphics
                using (Graphics g = control.CreateGraphics())
                {
                    string testChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    float totalWidth = g.MeasureString(testChars, control.Font).Width;
                    float avgWidth = totalWidth / testChars.Length;
                    return (int)Math.Ceiling(avgWidth);
                }
            }
            catch
            {
                // Final fallback based on font size
                return Math.Max((int)(control.Font.Size * 0.6), 8);
            }
        }

        private static void SetGutterStyle(FastColoredTextBox fctb, int foreColorShift, int backColorShift)
        {
            fctb.LineNumberColor = ShiftColor(fctb.ForeColor, foreColorShift);     // Line number text
            fctb.IndentBackColor = ShiftColor(fctb.BackColor, backColorShift);     // Line number margin background
            fctb.ServiceLinesColor = ShiftColor(fctb.BackColor, backColorShift);   // Ruler/separator (e.g., line guides)
        }

        /// <summary>
        /// Shifts the RGB values of the input color by a specified amount, clamping between 0 and 255.
        /// </summary>
        /// <param name="color">The original color to shift.</param>
        /// <param name="shiftAmount">The amount to shift the RGB components by (positive or negative).</param>
        /// <returns>A new Color with shifted RGB values.</returns>
        private static Color ShiftColor(Color color, int shiftAmount)
        {
            int r = Clamp(color.R + shiftAmount, 0, 255);
            int g = Clamp(color.G + shiftAmount, 0, 255);
            int b = Clamp(color.B + shiftAmount, 0, 255);

            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// Clamps an integer value to ensure it stays within the specified minimum and maximum bounds.
        /// </summary>
        /// <param name="value">The value to be clamped.</param>
        /// <param name="min">The minimum allowable value.</param>
        /// <param name="max">The maximum allowable value.</param>
        /// <returns>
        /// The clamped value: 
        /// - Returns <paramref name="min"/> if <paramref name="value"/> is less than <paramref name="min"/>.
        /// - Returns <paramref name="max"/> if <paramref name="value"/> is greater than <paramref name="max"/>.
        /// - Otherwise, returns <paramref name="value"/>.
        /// </returns>
        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        public static Size GetSizeOfRange(Range range)
        {
            return new Size((range.End.iChar - range.Start.iChar) * range.tb.CharWidth, range.tb.CharHeight);
        }

        public static GraphicsPath GetRoundedRectangle(Rectangle rect, int d)
        {
            GraphicsPath gp = new GraphicsPath();

            gp.AddArc(rect.X, rect.Y, d, d, 180, 90);
            gp.AddArc(rect.X + rect.Width - d, rect.Y, d, d, 270, 90);
            gp.AddArc(rect.X + rect.Width - d, rect.Y + rect.Height - d, d, d, 0, 90);
            gp.AddArc(rect.X, rect.Y + rect.Height - d, d, d, 90, 90);
            gp.AddLine(rect.X, rect.Y + rect.Height - d, rect.X, rect.Y + d / 2);

            return gp;
        }

        public virtual void Dispose()
        {
            ;
        }

        /// <summary>
        /// Returns CSS for export to HTML
        /// </summary>
        /// <returns></returns>
        public virtual string GetCSS()
        {
            return "";
        }

        /// <summary>
        /// Returns RTF descriptor for export to RTF
        /// </summary>
        /// <returns></returns>
        public virtual RTFStyleDescriptor GetRTF()
        {
            return new RTFStyleDescriptor();
        }
    }

    /// <summary>
    /// Style for chars rendering
    /// This renderer can draws chars, with defined fore and back colors
    /// </summary>
    public class TextStyle : Style
    {
        public Brush ForeBrush { get; set; }
        public Brush BackgroundBrush { get; set; }
        public FontStyle FontStyle { get; set; }
        public StringFormat stringFormat;

        // Added CustomFont property.
        public Font CustomFont { get; set; }

        public TextStyle(Brush foreBrush, Brush backgroundBrush, FontStyle fontStyle)
        {
            this.ForeBrush = foreBrush;
            this.BackgroundBrush = backgroundBrush;
            this.FontStyle = fontStyle;
            stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            // Draw background if specified.
            if (BackgroundBrush != null)
            {
                gr.FillRectangle(BackgroundBrush,
                    position.X,
                    position.Y,
                    (range.End.iChar - range.Start.iChar) * range.tb.CharWidth,
                    range.tb.CharHeight);
            }

            // Use CustomFont if provided; otherwise, create a new font from the control's default.
            using (var f = new Font(CustomFont ?? range.tb.Font, FontStyle))
            {
                Line line = range.tb[range.Start.iLine];
                float dx = range.tb.CharWidth;
                float y = position.Y + range.tb.LineInterval / 2;
                float x = position.X - range.tb.CharWidth / 3;

                if (ForeBrush == null)
                    ForeBrush = new SolidBrush(range.tb.ForeColor);

                if (range.tb.ImeAllowed)
                {
                    // IME mode: draw each character with scaling if needed.
                    for (int i = range.Start.iChar; i < range.End.iChar; i++)
                    {
                        SizeF size = FastColoredTextBox.GetCharSize(f, line[i].c);

                        var gs = gr.Save();
                        float k = size.Width > range.tb.CharWidth + 1 ? range.tb.CharWidth / size.Width : 1;
                        gr.TranslateTransform(x, y + (1 - k) * range.tb.CharHeight / 2);
                        gr.ScaleTransform(k, (float)Math.Sqrt(k));
                        gr.DrawString(line[i].c.ToString(), f, ForeBrush, 0, 0, stringFormat);
                        gr.Restore(gs);
                        x += dx;
                    }
                }
                else
                {
                    // Classic mode: draw each character.
                    for (int i = range.Start.iChar; i < range.End.iChar; i++)
                    {
                        gr.DrawString(line[i].c.ToString(), f, ForeBrush, x, y, stringFormat);
                        x += dx;
                    }
                }
            }
        }

        public override string GetCSS()
        {
            string result = "";

            if (BackgroundBrush is SolidBrush)
            {
                var s = ExportToHTML.GetColorAsString((BackgroundBrush as SolidBrush).Color);
                if (s != "")
                    result += "background-color:" + s + ";";
            }
            if (ForeBrush is SolidBrush)
            {
                var s = ExportToHTML.GetColorAsString((ForeBrush as SolidBrush).Color);
                if (s != "")
                    result += "color:" + s + ";";
            }
            if ((FontStyle & FontStyle.Bold) != 0)
                result += "font-weight:bold;";
            if ((FontStyle & FontStyle.Italic) != 0)
                result += "font-style:oblique;";
            if ((FontStyle & FontStyle.Strikeout) != 0)
                result += "text-decoration:line-through;";
            if ((FontStyle & FontStyle.Underline) != 0)
                result += "text-decoration:underline;";

            return result;
        }

        public override RTFStyleDescriptor GetRTF()
        {
            var result = new RTFStyleDescriptor();

            if (BackgroundBrush is SolidBrush)
                result.BackColor = (BackgroundBrush as SolidBrush).Color;

            if (ForeBrush is SolidBrush)
                result.ForeColor = (ForeBrush as SolidBrush).Color;

            if ((FontStyle & FontStyle.Bold) != 0)
                result.AdditionalTags += @"\b";
            if ((FontStyle & FontStyle.Italic) != 0)
                result.AdditionalTags += @"\i";
            if ((FontStyle & FontStyle.Strikeout) != 0)
                result.AdditionalTags += @"\strike";
            if ((FontStyle & FontStyle.Underline) != 0)
                result.AdditionalTags += @"\ul";

            return result;
        }
    }

    /// <summary>
    /// Renderer for folded block
    /// </summary>
    public class FoldedBlockStyle : TextStyle
    {
        public FoldedBlockStyle(Brush foreBrush, Brush backgroundBrush, FontStyle fontStyle) :
            base(foreBrush, backgroundBrush, fontStyle)
        {
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            if (range.End.iChar > range.Start.iChar)
            {
                base.Draw(gr, position, range);

                int firstNonSpaceSymbolX = position.X;

                //find first non space symbol
                for (int i = range.Start.iChar; i < range.End.iChar; i++)
                    if (range.tb[range.Start.iLine][i].c != ' ')
                        break;
                    else
                        firstNonSpaceSymbolX += range.tb.CharWidth;

                //create marker
                range.tb.AddVisualMarker(new FoldedAreaMarker(range.Start.iLine, new Rectangle(firstNonSpaceSymbolX, position.Y, position.X + (range.End.iChar - range.Start.iChar) * range.tb.CharWidth - firstNonSpaceSymbolX, range.tb.CharHeight)));
            }
            else
            {
                //draw '...'
                using (Font f = new Font(range.tb.Font, FontStyle))
                    gr.DrawString("...", f, ForeBrush, range.tb.LeftIndent, position.Y - 2);
                //create marker
                range.tb.AddVisualMarker(new FoldedAreaMarker(range.Start.iLine, new Rectangle(range.tb.LeftIndent + 2, position.Y, 2 * range.tb.CharHeight, range.tb.CharHeight)));
            }
        }
    }

    /// <summary>
    /// Renderer for selected area
    /// </summary>
    public class SelectionStyle : Style
    {
        public Brush BackgroundBrush { get; set; }
        public Brush ForegroundBrush { get; private set; }

        public override bool IsExportable
        {
            get { return false; }
            set { }
        }

        public SelectionStyle(Brush backgroundBrush, Brush foregroundBrush = null)
        {
            this.BackgroundBrush = backgroundBrush;
            this.ForegroundBrush = foregroundBrush;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //draw background
            if (BackgroundBrush != null)
            {
                gr.SmoothingMode = SmoothingMode.None;
                var rect = new Rectangle(position.X, position.Y, (range.End.iChar - range.Start.iChar) * range.tb.CharWidth, range.tb.CharHeight);
                if (rect.Width == 0)
                    return;
                gr.FillRectangle(BackgroundBrush, rect);
                //
                if (ForegroundBrush != null)
                {
                    //draw text
                    gr.SmoothingMode = SmoothingMode.AntiAlias;

                    var r = new Range(range.tb, range.Start.iChar, range.Start.iLine,
                                      Math.Min(range.tb[range.End.iLine].Count, range.End.iChar), range.End.iLine);
                    using (var style = new TextStyle(ForegroundBrush, null, FontStyle.Regular))
                        style.Draw(gr, new Point(position.X, position.Y - 1), r);
                }
            }
        }
    }

    /// <summary>
    /// Marker style
    /// Draws background color for text
    /// </summary>
    public class MarkerStyle : Style
    {
        public Brush BackgroundBrush { get; set; }

        public MarkerStyle(Brush backgroundBrush)
        {
            this.BackgroundBrush = backgroundBrush;
            IsExportable = true;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //draw background
            if (BackgroundBrush != null)
            {
                Rectangle rect = new Rectangle(position.X, position.Y, (range.End.iChar - range.Start.iChar) * range.tb.CharWidth, range.tb.CharHeight);
                if (rect.Width == 0)
                    return;
                gr.FillRectangle(BackgroundBrush, rect);
            }
        }

        public override string GetCSS()
        {
            string result = "";

            if (BackgroundBrush is SolidBrush)
            {
                var s = ExportToHTML.GetColorAsString((BackgroundBrush as SolidBrush).Color);
                if (s != "")
                    result += "background-color:" + s + ";";
            }

            return result;
        }
    }

    /// <summary>
    /// Draws small rectangle for popup menu
    /// </summary>
    public class ShortcutStyle : Style
    {
        public Pen borderPen;

        public ShortcutStyle(Pen borderPen)
        {
            this.borderPen = borderPen;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //get last char coordinates
            Point p = range.tb.PlaceToPoint(range.End);
            //draw small square under char
            Rectangle rect = new Rectangle(p.X - 5, p.Y + range.tb.CharHeight - 2, 4, 3);
            gr.FillPath(Brushes.White, GetRoundedRectangle(rect, 1));
            gr.DrawPath(borderPen, GetRoundedRectangle(rect, 1));
            //add visual marker for handle mouse events
            AddVisualMarker(range.tb, new StyleVisualMarker(new Rectangle(p.X - range.tb.CharWidth, p.Y, range.tb.CharWidth, range.tb.CharHeight), this));
        }
    }

    /// <summary>
    /// This style draws a wavy line below a given text range.
    /// </summary>
    /// <remarks>Thanks for Yallie</remarks>
    public class WavyLineStyle : Style
    {
        private Pen Pen { get; set; }

        public WavyLineStyle(int alpha, Color color)
        {
            Pen = new Pen(Color.FromArgb(alpha, color));
        }

        public override void Draw(Graphics gr, Point pos, Range range)
        {
            var size = GetSizeOfRange(range);
            var start = new Point(pos.X, pos.Y + size.Height - 1);
            var end = new Point(pos.X + size.Width, pos.Y + size.Height - 1);
            DrawWavyLine(gr, start, end);
        }

        private void DrawWavyLine(Graphics graphics, Point start, Point end)
        {
            if (end.X - start.X < 2)
            {
                graphics.DrawLine(Pen, start, end);
                return;
            }

            var offset = -1;
            var points = new List<Point>();

            for (int i = start.X; i <= end.X; i += 2)
            {
                points.Add(new Point(i, start.Y + offset));
                offset = -offset;
            }

            graphics.DrawLines(Pen, points.ToArray());
        }

        public override void Dispose()
        {
            base.Dispose();

            if (Pen != null)
                Pen.Dispose();
        }
    }

    /// <summary>
    /// This style is used to mark range of text as ReadOnly block
    /// </summary>
    /// <remarks>You can inherite this style to add visual effects of readonly text</remarks>
    public class ReadOnlyStyle : Style
    {
        public ReadOnlyStyle()
        {
            IsExportable = false;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //
        }
    }
}
