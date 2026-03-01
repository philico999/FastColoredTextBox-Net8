namespace Tester
{
    partial class CustomStyleSample
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomStyleSample));
            this.label1 = new System.Windows.Forms.Label();
            this.fctb = new FastColoredTextBoxNS.FastColoredTextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.fctb)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(345, 33);
            this.label1.TabIndex = 1;
            this.label1.Text = "This example shows how to create own custom style.";
            // 
            // fctb
            // 
            this.fctb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fctb.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.fctb.AutoIndent = false;
            this.fctb.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
            this.fctb.AutoScrollMinSize = new System.Drawing.Size(595, 180);
            this.fctb.BackBrush = null;
            this.fctb.CharHeight = 15;
            this.fctb.CharWidth = 7;
            this.fctb.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.fctb.DefaultMarkerSize = 8;
            this.fctb.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.fctb.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.fctb.IsReplaceMode = false;
            this.fctb.LeftBracket = '(';
            this.fctb.LeftPadding = 3;
            this.fctb.Location = new System.Drawing.Point(0, 39);
            this.fctb.Name = "fctb";
            this.fctb.PaddingBackColor = System.Drawing.Color.WhiteSmoke;
            this.fctb.Paddings = new System.Windows.Forms.Padding(15);
            this.fctb.PreferredLineWidth = 80;
            this.fctb.RightBracket = ')';
            this.fctb.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.fctb.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("fctb.ServiceColors")));
            this.fctb.ShowLineNumbers = false;
            this.fctb.Size = new System.Drawing.Size(592, 340);
            this.fctb.TabIndex = 2;
            this.fctb.Text = resources.GetString("fctb.Text");
            this.fctb.TextAreaBorder = FastColoredTextBoxNS.TextAreaBorderType.Shadow;
            this.fctb.WordWrap = true;
            this.fctb.WordWrapMode = FastColoredTextBoxNS.WordWrapMode.WordWrapPreferredWidth;
            this.fctb.Zoom = 100;
            this.fctb.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.fctb_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(469, 6);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(108, 27);
            this.textBox1.TabIndex = 3;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // CustomStyleSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 391);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.fctb);
            this.Controls.Add(this.label1);
            this.Name = "CustomStyleSample";
            this.Text = "Custom style sample";
            ((System.ComponentModel.ISupportInitialize)(this.fctb)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private FastColoredTextBoxNS.FastColoredTextBox fctb;
        private System.Windows.Forms.TextBox textBox1;
    }
}

