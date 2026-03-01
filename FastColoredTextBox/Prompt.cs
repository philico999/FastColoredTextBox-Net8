using System;
using System.Drawing;
using System.Windows.Forms;

namespace FastColoredTextBoxNS
{
    public static class Prompt
    {
        /// <summary>
        /// Displays a modal input dialog with optional appearance customization.
        /// The text box and buttons will be rendered 20% lighter than the dialog background.
        /// </summary>
        /// <param name="text">The prompt text.</param>
        /// <param name="caption">The dialog title.</param>
        /// <param name="defaultValue">Initial text in the input box.</param>
        /// <param name="dialogBackColor">Optional form background color.</param>
        /// <param name="dialogForeColor">Optional form foreground color.</param>
        /// <param name="buttonBackColor">Optional buttons' background color (overridden by auto-lighten).</param>
        /// <param name="buttonForeColor">Optional buttons' foreground color.</param>
        /// <param name="location">Optional dialog screen location.</param>
        /// <returns>User input or empty string if cancelled.</returns>
        public static string Show(
            string text,
            string caption,
            string defaultValue = "",
            Color? dialogBackColor = null,
            Color? dialogForeColor = null,
            Color? buttonBackColor = null,
            Color? buttonForeColor = null,
            Point? location = null)
        {
            using var form = new Form
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                Text = caption
            };

            if (location.HasValue)
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Location = location.Value;
            }
            else
            {
                form.StartPosition = FormStartPosition.CenterParent;
            }

            if (dialogBackColor.HasValue)
                form.BackColor = dialogBackColor.Value;
            if (dialogForeColor.HasValue)
                form.ForeColor = dialogForeColor.Value;

            // Calculate a 20% lighter color from the form background
            var baseColor = form.BackColor;
            Color lightColor = Color.FromArgb(
                baseColor.A,
                Math.Min(255, (int)(baseColor.R + (255 - baseColor.R) * 0.2)),
                Math.Min(255, (int)(baseColor.G + (255 - baseColor.G) * 0.2)),
                Math.Min(255, (int)(baseColor.B + (255 - baseColor.B) * 0.2))
            );

            // Label
            var lbl = new Label
            {
                Left = 10,
                Top = 10,
                Text = text,
                AutoSize = true,
                BackColor = dialogBackColor ?? form.BackColor,
                ForeColor = dialogForeColor ?? form.ForeColor
            };

            // TextBox
            var txt = new TextBox
            {
                Left = 10,
                Top = lbl.Bottom + 5,
                Width = 360,
                Text = defaultValue,
                BackColor = lightColor,
                ForeColor = dialogForeColor ?? form.ForeColor
            };

            // OK button
            var btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Left = 220,
                Width = 75,
                Height = 32,
                Top = txt.Bottom + 10,
                BackColor = lightColor,
                ForeColor = buttonForeColor ?? form.ForeColor
            };

            // Cancel button
            var btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Left = 300,
                Width = 75,
                Height = 32,
                Top = txt.Bottom + 10,
                BackColor = lightColor,
                ForeColor = buttonForeColor ?? form.ForeColor
            };

            form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            return form.ShowDialog() == DialogResult.OK ? txt.Text : string.Empty;
        }
    }
}
