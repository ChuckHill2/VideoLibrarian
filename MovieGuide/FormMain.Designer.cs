namespace MovieGuide
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
            this.m_MenuStrip = new System.Windows.Forms.MenuStrip();
            this.m_miBack = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miFile = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miStatusLog = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miDivider1 = new System.Windows.Forms.ToolStripSeparator();
            this.m_miExit = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miView = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miViewSmall = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miViewMedium = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miViewLarge = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miSort = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.m_miAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.m_flowPanel = new MovieGuide.MyFlowLayoutPanel();
            this.m_MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_MenuStrip
            // 
            this.m_MenuStrip.BackColor = System.Drawing.Color.AliceBlue;
            this.m_MenuStrip.BackgroundImage = global::MovieGuide.Properties.Resources.MenuBarGradient;
            this.m_MenuStrip.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_MenuStrip.Font = new System.Drawing.Font("Tahoma", 11F);
            this.m_MenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.m_MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_miBack,
            this.m_miFile,
            this.m_miView,
            this.m_miSort,
            this.m_miFilter,
            this.m_miAbout});
            this.m_MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.m_MenuStrip.Name = "m_MenuStrip";
            this.m_MenuStrip.Size = new System.Drawing.Size(454, 28);
            this.m_MenuStrip.TabIndex = 1;
            this.m_MenuStrip.Text = "menuStrip1";
            // 
            // m_miBack
            // 
            this.m_miBack.AutoToolTip = true;
            this.m_miBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_miBack.Enabled = false;
            this.m_miBack.Image = global::MovieGuide.Properties.Resources.Return;
            this.m_miBack.Name = "m_miBack";
            this.m_miBack.Size = new System.Drawing.Size(32, 24);
            this.m_miBack.Text = "Back";
            this.m_miBack.Click += new System.EventHandler(this.m_miBack_Click);
            // 
            // m_miFile
            // 
            this.m_miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_miSettings,
            this.m_miStatusLog,
            this.m_miDivider1,
            this.m_miExit});
            this.m_miFile.Name = "m_miFile";
            this.m_miFile.Size = new System.Drawing.Size(40, 24);
            this.m_miFile.Text = "File";
            // 
            // m_miSettings
            // 
            this.m_miSettings.Image = global::MovieGuide.Properties.Resources.Settings16;
            this.m_miSettings.Name = "m_miSettings";
            this.m_miSettings.Size = new System.Drawing.Size(170, 26);
            this.m_miSettings.Text = "Settings…";
            this.m_miSettings.Click += new System.EventHandler(this.m_miSettings_Click);
            // 
            // m_miStatusLog
            // 
            this.m_miStatusLog.Image = global::MovieGuide.Properties.Resources.Notepad16;
            this.m_miStatusLog.Name = "m_miStatusLog";
            this.m_miStatusLog.Size = new System.Drawing.Size(170, 26);
            this.m_miStatusLog.Text = "Status Log...";
            this.m_miStatusLog.Click += new System.EventHandler(this.m_miStatusLog_Click);
            // 
            // m_miDivider1
            // 
            this.m_miDivider1.Name = "m_miDivider1";
            this.m_miDivider1.Size = new System.Drawing.Size(167, 6);
            // 
            // m_miExit
            // 
            this.m_miExit.Image = global::MovieGuide.Properties.Resources.Close16;
            this.m_miExit.Name = "m_miExit";
            this.m_miExit.Size = new System.Drawing.Size(170, 26);
            this.m_miExit.Text = "Exit";
            this.m_miExit.Click += new System.EventHandler(this.m_miExit_Click);
            // 
            // m_miView
            // 
            this.m_miView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_miViewSmall,
            this.m_miViewMedium,
            this.m_miViewLarge});
            this.m_miView.Name = "m_miView";
            this.m_miView.Size = new System.Drawing.Size(49, 24);
            this.m_miView.Text = "View";
            // 
            // m_miViewSmall
            // 
            this.m_miViewSmall.Image = global::MovieGuide.Properties.Resources.CheckMark16;
            this.m_miViewSmall.Name = "m_miViewSmall";
            this.m_miViewSmall.Size = new System.Drawing.Size(165, 26);
            this.m_miViewSmall.Text = "Small Tiles";
            this.m_miViewSmall.Click += new System.EventHandler(this.m_miView_Click);
            // 
            // m_miViewMedium
            // 
            this.m_miViewMedium.Name = "m_miViewMedium";
            this.m_miViewMedium.Size = new System.Drawing.Size(165, 26);
            this.m_miViewMedium.Text = "Medium Tiles";
            this.m_miViewMedium.Click += new System.EventHandler(this.m_miView_Click);
            // 
            // m_miViewLarge
            // 
            this.m_miViewLarge.Name = "m_miViewLarge";
            this.m_miViewLarge.Size = new System.Drawing.Size(165, 26);
            this.m_miViewLarge.Text = "Large Tiles";
            this.m_miViewLarge.Click += new System.EventHandler(this.m_miView_Click);
            // 
            // m_miSort
            // 
            this.m_miSort.Name = "m_miSort";
            this.m_miSort.Size = new System.Drawing.Size(46, 24);
            this.m_miSort.Text = "Sort";
            this.m_miSort.Click += new System.EventHandler(this.m_miSort_Click);
            // 
            // m_miFilter
            // 
            this.m_miFilter.Name = "m_miFilter";
            this.m_miFilter.Size = new System.Drawing.Size(50, 24);
            this.m_miFilter.Text = "Filter";
            this.m_miFilter.Click += new System.EventHandler(this.m_miFilter_Click);
            // 
            // m_miAbout
            // 
            this.m_miAbout.Name = "m_miAbout";
            this.m_miAbout.Size = new System.Drawing.Size(58, 24);
            this.m_miAbout.Text = "About";
            this.m_miAbout.Click += new System.EventHandler(this.m_miAbout_Click);
            // 
            // m_flowPanel
            // 
            this.m_flowPanel.BackColor = System.Drawing.Color.LightGray;
            this.m_flowPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.m_flowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_flowPanel.Location = new System.Drawing.Point(0, 28);
            this.m_flowPanel.Margin = new System.Windows.Forms.Padding(0);
            this.m_flowPanel.Name = "m_flowPanel";
            this.m_flowPanel.Size = new System.Drawing.Size(454, 159);
            this.m_flowPanel.TabIndex = 0;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.ClientSize = new System.Drawing.Size(454, 187);
            this.Controls.Add(this.m_flowPanel);
            this.Controls.Add(this.m_MenuStrip);
            this.Icon = global::MovieGuide.Properties.Resources.favicon;
            this.MainMenuStrip = this.m_MenuStrip;
            this.Name = "FormMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Movie Guide";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.m_MenuStrip.ResumeLayout(false);
            this.m_MenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MovieGuide.MyFlowLayoutPanel m_flowPanel;
        private System.Windows.Forms.MenuStrip m_MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem m_miFile;
        private System.Windows.Forms.ToolStripMenuItem m_miSettings;
        private System.Windows.Forms.ToolStripSeparator m_miDivider1;
        private System.Windows.Forms.ToolStripMenuItem m_miExit;
        private System.Windows.Forms.ToolStripMenuItem m_miView;
        private System.Windows.Forms.ToolStripMenuItem m_miViewSmall;
        private System.Windows.Forms.ToolStripMenuItem m_miViewMedium;
        private System.Windows.Forms.ToolStripMenuItem m_miViewLarge;
        private System.Windows.Forms.ToolStripMenuItem m_miSort;
        private System.Windows.Forms.ToolStripMenuItem m_miFilter;
        private System.Windows.Forms.ToolStripMenuItem m_miAbout;
        private System.Windows.Forms.ToolStripMenuItem m_miBack;
        private System.Windows.Forms.ToolStripMenuItem m_miStatusLog;

    }
}

