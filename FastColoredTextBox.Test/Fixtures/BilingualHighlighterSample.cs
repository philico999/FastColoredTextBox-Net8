using FastColoredTextBoxNS;
using System.Text.RegularExpressions;

namespace Tester
{
    public partial class BilingualHighlighterSample : Form
    {
        public BilingualHighlighterSample()
        {
            InitializeComponent();
        }

        private void tb_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            var tb = (FastColoredTextBoxNS.FastColoredTextBox)sender;

            //highlight html
            tb.SyntaxHighlighter.HTMLSyntaxHighlight(tb.Range);
            tb.Range.ClearFoldingMarkers();
            //find PHP fragments
            foreach (var r in tb.GetRanges(@"<\?php.*?\?>", RegexOptions.Singleline))
            {
                //remove HTML highlighting from this fragment
                r.ClearStyle(StyleIndex.All);
                //do PHP highlighting
                tb.SyntaxHighlighter.InitStyleSchema(Language.PHP, this.BackColor);
                tb.SyntaxHighlighter.PHPSyntaxHighlight(r);
            }
        }
    }
}
