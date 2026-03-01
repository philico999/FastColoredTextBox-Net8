using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace FastColoredTextBoxNS
{

    /// <summary>
    /// Provides helper methods for UI-related operations.
    /// </summary>
    internal static class UIUtils
    {
        /// <summary>
        /// Creates a new font with the specified size, maintaining the original font family.
        /// </summary>
        /// <param name="font">The original font whose family name will be used for the new font.</param>
        /// <param name="size">The size for the new font.</param>
        /// <returns>A new Font object with the specified size and the same family name as the original font.</returns>
        public static Font SetFont(Font font, float size)
        {
            return new Font(font.Name, size);
        }

        /// <summary>
        /// Converts a <see cref="MessageBoxButtons"/> value to an array of corresponding <see cref="DialogResult"/> values.
        /// </summary>
        public static DialogResult[] MessageBoxButtonToDialogResult(MessageBoxButtons button)
        {
            switch (button)
            {
                case MessageBoxButtons.OK:
                    return new[] { DialogResult.OK };
                case MessageBoxButtons.YesNo:
                    return new[] { DialogResult.Yes, DialogResult.No };
                case MessageBoxButtons.YesNoCancel:
                    return new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel };
                case MessageBoxButtons.RetryCancel:
                    return new[] { DialogResult.Retry, DialogResult.Cancel };
                case MessageBoxButtons.OKCancel:
                    return new[] { DialogResult.OK, DialogResult.Cancel };
                default:
                    return new[] { DialogResult.None };
            }
        }

        /// <summary>
        /// Converts a textual button spec to an array of corresponding <see cref="DialogResult"/> values.
        /// </summary>
        public static DialogResult[] MessageBoxButtonToDialogResult(string button)
        {
            if (button == null) return new[] { DialogResult.None };
            switch (button.ToLower())
            {
                case "ok":
                    return new[] { DialogResult.OK };
                case "yesno":
                case "noyes":
                    return new[] { DialogResult.Yes, DialogResult.No };
                case "yesnocancel":
                case "noyescancel":
                case "cancelyesno":
                case "cancelnoyes":
                    return new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel };
                case "okcancel":
                case "cancelok":
                    return new[] { DialogResult.OK, DialogResult.Cancel };
                case "retrycancel":
                case "cancelretry":
                    return new[] { DialogResult.Retry, DialogResult.Cancel };
                default:
                    return new[] { DialogResult.None };
            }
        }

        /// <summary>
        /// Converts an array of <see cref="DialogResult"/> values to the closest matching <see cref="MessageBoxButtons"/> value.
        /// </summary>
        public static MessageBoxButtons DialogResultToMessageBoxButton(DialogResult[] results)
        {
            if (results == null) return (MessageBoxButtons)(-1);

            if (results.SequenceEqual(new[] { DialogResult.OK }))
                return MessageBoxButtons.OK;
            else if (results.SequenceEqual(new[] { DialogResult.Yes, DialogResult.No }))
                return MessageBoxButtons.YesNo;
            else if (results.SequenceEqual(new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel }))
                return MessageBoxButtons.YesNoCancel;
            else if (results.SequenceEqual(new[] { DialogResult.Retry, DialogResult.Cancel }))
                return MessageBoxButtons.RetryCancel;
            else
                return (MessageBoxButtons)(-1); // Undefined mapping
        }

        /// <summary>
        /// Converts an array of strings to the closest matching <see cref="MessageBoxButtons"/> value.
        /// </summary>
        public static MessageBoxButtons DialogResultToMessageBoxButton(string[] results)
        {
            // NOTE: This mirrors the VB order/logic you had. Adjust precedence if desired.
            if (ContainsIgnoreCase(results, "ok"))
                return MessageBoxButtons.OK;
            else if (ContainsIgnoreCase(results, "yes") || ContainsIgnoreCase(results, "no"))
                return MessageBoxButtons.YesNo;
            else if (ContainsIgnoreCase(results, "yes") || ContainsIgnoreCase(results, "no") || ContainsIgnoreCase(results, "cancel"))
                return MessageBoxButtons.YesNoCancel;
            else if (ContainsIgnoreCase(results, "ok") || ContainsIgnoreCase(results, "cancel"))
                return MessageBoxButtons.OKCancel;
            else if (ContainsIgnoreCase(results, "retry") || ContainsIgnoreCase(results, "cancel"))
                return MessageBoxButtons.RetryCancel;
            else
                return (MessageBoxButtons)(-1); // Undefined mapping
        }

        /// <summary>
        /// Calculates optimal window dimensions for displaying text based on readability principles.
        /// </summary>
        public static Size GetWindowSize(
            string expression,
            Font textFont,
            int minWidth = 72,
            int minHeight = 48,
            int maxWidth = 720,
            int maxHeight = 480)
        {
            // Constants for optimal reading
            const int HORIZONTAL_PADDING = 20;  // Left and right padding
            const int VERTICAL_PADDING = 20;    // Top and bottom padding
            const int OPTIMAL_CHARS_PER_LINE = 65;  // Optimal characters per line
            const int MIN_CHARS_PER_LINE = 45;      // Min characters per line
            const int MAX_CHARS_PER_LINE = 85;      // Max characters per line
            const float LINE_SPACING_MULTIPLIER = 1.2f; // Extra spacing between lines

            var bestSize = new Size(minWidth, minHeight);

            if (string.IsNullOrEmpty(expression))
                return bestSize;

            // Font metrics
            float charWidth;
            float lineHeight;
            using (var bmp = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bmp))
            {
                string sampleText = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                SizeF sampleSize = g.MeasureString(sampleText, textFont);
                charWidth = sampleSize.Width / sampleText.Length;

                float fontHeight = textFont.GetHeight(g);
                lineHeight = fontHeight * LINE_SPACING_MULTIPLIER;
            }

            // Words
            var words = expression
                .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
                return bestSize;

            int totalChars = words.Sum(w => w.Length);
            double avgWordLength = (double)totalChars / words.Length;

            int optimalCharsPerLine = OPTIMAL_CHARS_PER_LINE;
            if (avgWordLength < 4)
                optimalCharsPerLine = Math.Max(MIN_CHARS_PER_LINE, optimalCharsPerLine - 10);
            else if (avgWordLength > 8)
                optimalCharsPerLine = Math.Min(MAX_CHARS_PER_LINE, optimalCharsPerLine + 10);

            int requiredWidth = (int)((optimalCharsPerLine * charWidth) + HORIZONTAL_PADDING);

            int estimatedWordsPerLine = Math.Max(1, (int)(optimalCharsPerLine / (avgWordLength + 1))); // +1 for space
            int estimatedLines = (int)Math.Ceiling((double)words.Length / estimatedWordsPerLine);

            int requiredHeight = (int)((estimatedLines * lineHeight) + VERTICAL_PADDING);

            int finalWidth = Math.Max(minWidth, Math.Min(maxWidth, requiredWidth));
            int finalHeight = Math.Max(minHeight, Math.Min(maxHeight, requiredHeight));

            // If constrained by maxWidth, adjust height for wrapping
            if (requiredWidth > maxWidth)
            {
                int actualCharsPerLine = (int)((maxWidth - HORIZONTAL_PADDING) / charWidth);
                actualCharsPerLine = Math.Max(MIN_CHARS_PER_LINE, actualCharsPerLine);

                int actualWordsPerLine = Math.Max(1, (int)(actualCharsPerLine / (avgWordLength + 1)));
                int actualLines = (int)Math.Ceiling((double)words.Length / actualWordsPerLine);

                int adjustedHeight = (int)((actualLines * lineHeight) + VERTICAL_PADDING);
                finalHeight = Math.Max(finalHeight, Math.Min(maxHeight, adjustedHeight));
            }

            // For longer text, refine height using actual measurement at the chosen width
            if (words.Length > 20)
            {
                using (var bmp = new Bitmap(1, 1))
                using (var g = Graphics.FromImage(bmp))
                {
                    int testWidth = finalWidth - HORIZONTAL_PADDING;
                    SizeF measuredSize = g.MeasureString(expression, textFont, testWidth);
                    int measuredHeight = (int)(measuredSize.Height + VERTICAL_PADDING);
                    finalHeight = Math.Max(finalHeight, Math.Min(maxHeight, measuredHeight));
                }
            }

            return new Size(finalWidth, finalHeight);
        }

        /// <summary>
        /// Centers the specified form relative to a reference control's screen bounds, with screen-safety adjustments.
        /// </summary>
        public static void CenterForm(Form frm, Control referenceControl, int yShift = 0, int xShift = 0)
        {
            if (frm == null) return;

            if (referenceControl != null && referenceControl.IsHandleCreated)
            {
                // Get the control's bounds in screen coordinates
                Rectangle controlBounds = referenceControl.RectangleToScreen(referenceControl.ClientRectangle);

                frm.Top = controlBounds.Top + (controlBounds.Height - frm.Height) / 2 + yShift;
                frm.Left = controlBounds.Left + (controlBounds.Width - frm.Width) / 2 + xShift;
                frm.StartPosition = FormStartPosition.Manual;
            }
            else
            {
                // Fall back to screen center if no valid reference control
                frm.Location = GetScreenLocation();
                frm.StartPosition = FormStartPosition.Manual;
            }

            // Off-screen adjustments
            Rectangle workingArea = Screen.FromControl(frm).WorkingArea;

            if (frm.Right > workingArea.Right)
                frm.Left = workingArea.Right - frm.Width;
            if (frm.Left < workingArea.Left)
                frm.Left = workingArea.Left;
            if (frm.Bottom > workingArea.Bottom)
                frm.Top = workingArea.Bottom - frm.Height;
            if (frm.Top < workingArea.Top)
                frm.Top = workingArea.Top;

            // Apply shifts after bounds checking (allows intentional partial off-screen if desired)
            if (xShift != 0 || yShift != 0)
            {
                frm.Left += xShift;
                frm.Top += yShift;
            }
        }

        /// <summary>
        /// Centers the specified form relative to a base form (or best guess), with screen-safety adjustments.
        /// </summary>
        public static void CenterForm(Form frm, Form baseForm = null, int yShift = 0, int xShift = 0)
        {
            if (frm == null) return;

            // If no owner is set, try to find the active form to use as owner
            if (baseForm == null)
            {
                try
                {
                    baseForm = Control.FromHandle(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle) as Form;
                }
                catch { }
            }

            if (baseForm == null)
            {
                try { baseForm = Form.ActiveForm; } catch { }
            }

            if (baseForm == null)
            {
                try
                {
                    foreach (Form openForm in Application.OpenForms)
                    {
                        if (openForm.Visible && openForm != frm)
                        {
                            baseForm = openForm;
                            break;
                        }
                    }
                }
                catch { }
            }

            if (baseForm == null)
            {
                try
                {
                    foreach (Form openForm in Application.OpenForms)
                    {
                        if (openForm.TopMost && openForm != frm)
                        {
                            baseForm = openForm;
                            break;
                        }
                    }
                }
                catch { }
            }

            if (baseForm != null)
            {
                if (baseForm.MdiParent != null)
                {
                    frm.Top = baseForm.MdiParent.Top + (baseForm.MdiParent.Height - frm.Height) / 2 + yShift;
                    frm.Left = baseForm.MdiParent.Left + (baseForm.MdiParent.Width - frm.Width) / 2 + xShift;
                }
                else
                {
                    frm.Top = baseForm.Top + (baseForm.Height - frm.Height) / 2 + yShift;
                    frm.Left = baseForm.Left + (baseForm.Width - frm.Width) / 2 + xShift;
                }
            }
            else
            {
                frm.Location = GetScreenLocation();
                frm.StartPosition = FormStartPosition.Manual;
            }

            // Off-screen adjustments
            Rectangle workingArea = Screen.FromControl(frm).WorkingArea;

            if (frm.Right > workingArea.Right)
                frm.Left = workingArea.Right - frm.Width + xShift;

            if (frm.Left < workingArea.Left)
                frm.Left = workingArea.Left + xShift;

            if (frm.Bottom > workingArea.Bottom)
                frm.Top = workingArea.Bottom - frm.Height + yShift;

            if (frm.Top < workingArea.Top)
                frm.Top = workingArea.Top + yShift;
        }

        /// <summary>
        /// Gets the center point of the screen that currently contains the mouse cursor.
        /// </summary>
        /// <returns>
        /// A <see cref="Point"/> representing the center coordinates of the screen containing the cursor.
        /// If the cursor is not within any screen bounds, returns the center of the primary screen.
        /// </returns>
        /// <remarks>
        /// This method iterates through all available screens to find the one containing the current cursor position.
        /// Once found, it calculates and returns the center point of that screen. This is useful for positioning
        /// dialogs or windows at the center of the screen where the user is currently working in multi-monitor setups.
        /// </remarks>
        public static Point GetScreenLocation()
        {
            // First get all the screens 
            int x = Screen.PrimaryScreen.Bounds.Width / 2;
            int y = Screen.PrimaryScreen.Bounds.Height / 2;
            Rectangle ret;

            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                ret = Screen.AllScreens[i].Bounds;
                if (ret.Contains(Cursor.Position))
                {
                    if (Cursor.Position.Y > ret.Y)
                    {
                        y = ret.Y + (ret.Height / 2);
                    }
                    else
                    {
                        y = ret.Height / 2;
                    }

                    if (Cursor.Position.X > ret.X)
                    {
                        x = ret.X + (ret.Width / 2);
                    }
                    else
                    {
                        x = ret.Width / 2;
                    }

                    break;
                }
            }

            return new Point(x, y);
        }

        /// <summary>
        /// Sets the fixed panel of a SplitContainer control and optionally sets the splitter distance.
        /// </summary>
        public static void SplitContainer_SetFixedPanel(SplitContainer control, FixedPanel panel, int splitterDistance = 0)
        {
            control.SplitterDistance = splitterDistance > 0 ? splitterDistance : control.SplitterDistance;
            control.FixedPanel = panel;
        }

        /// <summary>
        /// Clears the fixed panel of a SplitContainer control.
        /// </summary>
        public static void SplitContainer_ClearFixedPanel(SplitContainer control)
        {
            control.FixedPanel = FixedPanel.None;
        }

        /// <summary>
        /// Enables double buffering for a control to reduce flicker.
        /// </summary>
        public static void EnableDoubleBuffering(Control ctrl)
        {
            var doubleBufferProperty = ctrl.GetType().GetProperty(
                "DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (doubleBufferProperty != null)
            {
                doubleBufferProperty.SetValue(ctrl, true, null);
            }
        }

        /// <summary>
        /// Intelligently resizes a target control and its parent form to accommodate content while respecting constraints.
        /// Call from the dialog’s Shown event.
        /// </summary>
        public static void SmartResize(
            Control target,
            Size? size = null,
            Size? minSize = null,
            Size? maxSize = null)
        {
            if (target == null) return;
            Form frm = target.FindForm();
            if (frm == null) return;

            // Ensure the form can shrink or expand to specified sizes if supplied
            frm.MinimumSize = minSize ?? Size.Empty;
            frm.MaximumSize = maxSize ?? Size.Empty;

            // Ensure the form is set to specified size
            frm.Size = size ?? frm.Size;

            // --- capture & suspend layout so anchors/docking don't fight us ---
            var savedAnchor = target.Anchor;
            var savedDock = target.Dock;
            bool savedAutoSize = target.AutoSize;

            frm.SuspendLayout();
            target.SuspendLayout();

            // prevent layout engine from stretching the control while we resize
            target.Dock = DockStyle.None;
            target.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            target.AutoSize = false;

            try
            {
                // --------- BASELINES (use current sizes as defaults) ---------
                Size defaultCtrlSize = target.Size;
                Size defaultFormClient = frm.ClientSize;

                // --------- margins around the target control ---------
                int leftMargin = target.Left;
                int topMargin = target.Top;
                int rightMargin = frm.ClientSize.Width - (target.Left + target.Width);
                int bottomSpace = Math.Max(0, frm.ClientSize.Height - (target.Top + target.Height));

                // --------- available screen area & width cap ---------
                Rectangle wa = Screen.FromControl(frm).WorkingArea;
                int safety = 20;
                int maxCtrlWidth = Math.Min(wa.Width - (leftMargin + rightMargin) - safety, Dpi(frm, 800));
                maxCtrlWidth = Math.Max(maxCtrlWidth, defaultCtrlSize.Width);

                // --------- measure wrapped text like a Label does ---------
                Size measured = MeasureWrappedText(target, maxCtrlWidth);

                // final control size (never below defaults)
                int newCtrlW = Math.Min(maxCtrlWidth, Math.Max(defaultCtrlSize.Width, measured.Width));
                int newCtrlH = Math.Max(defaultCtrlSize.Height, measured.Height);

                // --------- compute desired client size for the form ---------
                int desiredClientW = Math.Max(defaultFormClient.Width, leftMargin + newCtrlW + rightMargin);
                int desiredClientH = Math.Max(defaultFormClient.Height, topMargin + newCtrlH + bottomSpace);

                // clamp to working area (accounting for window chrome)
                Size chrome = GetChromeThickness(frm);
                int maxClientW = wa.Width - chrome.Width - safety;
                int maxClientH = wa.Height - chrome.Height - safety;
                desiredClientW = Math.Min(desiredClientW, maxClientW);
                desiredClientH = Math.Min(desiredClientH, maxClientH);

                // --------- apply sizes ---------
                // 1) resize form client first
                frm.ClientSize = new Size(desiredClientW, desiredClientH);

                // 2) configure target for wrapping and set its size
                target.MaximumSize = new Size(maxCtrlWidth, 0); // enable wrapping
                target.Size = new Size(newCtrlW, newCtrlH);
            }
            finally
            {
                // restore original layout settings
                target.Anchor = savedAnchor;
                target.Dock = savedDock;
                target.AutoSize = savedAutoSize;

                target.ResumeLayout();
                frm.ResumeLayout(performLayout: true);
            }

            // keep the window fully visible
            EnsureFullyOnScreen(frm, Screen.FromControl(frm).WorkingArea);
        }

        // --- helpers ---

        // Measure text using the same pipeline a Label uses (GDI TextRenderer + word wrap)
        private static Size MeasureWrappedText(Control ctrl, int maxWidth)
        {
            TextFormatFlags flags =
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.EndEllipsis;

            var proposed = new Size(maxWidth, int.MaxValue);
            string text = ctrl.Text ?? string.Empty;
            Size measured = TextRenderer.MeasureText(text, ctrl.Font, proposed, flags);

            // include control padding
            int padW = ctrl.Padding.Left + ctrl.Padding.Right;
            int padH = ctrl.Padding.Top + ctrl.Padding.Bottom;
            measured.Width += padW;
            measured.Height += padH;
            return measured;
        }

        // Chrome = total window size - client size
        private static Size GetChromeThickness(Form frm)
        {
            Size outer = frm.Size;
            Size client = frm.ClientSize;
            return new Size(
                Math.Max(outer.Width - client.Width, 0),
                Math.Max(outer.Height - client.Height, 0));
        }

        // Simple DPI scaler (logical 96 → actual)
        private static int Dpi(Form frm, int value)
        {
            float dpiX = 96.0f;
            try
            {
                using (Graphics g = frm.CreateGraphics())
                {
                    dpiX = g.DpiX;
                }
            }
            catch { }
            return (int)Math.Round(value * (dpiX / 96.0f));
        }

        // Keep the form within the working area
        private static void EnsureFullyOnScreen(Form frm, Rectangle wa)
        {
            int newLeft = Math.Max(wa.Left, Math.Min(frm.Left, wa.Right - frm.Width));
            int newTop = Math.Max(wa.Top, Math.Min(frm.Top, wa.Bottom - frm.Height));
            frm.Location = new Point(newLeft, newTop);
        }

        // Case-insensitive array contains helper (replaces Nc.Contains(Of String()))
        private static bool ContainsIgnoreCase(string[] arr, string value)
        {
            if (arr == null || value == null) return false;
            return arr.Any(s => string.Equals(s, value, StringComparison.OrdinalIgnoreCase));
        }
    }

}