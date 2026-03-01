using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Tester
{
    public partial class SimplestSyntaxHighlightingSample : Form
    {
        //Create style for highlighting
        TextStyle colorStyle = new TextStyle(null, Brushes.Gold, FontStyle.Bold);

        public SimplestSyntaxHighlightingSample()
        {
            InitializeComponent();
        }

        private void fctb_TextChanged(object sender, TextChangedEventArgs e)
        {
            //clear previous highlighting
            e.ChangedRange.ClearStyle(colorStyle);
            //highlight tags
            e.ChangedRange.SetStyle(colorStyle, textBox1.Text, RegexOptions.IgnoreCase);
        }

        private void textBox1_TextChanged(object sender, System.EventArgs e)
        {
            try
            {
                fctb.Range.ClearStyle(colorStyle);
                fctb.Range.SetStyle(colorStyle, textBox1.Text, RegexOptions.IgnoreCase);
            }
            catch (System.Exception)
            {
            }
        }
    }
}
