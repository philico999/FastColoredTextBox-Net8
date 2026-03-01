using System.Drawing;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;

namespace Tester
{
    public partial class CustomStyleSample : Form
    {
        //create my custom style
        EllipseStyle ellipseStyle = new EllipseStyle();

        public CustomStyleSample()
        {
            InitializeComponent();
            textBox1.Text = @"Babylon\b";
        }

        private void fctb_TextChanged(object sender, TextChangedEventArgs e)
        {
            //clear old styles of chars
            e.ChangedRange.ClearStyle(ellipseStyle);
            //append style for word 'Babylon'
            //e.ChangedRange.SetStyle(ellipseStyle, @"\b" + textBox1.Text + "\b", RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(ellipseStyle, textBox1.Text, RegexOptions.IgnoreCase);
        }

        private void textBox1_TextChanged(object sender, System.EventArgs e)
        {
            try
            {
                fctb.Range.ClearStyle(ellipseStyle);
                fctb.Range.SetStyle(ellipseStyle, textBox1.Text, RegexOptions.IgnoreCase);
            }
            catch (System.Exception)
            {
            }
        }
    }

    /// <summary>
    /// This style will drawing ellipse around of the word
    /// </summary>
    class EllipseStyle : Style
    {
        public override void Draw(Graphics gr, Point position, FastColoredTextBoxNS.Range range)
        {
            //get size of rectangle
            Size size = GetSizeOfRange(range);
            //create rectangle
            Rectangle rect = new Rectangle(position, size);
            //inflate it
            rect.Inflate(2, 2);
            //get rounded rectangle
            var path = GetRoundedRectangle(rect, 7);
            //draw rounded rectangle
            gr.DrawPath(Pens.Red, path);
        }
    }
}
