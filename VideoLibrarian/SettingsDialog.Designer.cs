namespace VideoLibrarian
{
    partial class SettingsDialog
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
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_lstFolders = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.m_btnRemove = new System.Windows.Forms.Button();
            this.m_btnAdd = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.AccessibleDescription = "Cancel all\\nchanges.";
            this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(303, 240);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(112, 35);
            this.m_btnCancel.TabIndex = 5;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
            // 
            // m_btnOK
            // 
            this.m_btnOK.AccessibleDescription = "Save changes and\\nupdate movie list";
            this.m_btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(182, 240);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(112, 35);
            this.m_btnOK.TabIndex = 4;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
            // 
            // m_lstFolders
            // 
            this.m_lstFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lstFolders.BackColor = System.Drawing.Color.AliceBlue;
            this.m_lstFolders.IntegralHeight = false;
            this.m_lstFolders.ItemHeight = 20;
            this.m_lstFolders.Location = new System.Drawing.Point(16, 26);
            this.m_lstFolders.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lstFolders.Name = "m_lstFolders";
            this.m_lstFolders.Size = new System.Drawing.Size(364, 106);
            this.m_lstFolders.Sorted = true;
            this.m_lstFolders.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = "List of root video folders.";
            this.groupBox1.Controls.Add(this.m_btnRemove);
            this.groupBox1.Controls.Add(this.m_btnAdd);
            this.groupBox1.Controls.Add(this.m_lstFolders);
            this.groupBox1.Location = new System.Drawing.Point(18, 18);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(398, 202);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Video Folders";
            // 
            // m_btnRemove
            // 
            this.m_btnRemove.AccessibleDescription = "Select row to remove and\\nthen click this button";
            this.m_btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnRemove.Location = new System.Drawing.Point(138, 151);
            this.m_btnRemove.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnRemove.Name = "m_btnRemove";
            this.m_btnRemove.Size = new System.Drawing.Size(112, 35);
            this.m_btnRemove.TabIndex = 8;
            this.m_btnRemove.Text = "Remove";
            this.m_btnRemove.UseVisualStyleBackColor = true;
            this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
            // 
            // m_btnAdd
            // 
            this.m_btnAdd.AccessibleDescription = "Add new root\\nmedia folder";
            this.m_btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnAdd.Location = new System.Drawing.Point(16, 151);
            this.m_btnAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnAdd.Name = "m_btnAdd";
            this.m_btnAdd.Size = new System.Drawing.Size(112, 35);
            this.m_btnAdd.TabIndex = 7;
            this.m_btnAdd.Text = "Add";
            this.m_btnAdd.UseVisualStyleBackColor = true;
            this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(435, 297);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.ListBox m_lstFolders;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button m_btnRemove;
        private System.Windows.Forms.Button m_btnAdd;
    }
}