namespace FastColoredTextBoxNS
{
    partial class FindForm
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
            btClose = new FctbButton();
            btFindNext = new FctbButton();
            tbFind = new System.Windows.Forms.TextBox();
            cbRegex = new System.Windows.Forms.CheckBox();
            cbMatchCase = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            cbWholeWord = new System.Windows.Forms.CheckBox();
            cbWrapSearches = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // btClose
            // 
            btClose.BackColor = System.Drawing.Color.White;
            btClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btClose.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            btClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btClose.Location = new System.Drawing.Point(431, 116);
            btClose.Margin = new System.Windows.Forms.Padding(4);
            btClose.Name = "btClose";
            btClose.Size = new System.Drawing.Size(91, 32);
            btClose.TabIndex = 6;
            btClose.Text = "Close";
            btClose.UseVisualStyleBackColor = false;
            btClose.Click += btClose_Click;
            // 
            // btFindNext
            // 
            btFindNext.BackColor = System.Drawing.Color.White;
            btFindNext.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            btFindNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btFindNext.Location = new System.Drawing.Point(309, 116);
            btFindNext.Margin = new System.Windows.Forms.Padding(4);
            btFindNext.Name = "btFindNext";
            btFindNext.Size = new System.Drawing.Size(112, 32);
            btFindNext.TabIndex = 5;
            btFindNext.Text = "Find next";
            btFindNext.UseVisualStyleBackColor = false;
            btFindNext.Click += btFindNext_Click;
            // 
            // tbFind
            // 
            tbFind.Location = new System.Drawing.Point(63, 17);
            tbFind.Margin = new System.Windows.Forms.Padding(4);
            tbFind.Name = "tbFind";
            tbFind.Size = new System.Drawing.Size(457, 24);
            tbFind.TabIndex = 0;
            tbFind.TextChanged += cbMatchCase_CheckedChanged;
            tbFind.Enter += tbFind_Enter;
            tbFind.KeyPress += tbFind_KeyPress;
            // 
            // cbRegex
            // 
            cbRegex.AutoSize = true;
            cbRegex.Location = new System.Drawing.Point(356, 53);
            cbRegex.Margin = new System.Windows.Forms.Padding(4);
            cbRegex.Name = "cbRegex";
            cbRegex.Size = new System.Drawing.Size(154, 22);
            cbRegex.TabIndex = 3;
            cbRegex.Text = "&Regular expression";
            cbRegex.UseVisualStyleBackColor = true;
            cbRegex.CheckedChanged += cbMatchCase_CheckedChanged;
            // 
            // cbMatchCase
            // 
            cbMatchCase.AutoSize = true;
            cbMatchCase.Location = new System.Drawing.Point(63, 53);
            cbMatchCase.Margin = new System.Windows.Forms.Padding(4);
            cbMatchCase.Name = "cbMatchCase";
            cbMatchCase.Size = new System.Drawing.Size(104, 22);
            cbMatchCase.TabIndex = 1;
            cbMatchCase.Text = "Match &case";
            cbMatchCase.UseVisualStyleBackColor = true;
            cbMatchCase.CheckedChanged += cbMatchCase_CheckedChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(9, 20);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(44, 18);
            label1.TabIndex = 5;
            label1.Text = "Find: ";
            // 
            // cbWholeWord
            // 
            cbWholeWord.AutoSize = true;
            cbWholeWord.Location = new System.Drawing.Point(186, 53);
            cbWholeWord.Margin = new System.Windows.Forms.Padding(4);
            cbWholeWord.Name = "cbWholeWord";
            cbWholeWord.Size = new System.Drawing.Size(148, 22);
            cbWholeWord.TabIndex = 2;
            cbWholeWord.Text = "Match &whole word";
            cbWholeWord.UseVisualStyleBackColor = true;
            cbWholeWord.CheckedChanged += cbMatchCase_CheckedChanged;
            // 
            // cbWrapSearches
            // 
            cbWrapSearches.AutoSize = true;
            cbWrapSearches.Checked = true;
            cbWrapSearches.CheckState = System.Windows.Forms.CheckState.Checked;
            cbWrapSearches.Location = new System.Drawing.Point(63, 84);
            cbWrapSearches.Margin = new System.Windows.Forms.Padding(4);
            cbWrapSearches.Name = "cbWrapSearches";
            cbWrapSearches.Size = new System.Drawing.Size(128, 22);
            cbWrapSearches.TabIndex = 4;
            cbWrapSearches.Text = "Wrap &searches";
            cbWrapSearches.UseVisualStyleBackColor = true;
            // 
            // FindForm
            // 
            AcceptButton = btFindNext;
            AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btClose;
            ClientSize = new System.Drawing.Size(540, 162);
            Controls.Add(cbWrapSearches);
            Controls.Add(cbWholeWord);
            Controls.Add(label1);
            Controls.Add(cbMatchCase);
            Controls.Add(cbRegex);
            Controls.Add(tbFind);
            Controls.Add(btFindNext);
            Controls.Add(btClose);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "FindForm";
            Padding = new System.Windows.Forms.Padding(7, 7, 7, 7);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Find";
            TopMost = true;
            FormClosing += FindForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private FctbButton btClose;
        private FctbButton btFindNext;
        private System.Windows.Forms.CheckBox cbRegex;
        private System.Windows.Forms.CheckBox cbMatchCase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbWholeWord;
        public System.Windows.Forms.TextBox tbFind;
        private System.Windows.Forms.CheckBox cbWrapSearches;
    }
}