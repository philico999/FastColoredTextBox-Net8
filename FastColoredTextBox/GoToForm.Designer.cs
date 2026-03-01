namespace FastColoredTextBoxNS
{
    partial class GoToForm
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
            label = new System.Windows.Forms.Label();
            tbLineNumber = new System.Windows.Forms.TextBox();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // label
            // 
            label.AutoSize = true;
            label.Location = new System.Drawing.Point(18, 12);
            label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label.Name = "label";
            label.Size = new System.Drawing.Size(130, 18);
            label.TabIndex = 0;
            label.Text = "Line Number (1/1):";
            // 
            // tbLineNumber
            // 
            tbLineNumber.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbLineNumber.Location = new System.Drawing.Point(18, 40);
            tbLineNumber.Margin = new System.Windows.Forms.Padding(4);
            tbLineNumber.Name = "tbLineNumber";
            tbLineNumber.Size = new System.Drawing.Size(442, 24);
            tbLineNumber.TabIndex = 1;
            // 
            // btnOk
            // 
            btnOk.BackColor = System.Drawing.Color.White;
            btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOk.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOk.Location = new System.Drawing.Point(228, 98);
            btnOk.Margin = new System.Windows.Forms.Padding(4);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(112, 32);
            btnOk.TabIndex = 2;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = false;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = System.Drawing.Color.White;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCancel.Location = new System.Drawing.Point(350, 98);
            btnCancel.Margin = new System.Windows.Forms.Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(112, 32);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // GoToForm
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(480, 146);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(tbLineNumber);
            Controls.Add(label);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GoToForm";
            Padding = new System.Windows.Forms.Padding(7, 7, 7, 7);
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Go To Line";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label;
        private System.Windows.Forms.TextBox tbLineNumber;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}