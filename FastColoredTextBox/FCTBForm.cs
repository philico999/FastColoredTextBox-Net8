using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FastColoredTextBoxNS
{
    public class FctbForm : Form
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        private int _cornerRadius = 12;

        internal enum DialogStyles
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

        // Define dialog style colors
        private static readonly Dictionary<string, RGB> _dialogStyleColors =
            new Dictionary<string, RGB>
            {
        { "Dark",          new RGB(33, 33, 45) },
        { "DeepBlack",     new RGB(25, 25, 28) },
        { "Default",       new RGB(50, 50, 50) },
        { "Khaki",         new RGB(46, 72, 23) },
        { "Light",         new RGB(195, 193, 206) },
        { "NavajoRed",     new RGB(28, 12, 8) },
        { "RubyBlueDark",  new RGB(26, 47, 61) },
        { "RubyBlueLight", new RGB(63, 82, 96) },
        { "Twilight",      new RGB(75, 80, 85) },
        { "Custom1",       new RGB(33, 33, 45) },
        { "Custom2",       new RGB(164, 170, 181) },
        { "Custom3",       new RGB(26, 47, 61) }
            };

        // Internal dictionary for editor background colors — can be updated at runtime
        public static readonly Dictionary<string, Color> EditorBackColor =
            new Dictionary<string, Color>
            {
        { "Dark",          Color.FromArgb(37, 37, 48) },
        { "DeepBlack",     Color.FromArgb(15, 15, 15) },
        { "Default",       Color.FromArgb(60, 60, 60) },
        { "Khaki",         Color.FromArgb(240, 255, 202) },
        { "Light",         Color.FromArgb(255, 253, 247) },
        { "NavajoRed",     Color.FromArgb(255, 248, 224) },
        { "RubyBlueDark",  Color.FromArgb(20, 30, 40) },
        { "RubyBlueLight", Color.FromArgb(223, 242, 255) },
        { "Twilight",      Color.FromArgb(205, 205, 205) },
        { "Custom1",       Color.FromArgb(37, 37, 48) },
        { "Custom2",       Color.FromArgb(255, 253, 247) },
        { "Custom3",       Color.FromArgb(20, 30, 40) }
            };


        private DialogStyles _dialogStyle = DialogStyles.Default;

        public static Color DefaultButtonColorDarkTheme { get; } = Color.FromArgb(104, 99, 214);
        public static Color DefaultButtonColorLightTheme { get; } = Color.FromArgb(0, 122, 204);
        public static Color DarkGlowColor { get; } = Color.FromArgb(127, 122, 226);
        public static Color LightGlowColor { get; } = Color.FromArgb(0, 100, 170);

        [Category("Appearance")]
        [DefaultValue(12)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                value = Math.Max(0, value);
                if (_cornerRadius != value)
                {
                    _cornerRadius = value;
                    UpdateWindowRegion();
                    Invalidate();
                }
            }
        }

        // --- Public knobs (show up in the designer) ---
        [Category("Glow"), DefaultValue(6)]
        public int GlowSize
        {
            get => _glowSize;
            set { _glowSize = Math.Max(0, value); RecalcPadding(); Invalidate(); }
        }

        [Category("Glow")]
        public Color ActiveGlow { get; set; } = Color.FromArgb(96, 140, 244);     // VS-ish blue

        [Category("Glow")]
        public Color InactiveGlow { get; set; } = Color.FromArgb(180, 180, 180);  // subtle gray

        [Category("Glow"), DefaultValue(true)]
        public bool DrawInactiveGlow { get; set; } = true;

        [Category("Glow"), DefaultValue(true)]
        public bool UseSystemHighlight { get; set; } = true;

        // --- Internals ---
        private bool _isActive;
        private int _glowSize = 1;
        private Padding _basePadding;  // whatever the form had before we added space for the glow

        public FctbForm()
        {
            // smooth, flicker-free
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // remember original padding and add space for glow
            _basePadding = base.Padding;
            RecalcPadding();

            // optional: align to system highlight to “feel VS”
            if (UseSystemHighlight)
                ActiveGlow = Color.FromArgb(ActiveGlow.A == 0 ? 255 : ActiveGlow.A, SystemColors.Highlight);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRoundedRegion();
            _isActive = Focused || ContainsFocus;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateRoundedRegion();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _isActive = true;
            Invalidate();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            _isActive = false;
            Invalidate();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            // if user changes Padding at runtime, keep our base padding up to date
            // (avoid infinite loop by only updating our record)
            _basePadding = new Padding(
                Math.Max(0, base.Padding.Left - GlowSize),
                Math.Max(0, base.Padding.Top - GlowSize),
                Math.Max(0, base.Padding.Right - GlowSize),
                Math.Max(0, base.Padding.Bottom - GlowSize)
            );
            base.OnPaddingChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            if (GlowSize <= 0) return;

            Rectangle r = ClientRectangle;
            if (r.Width <= 1 || r.Height <= 1) return;
            r.Inflate(-1, -1);

            Color baseColor = _isActive ? ActiveGlow : InactiveGlow;

            if (!_isActive && !DrawInactiveGlow)
                return;

            // Local helper: rounded rectangle path
            System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
            {
                var path = new System.Drawing.Drawing2D.GraphicsPath();

                if (radius <= 0)
                {
                    path.AddRectangle(rect);
                    path.CloseFigure();
                    return path;
                }

                int rr = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height / 2));
                int d = rr * 2;

                var arc = new Rectangle(rect.Location, new Size(d, d));

                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - d;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - d;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);

                path.CloseFigure();
                return path;
            }

            // Choose a radius. If you add a CornerRadius property, use it.
            // Otherwise this default will give you modern rounded edges.
            int radius = 12;

            if (!_isActive)
            {
                // Crisp rounded edge when inactive
                using var pen = new Pen(baseColor);
                using var path = CreateRoundedRectPath(r, radius);
                g.DrawPath(pen, path);
                return;
            }

            // Active: multi-ring fade to simulate glow (rounded)
            for (int i = 0; i < GlowSize; i++)
            {
                int alpha = (int)(255 * (0.9 - (0.8 * i / Math.Max(1, GlowSize - 1))));
                alpha = Math.Max(10, Math.Min(230, alpha));
                using var pen = new Pen(Color.FromArgb(alpha, baseColor), 1f);

                var rr = new Rectangle(r.Left + i, r.Top + i, r.Width - 1 - 2 * i, r.Height - 1 - 2 * i);
                if (rr.Width <= 0 || rr.Height <= 0) break;

                // Reduce radius as we move inward so rings stay concentric
                using var path = CreateRoundedRectPath(rr, Math.Max(0, radius - i));
                g.DrawPath(pen, path);
            }

            // Subtle inner edge for definition (rounded)
            using var edge = new Pen(Color.FromArgb(220, baseColor));
            var inner = new Rectangle(
                r.Left + GlowSize - 1,
                r.Top + GlowSize - 1,
                r.Width - 2 * GlowSize + 1,
                r.Height - 2 * GlowSize + 1);

            if (inner.Width > 0 && inner.Height > 0)
            {
                using var path = CreateRoundedRectPath(inner, Math.Max(0, radius - GlowSize));
                g.DrawPath(edge, path);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Paint what would be behind us (supports solid colors and most custom parent painting)
            if (Parent == null)
            {
                base.OnPaintBackground(pevent);
                return;
            }

            Graphics g = pevent.Graphics;
            GraphicsState state = g.Save();
            try
            {
                // Move origin so parent paints in the right place
                g.TranslateTransform(-Left, -Top);

                Rectangle parentRect = new Rectangle(Left, Top, Width, Height);
                using var pea = new PaintEventArgs(g, parentRect);

                InvokePaintBackground(Parent, pea);
                InvokePaint(Parent, pea);
            }
            finally
            {
                g.Restore(state);
            }
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        private void UpdateWindowRegion()
        {
            if (!IsHandleCreated) return;

            // Dispose old region to avoid leaks
            Region?.Dispose();

            using (var path = GetRoundRectPath(this.ClientRectangle, _cornerRadius))
            {
                // This sets the actual window shape (clipping + hit-test)
                Region = new Region(path);
            }
        }

        private static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            int r = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height / 2));
            int d = r * 2;

            var arc = new Rectangle(rect.Location, new Size(d, d));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private static GraphicsPath GetRoundRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            int r = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height / 2));
            int d = r * 2;

            // shrink 1px so the region doesn't clip the border weirdly
            rect = Rectangle.Inflate(rect, -1, -1);

            var arc = new Rectangle(rect.Location, new Size(d, d));
            path.AddArc(arc, 180, 90);

            arc.X = rect.Right - d;
            path.AddArc(arc, 270, 90);

            arc.Y = rect.Bottom - d;
            path.AddArc(arc, 0, 90);

            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }


        private void RecalcPadding()
        {
            base.Padding = new Padding(
                _basePadding.Left + GlowSize,
                _basePadding.Top + GlowSize,
                _basePadding.Right + GlowSize,
                _basePadding.Bottom + GlowSize
            );
        }

        /// <summary>
        /// Recursively applies styles based on EditorStyle.
        /// </summary>
        private void SetControlStyles(
            Control ctrl)
        {
            if (ctrl == null) return;

            SetBorderStyle(ctrl);

            // Form dialogs
            if (ctrl is Form || ctrl is FctbForm)
            {
                SetControlColors(ctrl);
            }
            // Textbox inputs
            else if (ctrl is TextBox ||
                ctrl is MaskedTextBox ||
                ctrl is RichTextBox)
            {
                var textBoxBase = ctrl as TextBoxBase;

                if (textBoxBase != null && textBoxBase.ReadOnly)
                {
                    SetControlColors(ctrl);
                }
                else
                {
                    Color backColor = ColorHelper.ShiftColor(GetEditorStyleColor(), 30);
                    SetControlColors(ctrl, backColor);
                }

                ForeColorAdjustments(ctrl);
            }
            // Buttons
            else if (ctrl is Button || ctrl is FctbButton)
            {
                SetControlColors(ctrl);

                if (IsDefaultButton(ctrl))
                {
                    ctrl.BackColor =
                        (ColorHelper.ColorLuminance(GetEditorStyleColor()) > ColorHelper.LuminanceThreshold)
                            ? DefaultButtonColorLightTheme
                            : DefaultButtonColorDarkTheme;
                }
            }
            // CheckBoxes
            else if (ctrl is CheckBox)
            {
                SetControlColors(ctrl);
            }
            // RadioButtons
            else if (ctrl is RadioButton)
            {
                SetControlColors(ctrl);
            }
            // Labels / Groups
            else if (ctrl is LinkLabel ||
                     ctrl is GroupBox ||
                     ctrl is Label)
            {
                SetControlColors(ctrl);
            }

            // Recurse
            foreach (Control child in ctrl.Controls)
            {
                SetControlStyles(child);
            }
        }

        public Color GetEditorStyleColor()
        {
            switch (_dialogStyle)
            {
                case DialogStyles.Dark:
                    return _dialogStyleColors["Dark"].ToColor();
                case DialogStyles.DeepBlack:
                    return _dialogStyleColors["DeepBlack"].ToColor();
                case DialogStyles.Default:
                    return _dialogStyleColors["Default"].ToColor();
                case DialogStyles.Khaki:
                    return _dialogStyleColors["Khaki"].ToColor();
                case DialogStyles.Light:
                    return _dialogStyleColors["Light"].ToColor();
                case DialogStyles.NavajoRed:
                    return _dialogStyleColors["NavajoRed"].ToColor();
                case DialogStyles.RubyBlueDark:
                    return _dialogStyleColors["RubyBlueDark"].ToColor();
                case DialogStyles.RubyBlueLight:
                    return _dialogStyleColors["RubyBlueLight"].ToColor();
                case DialogStyles.Twilight:
                    return _dialogStyleColors["Twilight"].ToColor();
                case DialogStyles.Custom1:
                    return _dialogStyleColors["Custom1"].ToColor();
                case DialogStyles.Custom2:
                    return _dialogStyleColors["Custom2"].ToColor();
                case DialogStyles.Custom3:
                    return _dialogStyleColors["Custom3"].ToColor();
                default:
                    return Color.Empty; // VB Select Case had no Else
            }
        }

        private void SetControlColors(Control ctrl)
        {
            Color backColor = GetEditorStyleColor();
            SetControlColors(ctrl, backColor);
        }

        private void SetControlColors(Control ctrl, Color backColor)
        {
            var foreColor = ColorHelper.ColorLuminance(backColor) > ColorHelper.LuminanceThreshold ? Color.Black : Color.White;

            if (ctrl is Form dlg)
            {
                dlg.BackColor = backColor;
                dlg.ForeColor = foreColor;
            }
            else if (ctrl is FctbForm fdlg)
            {
                fdlg.BackColor = backColor;
                fdlg.ForeColor = foreColor;
            }
            else if (ctrl is TextBox tb)
            {
                tb.BorderStyle = BorderStyle.None;
                tb.BackColor = backColor;
                tb.ForeColor = foreColor;
            }
            else if (ctrl is RichTextBox rtb)
            {
                rtb.BorderStyle = BorderStyle.None;
                rtb.BackColor = backColor;
                rtb.ForeColor = foreColor;
            }
            else if (ctrl is FctbButton fbt)
            {
                Color shiftedColor = SuggestedShiftColor(backColor, redShift: -10, greenShift: 0, blueShift: -10);

                fbt.BackColor = (fbt.BackgroundImage == null)
                    ? SuggestedShiftColor(backColor)
                    : backColor;

                fbt.FlatAppearance.BorderColor = (fbt.BackgroundImage == null)
                    ? fbt.FlatAppearance.BorderColor
                    : backColor;

                fbt.ForeColor = foreColor;

                bool isDarkBackground = (ColorHelper.ColorLuminance(backColor) < ColorHelper.LuminanceThreshold);
                fbt.ApplyContrastImage(isDarkBackground);
                fbt.ApplyContrastBackgroundImage(isDarkBackground);

                fbt.CornerRadius = 12;
            }
            else if (ctrl is Button bt)
            {
                backColor = bt.FindForm().BackColor;
                bt.BackColor = SuggestedShiftColor(backColor, -10, 0, -10);

                bt.ForeColor = foreColor;
            }
            else if (ctrl is LinkLabel ll)
            {
                ll.BackColor = backColor;
                ll.LinkColor = foreColor;
            }
            else if (ctrl is Label lb)
            {
                lb.BackColor = backColor;
                lb.ForeColor = foreColor;
            }
            else
            {
                ctrl.BackColor = backColor;
                ctrl.ForeColor = foreColor;
            }

            ctrl.Invalidate();
        }

        public bool IsDefaultButton(Control btn)
        {
            if (btn == null)
                return false;

            // Find the form that contains this button
            Form dlg = btn.FindForm();
            if (dlg == null)
                return false;

            // Check if this button is the form's AcceptButton
            return dlg.AcceptButton == btn;
        }

        private Color SuggestedShiftColor(Color color, int redShift = 0, int greenShift = 0, int blueShift = 0)
        {
            int shift = ColorHelper.NormalizeColorShift(color, 20); // SuggestedColorShift(color)
            return ColorHelper.ShiftColor(
                color,
                redShift: shift + redShift,
                greenShift: shift + greenShift,
                blueShift: shift + blueShift,
                alphaShift: 0,
                luminanceThreshold: ColorHelper.LuminanceThreshold);
        }

        /// <summary>
        /// Returns the top-level container (Form, UserControl, etc.) that contains the specified control.
        /// </summary>
        /// <param name="ctrl">The control to inspect.</param>
        /// <returns>The top-level container control, or null if not found.</returns>
        private static Control FindContainer(Control ctrl)
        {
            if (ctrl == null) return null;

            Control current = ctrl.Parent;
            while (current != null && !(current is Form) && !(current is UserControl))
            {
                current = current.Parent;
            }

            return current;
        }

        private void SetBorderStyle(Control ctrl)
        {
            if (ctrl is TextBox ||
                ctrl is MaskedTextBox ||
                ctrl is RichTextBox)
            {
                var tb = ctrl as TextBoxBase;
                if (tb != null)
                {
                    var container = FindContainer(ctrl);
                    var containerBack = container != null ? container.BackColor : ctrl.BackColor;

                    tb.BorderStyle =
                        ColorHelper.ColorLuminance(containerBack) > ColorHelper.LuminanceThreshold
                            ? System.Windows.Forms.BorderStyle.FixedSingle
                            : System.Windows.Forms.BorderStyle.None;
                }
            }
            else if (ctrl is ComboBox combo)
            {
                combo.FlatStyle = FlatStyle.Flat;
            }
            else if (ctrl is ListBox listBox)
            {
                // NOTE: VB code sets "BorderStyle = FlatStyle.Flat" which doesn't match WinForms types.
                // ListBox.BorderStyle expects a System.Windows.Forms.BorderStyle enum.
                // Keeping intent: make it appear flat.
                listBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            }
            else if (ctrl is System.Windows.Forms.CheckedListBox)
            {
                var lb = ctrl as ListBox;
                if (lb != null)
                {
                    var container = FindContainer(ctrl);
                    var containerBack = container != null ? container.BackColor : ctrl.BackColor;

                    lb.BorderStyle =
                        ColorHelper.ColorLuminance(containerBack) > ColorHelper.LuminanceThreshold
                            ? System.Windows.Forms.BorderStyle.FixedSingle
                            : System.Windows.Forms.BorderStyle.None;
                }
            }
            else if (ctrl is Button button)
            {
                Control container = FindContainer(ctrl);
                if (container != null)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 1;

                    if (ColorHelper.ColorLuminance(container.BackColor) > ColorHelper.LuminanceThreshold)
                    {
                        button.FlatAppearance.BorderColor = ColorHelper.ShiftColor(container.BackColor, -50);
                    }
                    else
                    {
                        button.FlatAppearance.BorderColor = ColorHelper.ShiftColor(container.BackColor, 20);
                    }
                }
            }
        }

        private void UpdateRoundedRegion()
        {
            if (!IsHandleCreated) return;

            if (_cornerRadius <= 0)
            {
                Region = null;
                return;
            }

            // ellipse diameter = radius * 2
            IntPtr hrgn = CreateRoundRectRgn(0, 0, Width + 1, Height + 1, _cornerRadius * 2, _cornerRadius * 2);
            SetWindowRgn(Handle, hrgn, true);
        }

        /// <summary>
        /// Normalizes the given color to either a dark or light gray, adjusting its luminance
        /// based on the difference from the original color's luminance.
        /// </summary>
        /// <param name="color">The original <see cref="Color"/> to be normalized.</param>
        /// <returns>
        /// A normalized <see cref="Color"/> that is either <see cref="Color.DarkGray"/> or
        /// <see cref="Color.LightGray"/>, potentially shifted in luminance.
        /// </returns>
        private Color NormalizeGray(Color color)
        {
            Color grayColor =
                ColorHelper.ColorLuminance(color) > ColorHelper.LuminanceThreshold
                    ? Color.DarkGray
                    : Color.LightGray;

            int lumaDiff =
                ColorHelper.ColorLuminance(grayColor) - ColorHelper.ColorLuminance(color);

            if (lumaDiff > 0 && lumaDiff <= 5)
            {
                return ColorHelper.ShiftColor(grayColor, 20);
            }
            else if (lumaDiff > 5 && lumaDiff <= 10)
            {
                return ColorHelper.ShiftColor(grayColor, 10);
            }
            else if (lumaDiff < 0 && lumaDiff >= -5)
            {
                return ColorHelper.ShiftColor(grayColor, -20);
            }
            else if (lumaDiff < -5 && lumaDiff >= -10)
            {
                return ColorHelper.ShiftColor(grayColor, -10);
            }
            else
            {
                return grayColor;
            }
        }

        public Color GetSuggestedBackColor(int rgbShift = 0)
        {
            switch (_dialogStyle)
            {
                case DialogStyles.Dark:
                    return ColorHelper.ShiftColor(_dialogStyleColors["Dark"].ToColor(), rgbShift);
                case DialogStyles.DeepBlack:
                    return ColorHelper.ShiftColor(_dialogStyleColors["DeepBlack"].ToColor(), rgbShift);
                case DialogStyles.Default:
                    return ColorHelper.ShiftColor(_dialogStyleColors["Default"].ToColor(), rgbShift);
                case DialogStyles.Khaki:
                    return ColorHelper.ShiftColor(_dialogStyleColors["Khaki"].ToColor(), rgbShift);
                case DialogStyles.Light:
                    return ColorHelper.ShiftColor(_dialogStyleColors["Light"].ToColor(), rgbShift);
                case DialogStyles.NavajoRed:
                    return ColorHelper.ShiftColor(_dialogStyleColors["NavajoRed"].ToColor(), rgbShift);
                case DialogStyles.RubyBlueDark:
                    return ColorHelper.ShiftColor(_dialogStyleColors["RubyBlueDark"].ToColor(), rgbShift);
                case DialogStyles.RubyBlueLight:
                    return ColorHelper.ShiftColor(_dialogStyleColors["RubyBlueLight"].ToColor(), rgbShift);
                case DialogStyles.Twilight:
                    return ColorHelper.ShiftColor(_dialogStyleColors["Twilight"].ToColor(), rgbShift);
                case DialogStyles.Custom1:
                    return ColorHelper.ShiftColor(_dialogStyleColors["Custom1"].ToColor(), rgbShift);
                case DialogStyles.Custom2:
                    return ColorHelper.ShiftColor(_dialogStyleColors["Custom2"].ToColor(), rgbShift);
                case DialogStyles.Custom3:
                    return ColorHelper.ShiftColor(_dialogStyleColors["Custom3"].ToColor(), rgbShift);
                default:
                    return Color.Empty; // VB Select Case had no default; choose a safe fallback.
            }
        }

        /// <summary>
        /// Adjusts the <c>LeftPadding</c> of a <see cref="TextBox"/> control based on the character width,
        /// if it hasn't been set explicitly. Also synchronizes the <c>PaddingBackColor</c> with the control's <c>BackColor</c>.
        /// </summary>
        /// <param name="ctrl">The <see cref="TextBox"/> control to adjust.</param>
        public void ForeColorAdjustments(Control ctrl)
        {
            ctrl.ForeColor = ColorHelper.AdjustColorWithDesiredContrast(ctrl.BackColor, ctrl.ForeColor);
            ctrl.Invalidate();
        }

        private int SuggestedColorShift(Color color)
        {
            // NOTE: Your VB uses BackColor (likely the container/control backcolor) not the parameter.
            // Preserving that behavior here:
            int luma = ColorHelper.ColorLuminance(this.BackColor);

            int expectedShift;
            if (luma > 240)
                expectedShift = -60;
            else if (luma > 180)
                expectedShift = -40;
            else if (luma > 100)
                expectedShift = -20;
            else if (luma > 60)
                expectedShift = 20;
            else
                expectedShift = 40;

            return expectedShift;
        }
    }
}
