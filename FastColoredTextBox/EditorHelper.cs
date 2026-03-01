using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FastColoredTextBoxNS
{
    public class EditorHelper
    {
        /// <summary>
        /// Word-wraps the currently selected text in the given FastColoredTextBox at the user-specified column.
        /// </summary>
        public static void WordWrapSelectedText(FastColoredTextBox editor)
        {
            // Nothing to do if there's no selection
            if (editor == null || string.IsNullOrEmpty(editor.SelectedText))
                return;

            // Prompt for wrap column
            string positionStr = Prompt.Show("Enter position:", "Wordwrap Selection", "120", editor.FindForm()?.BackColor, editor.FindForm()?.ForeColor);

            // Validate numeric input between 1 and 1024
            if (!IsNumeric(positionStr))
                return;

            int position = ToInt(positionStr);
            if (position <= 0 || position > 1024)
                return;

            // Split the text at the given column and rejoin with newlines
            string[] chunks = SplitText(editor.SelectedText, position);
            editor.SelectedText = ArrayToStringBuilder(chunks, Environment.NewLine).ToString() + Environment.NewLine;
        }

        /// <summary>
        /// Determines whether the given value is numeric.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value is numeric; otherwise, false.</returns>
        private static bool IsNumeric(object value)
        {
            return double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture),
                System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }

        /// <summary>
        /// Converts an object to an integer.
        /// </summary>
        /// <param name="input">The object to convert.</param>
        /// <returns>The integer representation of the expression object.</returns>
        /// <remarks>
        /// This method attempts to convert the expression object to an integer. If the conversion fails, it returns the default value.
        /// </remarks>
        private static int ToInt(object input)
        {
            return IsNumeric(input) ? Convert.ToInt32(input) : default;
        }

        /// <summary>
        /// Splits the input text into an array of strings based on specified size, breaking at word boundaries when possible.
        /// </summary>
        /// <param name="expression">The input string to be split.</param>
        /// <param name="maxLength">The desired maximum length of each substring. Must be at least 1.</param>
        /// <returns>An array of strings split according to the specified size and margin of error.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified size is less than 1.</exception>
        public static string[] SplitText(string expression, int maxLength = 80)
        {
            if (string.IsNullOrEmpty(expression))
                return Array.Empty<string>();

            if (maxLength < 1)
                throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be at least 1.");

            var result = new List<string>();
            var words = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                // If adding this word would exceed the limit
                if (currentLine.Length > 0 && currentLine.Length + 1 + word.Length > maxLength)
                {
                    // Add current line to result and start new line
                    result.Add(currentLine.ToString());
                    currentLine.Clear();
                }

                // Add word to current line
                if (currentLine.Length > 0)
                    currentLine.Append(' ');
                currentLine.Append(word);
            }

            // Add the last line if it has content
            if (currentLine.Length > 0)
            {
                result.Add(currentLine.ToString());
            }

            return result.ToArray();
        }

        /// <summary>
        /// Builds a delimited string from an array.
        /// </summary>
        /// <param name="arr">The array to convert.</param>
        /// <param name="delimiter">The delimiters to use between elements (default is ",").</param>
        /// <param name="characterCasing">Optional character casing to apply to each element.</param>
        /// <param name="startRow">The starting index of the array (default is 0).</param>
        /// <param name="endRow">The ending index of the array (default is -1, which means the last element).</param>
        /// <param name="fifo">If true, processes elements in FIFO order; otherwise, in LIFO order (default is true).</param>
        /// <param name="sortList">If true, sorts the list before processing (default is false).</param>
        /// <param name="removeNullOrWhiteSpace">If true, ignores empty or null elements (default is false).</param>
        /// <returns>A <see cref="StringBuilder"/> containing the delimited string representation of the array.</returns>
        /// <exception cref="Exception">Thrown when the input array is of an unsupported type.</exception>
        private static StringBuilder ArrayToStringBuilder(object arr, string delimiter = ",", string characterCasing = "",
            int startRow = 0, int endRow = -1, bool fifo = true, bool sortList = false, bool removeNullOrWhiteSpace = true)
        {
            //Desc: Takes an array and returns a delimited string expression
            int i = 0;
            StringBuilder sb = new StringBuilder();

            if (arr is ArrayList)
            {
                if (startRow < 0)
                {
                    startRow = 0;
                }
                if (endRow < 0)
                {
                    endRow = ((ArrayList)arr).Count - 1;
                }

                ArrayList ar = (ArrayList)arr;

                if (sortList)
                {
                    ar.Sort();
                }

                if (fifo)
                {
                    for (i = 0; i < ar.Count; i++)
                    {
                        if (i >= startRow && i <= endRow)
                        {
                            if (!removeNullOrWhiteSpace || $"{ar[i]}".Length > 0)
                            {
                                if (i < endRow)
                                {
                                    sb.Append(characterCasing + $"{ar[i]}" + characterCasing + delimiter);
                                }
                                else
                                {
                                    sb.Append(characterCasing + $"{ar[i]}" + characterCasing);
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (i = ar.Count - 1; i >= 0; i--)
                    {
                        if (!removeNullOrWhiteSpace || $"{ar[i]}".Length > 0)
                        {
                            if (i >= startRow && i <= endRow)
                            {
                                if (i > endRow)
                                {
                                    sb.Append(characterCasing + $"{ar[i]}" + characterCasing + delimiter);
                                }
                                else
                                {
                                    sb.Append(characterCasing + $"{ar[i]}" + characterCasing);
                                }
                            }
                        }
                    }
                }

                return sb;
            }
            else if (arr is Array)
            {
                if (startRow < 0)
                {
                    startRow = 0;
                }
                if (endRow < 0)
                {
                    endRow = ((Array)arr).Length - 1;
                }

                object[] ar = ((IEnumerable)arr).Cast<object>().ToArray();

                if (fifo)
                {
                    for (i = 0; i < ar.Length; i++)
                    {
                        if (i >= startRow && i <= endRow)
                        {
                            if (!removeNullOrWhiteSpace || $"{ar[i]}".Length > 0)
                            {
                                if (i >= startRow && i <= endRow)
                                {
                                    if (i < endRow)
                                    {
                                        sb.Append(characterCasing + $"{ar[i]}" + characterCasing + delimiter);
                                    }
                                    else
                                    {
                                        sb.Append(characterCasing + $"{ar[i]}" + characterCasing);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (i = ar.Count() - 1; i >= 0; i--)
                    {
                        if (i >= startRow && i <= endRow)
                        {
                            if (!removeNullOrWhiteSpace || $"{ar[i]}".Length > 0)
                            {
                                if (i >= startRow && i <= endRow)
                                {
                                    if (i > 0)
                                    {
                                        sb.Append(characterCasing + $"{ar[i]}" + characterCasing + delimiter);
                                    }
                                    else
                                    {
                                        sb.Append(characterCasing + $"{ar[i]}" + characterCasing);
                                    }
                                }
                            }
                        }
                    }
                }

                return sb;
            }
            else if (arr == null)
            {
                return null;
            }
            else
            {
                throw new System.Exception("Unsupported Type");
            }
        }

    }
}