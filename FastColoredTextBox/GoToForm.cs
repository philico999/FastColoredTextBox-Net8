using System;
using System.Windows.Forms;

namespace FastColoredTextBoxNS
{
    public partial class GoToForm : FctbForm
    {
        public int SelectedLineNumber { get; set; }
        public int TotalLineCount { get; set; }

        public GoToForm()
        {
            InitializeComponent();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.tbLineNumber.Text = this.SelectedLineNumber.ToString();

            this.label.Text = String.Format("Line number (1 - {0}):", this.TotalLineCount);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.tbLineNumber.Focus();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            int enteredLine;
            if (int.TryParse(this.tbLineNumber.Text, out enteredLine))
            {
                enteredLine = Math.Min(enteredLine, this.TotalLineCount);
                enteredLine = Math.Max(1, enteredLine);

                this.SelectedLineNumber = enteredLine;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
