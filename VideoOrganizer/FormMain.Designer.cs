namespace VideoOrganizer
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.m_txtRoot = new VideoOrganizer.LabeledTextBox();
            this.m_btn_SelectRoot = new System.Windows.Forms.Button();
            this.m_rtfStatus = new System.Windows.Forms.RichTextBox();
            this.m_btnGo = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnAbout = new System.Windows.Forms.Button();
            this.m_btnManualConfig = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_txtRoot
            // 
            this.m_txtRoot.AccessibleDescription = "The folder starting point in the search for\\nvideos without an associated IMDB sh" +
    "ortcut. Add\\nfoldername via command-line argument, click-n-\\ndrag, or using open" +
    " folder dialog to the right.";
            this.m_txtRoot.AllowDrop = true;
            this.m_txtRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtRoot.ForeColor = System.Drawing.SystemColors.ControlText;
            this.m_txtRoot.Location = new System.Drawing.Point(12, 13);
            this.m_txtRoot.Name = "m_txtRoot";
            this.m_txtRoot.ReadOnly = true;
            this.m_txtRoot.Size = new System.Drawing.Size(601, 20);
            this.m_txtRoot.TabIndex = 1;
            this.m_txtRoot.TextLabel = "Select folder search starting point...";
            this.m_txtRoot.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_txtRoot_DragDrop);
            this.m_txtRoot.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_txtRoot_DragEnter);
            // 
            // m_btn_SelectRoot
            // 
            this.m_btn_SelectRoot.AccessibleDescription = "Select starting\\npoint folder.";
            this.m_btn_SelectRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btn_SelectRoot.Image = ((System.Drawing.Image)(resources.GetObject("m_btn_SelectRoot.Image")));
            this.m_btn_SelectRoot.Location = new System.Drawing.Point(613, 12);
            this.m_btn_SelectRoot.Name = "m_btn_SelectRoot";
            this.m_btn_SelectRoot.Size = new System.Drawing.Size(22, 22);
            this.m_btn_SelectRoot.TabIndex = 0;
            this.m_btn_SelectRoot.UseVisualStyleBackColor = true;
            this.m_btn_SelectRoot.Click += new System.EventHandler(this.m_btnSelectRoot_Click);
            // 
            // m_rtfStatus
            // 
            this.m_rtfStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_rtfStatus.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_rtfStatus.Location = new System.Drawing.Point(12, 40);
            this.m_rtfStatus.Name = "m_rtfStatus";
            this.m_rtfStatus.ReadOnly = true;
            this.m_rtfStatus.Size = new System.Drawing.Size(623, 477);
            this.m_rtfStatus.TabIndex = 2;
            this.m_rtfStatus.Text = "";
            this.m_rtfStatus.WordWrap = false;
            this.m_rtfStatus.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.m_rtfStatus_LinkClicked);
            // 
            // m_btnGo
            // 
            this.m_btnGo.AccessibleDescription = "Begin VideoLibrarian\\nfolder configuration.";
            this.m_btnGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnGo.Location = new System.Drawing.Point(479, 523);
            this.m_btnGo.Name = "m_btnGo";
            this.m_btnGo.Size = new System.Drawing.Size(75, 23);
            this.m_btnGo.TabIndex = 3;
            this.m_btnGo.Text = "Go";
            this.m_btnGo.UseVisualStyleBackColor = true;
            this.m_btnGo.Click += new System.EventHandler(this.m_btnGo_Click);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.AccessibleDescription = "Exit application.";
            this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnCancel.Location = new System.Drawing.Point(560, 523);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
            this.m_btnCancel.TabIndex = 4;
            this.m_btnCancel.Text = "Exit";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
            // 
            // m_btnAbout
            // 
            this.m_btnAbout.AccessibleDescription = "This application description\\nand instructions.";
            this.m_btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnAbout.Location = new System.Drawing.Point(12, 523);
            this.m_btnAbout.Name = "m_btnAbout";
            this.m_btnAbout.Size = new System.Drawing.Size(75, 23);
            this.m_btnAbout.TabIndex = 5;
            this.m_btnAbout.Text = "About";
            this.m_btnAbout.UseVisualStyleBackColor = true;
            this.m_btnAbout.Click += new System.EventHandler(this.m_btnAbout_Click);
            // 
            // m_btnManualConfig
            // 
            this.m_btnManualConfig.AccessibleDescription = "Create/edit properties for a single video.";
            this.m_btnManualConfig.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.m_btnManualConfig.AutoSize = true;
            this.m_btnManualConfig.Location = new System.Drawing.Point(232, 523);
            this.m_btnManualConfig.Name = "m_btnManualConfig";
            this.m_btnManualConfig.Size = new System.Drawing.Size(117, 23);
            this.m_btnManualConfig.TabIndex = 6;
            this.m_btnManualConfig.Text = "Edit Video Properties";
            this.m_btnManualConfig.UseVisualStyleBackColor = true;
            this.m_btnManualConfig.Click += new System.EventHandler(this.m_btnManualConfig_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 558);
            this.Controls.Add(this.m_btnManualConfig);
            this.Controls.Add(this.m_btnAbout);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnGo);
            this.Controls.Add(this.m_rtfStatus);
            this.Controls.Add(this.m_btn_SelectRoot);
            this.Controls.Add(this.m_txtRoot);
            this.Icon = global::VideoOrganizer.Properties.Resources.favicon;
            this.MinimumSize = new System.Drawing.Size(278, 141);
            this.Name = "FormMain";
            this.Text = "VideoLibrarian Folder Organizer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private VideoOrganizer.LabeledTextBox m_txtRoot;
        private System.Windows.Forms.Button m_btn_SelectRoot;
        private System.Windows.Forms.RichTextBox m_rtfStatus;
        private System.Windows.Forms.Button m_btnGo;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.Button m_btnAbout;
        private System.Windows.Forms.Button m_btnManualConfig;
    }
}

