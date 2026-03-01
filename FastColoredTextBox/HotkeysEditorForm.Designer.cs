namespace FastColoredTextBoxNS
{
    partial class HotkeysEditorForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            dgv = new System.Windows.Forms.DataGridView();
            cbModifiers = new System.Windows.Forms.DataGridViewComboBoxColumn();
            cbKey = new System.Windows.Forms.DataGridViewComboBoxColumn();
            cbAction = new System.Windows.Forms.DataGridViewComboBoxColumn();
            btAdd = new System.Windows.Forms.Button();
            btRemove = new System.Windows.Forms.Button();
            btCancel = new System.Windows.Forms.Button();
            btOk = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            btResore = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            SuspendLayout();
            // 
            // dgv
            // 
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgv.BackgroundColor = System.Drawing.SystemColors.Control;
            dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { cbModifiers, cbKey, cbAction });
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.LightSteelBlue;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgv.DefaultCellStyle = dataGridViewCellStyle1;
            dgv.GridColor = System.Drawing.SystemColors.Control;
            dgv.Location = new System.Drawing.Point(18, 38);
            dgv.Margin = new System.Windows.Forms.Padding(4);
            dgv.Name = "dgv";
            dgv.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgv.Size = new System.Drawing.Size(806, 428);
            dgv.TabIndex = 0;
            dgv.RowsAdded += dgv_RowsAdded;
            // 
            // cbModifiers
            // 
            cbModifiers.DataPropertyName = "Modifiers";
            cbModifiers.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            cbModifiers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cbModifiers.HeaderText = "Modifiers";
            cbModifiers.Name = "cbModifiers";
            // 
            // cbKey
            // 
            cbKey.DataPropertyName = "Key";
            cbKey.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            cbKey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cbKey.HeaderText = "Key";
            cbKey.Name = "cbKey";
            cbKey.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            cbKey.Width = 120;
            // 
            // cbAction
            // 
            cbAction.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            cbAction.DataPropertyName = "Action";
            cbAction.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            cbAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cbAction.HeaderText = "Action";
            cbAction.Name = "cbAction";
            // 
            // btAdd
            // 
            btAdd.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btAdd.BackColor = System.Drawing.Color.White;
            btAdd.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            btAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btAdd.Location = new System.Drawing.Point(19, 488);
            btAdd.Margin = new System.Windows.Forms.Padding(4);
            btAdd.Name = "btAdd";
            btAdd.Size = new System.Drawing.Size(112, 32);
            btAdd.TabIndex = 1;
            btAdd.Text = "Add";
            btAdd.UseVisualStyleBackColor = false;
            btAdd.Click += btAdd_Click;
            // 
            // btRemove
            // 
            btRemove.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btRemove.BackColor = System.Drawing.Color.White;
            btRemove.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            btRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btRemove.Location = new System.Drawing.Point(154, 488);
            btRemove.Margin = new System.Windows.Forms.Padding(4);
            btRemove.Name = "btRemove";
            btRemove.Size = new System.Drawing.Size(112, 32);
            btRemove.TabIndex = 2;
            btRemove.Text = "Remove";
            btRemove.UseVisualStyleBackColor = false;
            btRemove.Click += btRemove_Click;
            // 
            // btCancel
            // 
            btCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btCancel.BackColor = System.Drawing.Color.White;
            btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btCancel.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            btCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btCancel.Location = new System.Drawing.Point(708, 488);
            btCancel.Margin = new System.Windows.Forms.Padding(4);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(112, 32);
            btCancel.TabIndex = 4;
            btCancel.Text = "Cancel";
            btCancel.UseVisualStyleBackColor = false;
            // 
            // btOk
            // 
            btOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btOk.BackColor = System.Drawing.Color.White;
            btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            btOk.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            btOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btOk.Location = new System.Drawing.Point(586, 488);
            btOk.Margin = new System.Windows.Forms.Padding(4);
            btOk.Name = "btOk";
            btOk.Size = new System.Drawing.Size(112, 32);
            btOk.TabIndex = 3;
            btOk.Text = "OK";
            btOk.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            label1.Location = new System.Drawing.Point(18, 12);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(113, 16);
            label1.TabIndex = 5;
            label1.Text = "Hotkeys mapping";
            // 
            // btResore
            // 
            btResore.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btResore.BackColor = System.Drawing.Color.White;
            btResore.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            btResore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btResore.Location = new System.Drawing.Point(291, 488);
            btResore.Margin = new System.Windows.Forms.Padding(4);
            btResore.Name = "btResore";
            btResore.Size = new System.Drawing.Size(158, 32);
            btResore.TabIndex = 6;
            btResore.Text = "Restore default";
            btResore.UseVisualStyleBackColor = false;
            btResore.Click += btResore_Click;
            // 
            // HotkeysEditorForm
            // 
            AcceptButton = btOk;
            AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(842, 537);
            Controls.Add(btResore);
            Controls.Add(label1);
            Controls.Add(btCancel);
            Controls.Add(btOk);
            Controls.Add(btRemove);
            Controls.Add(btAdd);
            Controls.Add(dgv);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(4);
            MaximumSize = new System.Drawing.Size(842, 960);
            MinimumSize = new System.Drawing.Size(842, 537);
            Name = "HotkeysEditorForm";
            Padding = new System.Windows.Forms.Padding(7, 7, 7, 7);
            ShowIcon = false;
            Text = "Hotkeys Editor";
            FormClosing += HotkeysEditorForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.Button btRemove;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btResore;
        private System.Windows.Forms.DataGridViewComboBoxColumn cbModifiers;
        private System.Windows.Forms.DataGridViewComboBoxColumn cbKey;
        private System.Windows.Forms.DataGridViewComboBoxColumn cbAction;
    }
}