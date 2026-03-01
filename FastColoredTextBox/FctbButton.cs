using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FastColoredTextBoxNS
{
    [DefaultEvent("Click")]
    public class FctbButton : Button
    {
        private Image _backgroundImageDarker;
        private Image _backgroundImageLighter;

        private Image _imageDarker;
        private Image _imageLighter;

        private int _cornerRadius = 0;

        private bool _isHovered = false;
        private bool _isPressedMouse = false;
        private bool _isPressedKeyboard = false;

        private GraphicsPath _shapePath = null;
        private Rectangle _shapeRect = Rectangle.Empty;

        private bool IsPressed => _isPressedMouse || _isPressedKeyboard;

        public FctbButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            DoubleBuffered = true;

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            FlatAppearance.MouseOverBackColor = Color.Transparent;

            UseVisualStyleBackColor = false;
        }

        [Category("Appearance")]
        [DefaultValue(0)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                if (value < 0) value = 0;
                if (_cornerRadius != value)
                {
                    _cornerRadius = value;
                    UpdateRegion();
                    Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (_shapePath == null)
                UpdateRegion();

            // Paint parent background (so clipped corners look clean)
            if (Parent != null)
            {
                using var bg = new SolidBrush(Parent.BackColor);
                g.FillRectangle(bg, ClientRectangle);
            }

            // Clip + fill rounded shape
            using (var clip = new Region(_shapePath))
            {
                g.Clip = clip;

                using var backBrush = new SolidBrush(BackColor);
                g.FillPath(backBrush, _shapePath);

                // Background image (respect layout roughly)
                if (BackgroundImage != null)
                {
                    g.DrawImage(BackgroundImage, ClientRectangle);
                }

                // Draw text centered (simple, WinForms-like)
                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    ClientRectangle,
                    Enabled ? ForeColor : SystemColors.GrayText,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.EndEllipsis);
            }

            // Focus cues
            if (Focused && ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(ClientRectangle, -4, -4));
            }
        }


        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRegion();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressedMouse = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!HitTest(e.Location))
                return;

            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                _isPressedMouse = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressedMouse = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _isPressedKeyboard = false;
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!Enabled) return;

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                if (!_isPressedKeyboard)
                {
                    _isPressedKeyboard = true;
                    Invalidate();
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                if (_isPressedKeyboard)
                {
                    _isPressedKeyboard = false;
                    Invalidate();
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_isPressedKeyboard)
            {
                _isPressedKeyboard = false;
                Invalidate();
            }
        }

        private bool HitTest(Point pt)
        {
            if (_shapePath == null) return true;
            return _shapePath.IsVisible(pt);
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Image), null)]
        public Image BackgroundImageDarker
        {
            get => _backgroundImageDarker;
            set
            {
                if (!Equals(_backgroundImageDarker, value))
                {
                    _backgroundImageDarker = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Image), null)]
        public Image BackgroundImageLighter
        {
            get => _backgroundImageLighter;
            set
            {
                if (!Equals(_backgroundImageLighter, value))
                {
                    _backgroundImageLighter = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Image), null)]
        public Image ImageDarker
        {
            get => _imageDarker;
            set
            {
                if (!Equals(_imageDarker, value))
                {
                    _imageDarker = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Image), null)]
        public Image ImageLighter
        {
            get => _imageLighter;
            set
            {
                if (!Equals(_imageLighter, value))
                {
                    _imageLighter = value;
                    Invalidate();
                }
            }
        }

        public void ApplyContrastImage(bool useLighterImage)
        {
            if (useLighterImage)
            {
                if (_imageLighter != null)
                {
                    Image = _imageLighter;
                    Invalidate();
                }
            }
            else
            {
                if (_imageDarker != null)
                {
                    Image = _imageDarker;
                    Invalidate();
                }
            }
        }

        public void ApplyContrastBackgroundImage(bool useLighterImage)
        {
            if (useLighterImage)
            {
                if (_backgroundImageLighter != null)
                {
                    BackgroundImage = _backgroundImageLighter;
                    Invalidate();
                }
            }
            else
            {
                if (_backgroundImageDarker != null)
                {
                    BackgroundImage = _backgroundImageDarker;
                    Invalidate();
                }
            }
        }

        public void ApplyContrastBackgroundImage(Color backColor, int thresholdLuminance = ColorHelper.LuminanceThreshold)
        {
            if (ColorHelper.ColorLuminance(backColor) > thresholdLuminance)
            {
                if (_backgroundImageDarker != null)
                {
                    BackgroundImage = _backgroundImageDarker;
                    Invalidate();
                }
            }
            else
            {
                if (_backgroundImageLighter != null)
                {
                    BackgroundImage = _backgroundImageLighter;
                    Invalidate();
                }
            }
        }

        private void UpdateRegion()
        {
            _shapePath?.Dispose();
            _shapePath = null;
            _shapeRect = Rectangle.Empty;

            Rectangle rect = Rectangle.Inflate(this.ClientRectangle, -1, -1);
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            _shapeRect = rect;
            _shapePath = GetButtonPath(_shapeRect, _cornerRadius);

            // IMPORTANT:
            // Do NOT set this.Region here — that causes jagged edges.
        }

        private GraphicsPath GetButtonPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            int r = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height / 2));
            int d = r * 2;

            Rectangle arc = new Rectangle(rect.Location, new Size(d, d));

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
    }
}
