
namespace VideoOrganizer
{
    partial class CustomGroupsEditor
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
            this.m_lbGroups = new ChuckHill2.Forms.EditListBox();
            this.SuspendLayout();
            // 
            // m_lbGroups
            // 
            this.m_lbGroups.BackColor = System.Drawing.Color.AliceBlue;
            this.m_lbGroups.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_lbGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_lbGroups.InvalidChars = ";#";
            this.m_lbGroups.Location = new System.Drawing.Point(0, 0);
            this.m_lbGroups.Name = "m_lbGroups";
            this.m_lbGroups.Size = new System.Drawing.Size(178, 211);
            this.m_lbGroups.TabIndex = 0;
            // 
            // CustomGroupsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(178, 211);
            this.Controls.Add(this.m_lbGroups);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "CustomGroupsEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "User-Defined Groups";
            this.ResumeLayout(false);

        }

        #endregion
        private ChuckHill2.Forms.EditListBox m_lbGroups;
    }
}
