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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_lstFolders = new System.Windows.Forms.ListBox();
            this.m_grpVideoFolders = new System.Windows.Forms.GroupBox();
            this.m_btnRemove = new System.Windows.Forms.Button();
            this.m_btnAdd = new System.Windows.Forms.Button();
            this.m_btnBrowser = new System.Windows.Forms.Button();
            this.m_btnPlayer = new System.Windows.Forms.Button();
            this.m_cmbBrowser = new System.Windows.Forms.ComboBox();
            this.m_btnLogViewer = new System.Windows.Forms.Button();
            this.m_grpLogSeverity = new System.Windows.Forms.GroupBox();
            this.m_chkVerbose = new System.Windows.Forms.CheckBox();
            this.m_chkInfo = new System.Windows.Forms.CheckBox();
            this.m_chkWarning = new System.Windows.Forms.CheckBox();
            this.m_chkError = new System.Windows.Forms.CheckBox();
            this.m_chkSuccess = new System.Windows.Forms.CheckBox();
            this.m_chkNone = new System.Windows.Forms.CheckBox();
            this.m_txtLogViewer = new VideoLibrarian.LabeledTextBox();
            this.m_txtPlayer = new VideoLibrarian.LabeledTextBox();
            this.m_txtBrowser = new VideoLibrarian.LabeledTextBox();
            this.m_grpVideoFolders.SuspendLayout();
            this.m_grpLogSeverity.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.AccessibleDescription = "Cancel all\\nchanges.";
            this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(321, 372);
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
            this.m_btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(200, 372);
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
            this.m_lstFolders.Size = new System.Drawing.Size(233, 106);
            this.m_lstFolders.Sorted = true;
            this.m_lstFolders.TabIndex = 6;
            // 
            // m_grpVideoFolders
            // 
            this.m_grpVideoFolders.AccessibleDescription = "List of root video folders.";
            this.m_grpVideoFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_grpVideoFolders.Controls.Add(this.m_btnRemove);
            this.m_grpVideoFolders.Controls.Add(this.m_btnAdd);
            this.m_grpVideoFolders.Controls.Add(this.m_lstFolders);
            this.m_grpVideoFolders.Location = new System.Drawing.Point(18, 18);
            this.m_grpVideoFolders.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpVideoFolders.Name = "m_grpVideoFolders";
            this.m_grpVideoFolders.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpVideoFolders.Size = new System.Drawing.Size(267, 202);
            this.m_grpVideoFolders.TabIndex = 7;
            this.m_grpVideoFolders.TabStop = false;
            this.m_grpVideoFolders.Text = "Video Folders";
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
            // m_btnBrowser
            // 
            this.m_btnBrowser.AccessibleDescription = "Select the browser executable.";
            this.m_btnBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnBrowser.Image = global::VideoLibrarian.Properties.Resources.OpenFile_24;
            this.m_btnBrowser.Location = new System.Drawing.Point(407, 238);
            this.m_btnBrowser.Name = "m_btnBrowser";
            this.m_btnBrowser.Size = new System.Drawing.Size(28, 28);
            this.m_btnBrowser.TabIndex = 8;
            this.m_btnBrowser.UseVisualStyleBackColor = true;
            this.m_btnBrowser.Click += new System.EventHandler(this.m_btnBrowser_Click);
            // 
            // m_btnPlayer
            // 
            this.m_btnPlayer.AccessibleDescription = "Select the video player executable.";
            this.m_btnPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnPlayer.Image = global::VideoLibrarian.Properties.Resources.OpenFile_24;
            this.m_btnPlayer.Location = new System.Drawing.Point(407, 282);
            this.m_btnPlayer.Name = "m_btnPlayer";
            this.m_btnPlayer.Size = new System.Drawing.Size(28, 28);
            this.m_btnPlayer.TabIndex = 10;
            this.m_btnPlayer.UseVisualStyleBackColor = true;
            this.m_btnPlayer.Click += new System.EventHandler(this.m_btnPlayer_Click);
            // 
            // m_cmbBrowser
            // 
            this.m_cmbBrowser.AccessibleDescription = resources.GetString("m_cmbBrowser.AccessibleDescription");
            this.m_cmbBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cmbBrowser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbBrowser.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_cmbBrowser.FormattingEnabled = true;
            this.m_cmbBrowser.Location = new System.Drawing.Point(18, 239);
            this.m_cmbBrowser.Name = "m_cmbBrowser";
            this.m_cmbBrowser.Size = new System.Drawing.Size(389, 26);
            this.m_cmbBrowser.TabIndex = 12;
            this.m_cmbBrowser.DropDown += new System.EventHandler(this.m_cmbBrowser_DropDown);
            this.m_cmbBrowser.SelectionChangeCommitted += new System.EventHandler(this.m_cmbBrowser_SelectionChangeCommitted);
            // 
            // m_btnLogViewer
            // 
            this.m_btnLogViewer.AccessibleDescription = "Select the log viewer executable. This\\nmay be any text viewer/editor.";
            this.m_btnLogViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnLogViewer.Image = global::VideoLibrarian.Properties.Resources.OpenFile_24;
            this.m_btnLogViewer.Location = new System.Drawing.Point(407, 326);
            this.m_btnLogViewer.Name = "m_btnLogViewer";
            this.m_btnLogViewer.Size = new System.Drawing.Size(28, 28);
            this.m_btnLogViewer.TabIndex = 13;
            this.m_btnLogViewer.UseVisualStyleBackColor = true;
            this.m_btnLogViewer.Click += new System.EventHandler(this.m_btnLogViewer_Click);
            // 
            // m_grpLogSeverity
            // 
            this.m_grpLogSeverity.AccessibleDescription = resources.GetString("m_grpLogSeverity.AccessibleDescription");
            this.m_grpLogSeverity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_grpLogSeverity.Controls.Add(this.m_chkVerbose);
            this.m_grpLogSeverity.Controls.Add(this.m_chkInfo);
            this.m_grpLogSeverity.Controls.Add(this.m_chkWarning);
            this.m_grpLogSeverity.Controls.Add(this.m_chkError);
            this.m_grpLogSeverity.Controls.Add(this.m_chkSuccess);
            this.m_grpLogSeverity.Controls.Add(this.m_chkNone);
            this.m_grpLogSeverity.Location = new System.Drawing.Point(292, 18);
            this.m_grpLogSeverity.Name = "m_grpLogSeverity";
            this.m_grpLogSeverity.Size = new System.Drawing.Size(141, 202);
            this.m_grpLogSeverity.TabIndex = 15;
            this.m_grpLogSeverity.TabStop = false;
            this.m_grpLogSeverity.Text = "Logging Severity";
            // 
            // m_chkVerbose
            // 
            this.m_chkVerbose.AccessibleDescription = "Verbose/Debugging messages. Lots\\nof information regarding the state at\\nmany poi" +
    "nts in the application.";
            this.m_chkVerbose.AutoSize = true;
            this.m_chkVerbose.Location = new System.Drawing.Point(14, 161);
            this.m_chkVerbose.Name = "m_chkVerbose";
            this.m_chkVerbose.Size = new System.Drawing.Size(88, 24);
            this.m_chkVerbose.TabIndex = 5;
            this.m_chkVerbose.Text = "Verbose";
            this.m_chkVerbose.UseVisualStyleBackColor = true;
            this.m_chkVerbose.Click += new System.EventHandler(this.m_chkLevel_Click);
            // 
            // m_chkInfo
            // 
            this.m_chkInfo.AccessibleDescription = "Informational messages. Selected\\ninfo regarding successful actions.";
            this.m_chkInfo.AutoSize = true;
            this.m_chkInfo.Location = new System.Drawing.Point(14, 135);
            this.m_chkInfo.Name = "m_chkInfo";
            this.m_chkInfo.Size = new System.Drawing.Size(56, 24);
            this.m_chkInfo.TabIndex = 4;
            this.m_chkInfo.Text = "Info";
            this.m_chkInfo.UseVisualStyleBackColor = true;
            this.m_chkInfo.Click += new System.EventHandler(this.m_chkLevel_Click);
            // 
            // m_chkWarning
            // 
            this.m_chkWarning.AccessibleDescription = "Warning messages. The\\naction failed but recovered.";
            this.m_chkWarning.AutoSize = true;
            this.m_chkWarning.Location = new System.Drawing.Point(14, 109);
            this.m_chkWarning.Name = "m_chkWarning";
            this.m_chkWarning.Size = new System.Drawing.Size(87, 24);
            this.m_chkWarning.TabIndex = 3;
            this.m_chkWarning.Text = "Warning";
            this.m_chkWarning.UseVisualStyleBackColor = true;
            this.m_chkWarning.Click += new System.EventHandler(this.m_chkLevel_Click);
            // 
            // m_chkError
            // 
            this.m_chkError.AccessibleDescription = "Error messages. The\\naction failed.";
            this.m_chkError.AutoSize = true;
            this.m_chkError.Location = new System.Drawing.Point(14, 83);
            this.m_chkError.Name = "m_chkError";
            this.m_chkError.Size = new System.Drawing.Size(63, 24);
            this.m_chkError.TabIndex = 2;
            this.m_chkError.Text = "Error";
            this.m_chkError.UseVisualStyleBackColor = true;
            this.m_chkError.Click += new System.EventHandler(this.m_chkLevel_Click);
            // 
            // m_chkSuccess
            // 
            this.m_chkSuccess.AccessibleDescription = "Success messages. The\\naction succeeded.";
            this.m_chkSuccess.AutoSize = true;
            this.m_chkSuccess.Location = new System.Drawing.Point(14, 57);
            this.m_chkSuccess.Name = "m_chkSuccess";
            this.m_chkSuccess.Size = new System.Drawing.Size(89, 24);
            this.m_chkSuccess.TabIndex = 1;
            this.m_chkSuccess.Text = "Success";
            this.m_chkSuccess.UseVisualStyleBackColor = true;
            this.m_chkSuccess.Click += new System.EventHandler(this.m_chkLevel_Click);
            // 
            // m_chkNone
            // 
            this.m_chkNone.AccessibleDescription = "Messages with\\nno severity status.";
            this.m_chkNone.AutoSize = true;
            this.m_chkNone.Location = new System.Drawing.Point(14, 31);
            this.m_chkNone.Name = "m_chkNone";
            this.m_chkNone.Size = new System.Drawing.Size(66, 24);
            this.m_chkNone.TabIndex = 0;
            this.m_chkNone.Text = "None";
            this.m_chkNone.UseVisualStyleBackColor = true;
            this.m_chkNone.Click += new System.EventHandler(this.m_chkLevel_Click);
            // 
            // m_txtLogViewer
            // 
            this.m_txtLogViewer.AccessibleDescription = resources.GetString("m_txtLogViewer.AccessibleDescription");
            this.m_txtLogViewer.AllowDrop = true;
            this.m_txtLogViewer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtLogViewer.Location = new System.Drawing.Point(18, 327);
            this.m_txtLogViewer.Name = "m_txtLogViewer";
            this.m_txtLogViewer.Size = new System.Drawing.Size(389, 26);
            this.m_txtLogViewer.TabIndex = 14;
            this.m_txtLogViewer.TextLabel = "Select Text Log Viewer";
            this.m_txtLogViewer.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_txtExecutable_DragDrop);
            this.m_txtLogViewer.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_txtExecutable_DragEnter);
            // 
            // m_txtPlayer
            // 
            this.m_txtPlayer.AccessibleDescription = resources.GetString("m_txtPlayer.AccessibleDescription");
            this.m_txtPlayer.AllowDrop = true;
            this.m_txtPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtPlayer.Location = new System.Drawing.Point(18, 283);
            this.m_txtPlayer.Name = "m_txtPlayer";
            this.m_txtPlayer.Size = new System.Drawing.Size(389, 26);
            this.m_txtPlayer.TabIndex = 11;
            this.m_txtPlayer.TextLabel = "Select Video Player";
            this.m_txtPlayer.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_txtExecutable_DragDrop);
            this.m_txtPlayer.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_txtExecutable_DragEnter);
            // 
            // m_txtBrowser
            // 
            this.m_txtBrowser.AccessibleDescription = resources.GetString("m_txtBrowser.AccessibleDescription");
            this.m_txtBrowser.AllowDrop = true;
            this.m_txtBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtBrowser.Location = new System.Drawing.Point(18, 239);
            this.m_txtBrowser.Name = "m_txtBrowser";
            this.m_txtBrowser.Size = new System.Drawing.Size(369, 26);
            this.m_txtBrowser.TabIndex = 9;
            this.m_txtBrowser.TextLabel = "Select Browser";
            this.m_txtBrowser.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_txtExecutable_DragDrop);
            this.m_txtBrowser.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_txtExecutable_DragEnter);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(453, 425);
            this.Controls.Add(this.m_grpLogSeverity);
            this.Controls.Add(this.m_txtLogViewer);
            this.Controls.Add(this.m_btnLogViewer);
            this.Controls.Add(this.m_txtPlayer);
            this.Controls.Add(this.m_btnPlayer);
            this.Controls.Add(this.m_txtBrowser);
            this.Controls.Add(this.m_btnBrowser);
            this.Controls.Add(this.m_grpVideoFolders);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_cmbBrowser);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.m_grpVideoFolders.ResumeLayout(false);
            this.m_grpLogSeverity.ResumeLayout(false);
            this.m_grpLogSeverity.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.ListBox m_lstFolders;
        private System.Windows.Forms.GroupBox m_grpVideoFolders;
        private System.Windows.Forms.Button m_btnRemove;
        private System.Windows.Forms.Button m_btnAdd;
        private System.Windows.Forms.Button m_btnBrowser;
        private VideoLibrarian.LabeledTextBox m_txtBrowser;
        private VideoLibrarian.LabeledTextBox m_txtPlayer;
        private System.Windows.Forms.Button m_btnPlayer;
        private System.Windows.Forms.ComboBox m_cmbBrowser;
        private LabeledTextBox m_txtLogViewer;
        private System.Windows.Forms.Button m_btnLogViewer;
        private System.Windows.Forms.GroupBox m_grpLogSeverity;
        private System.Windows.Forms.CheckBox m_chkVerbose;
        private System.Windows.Forms.CheckBox m_chkInfo;
        private System.Windows.Forms.CheckBox m_chkWarning;
        private System.Windows.Forms.CheckBox m_chkError;
        private System.Windows.Forms.CheckBox m_chkSuccess;
        private System.Windows.Forms.CheckBox m_chkNone;
    }
}
