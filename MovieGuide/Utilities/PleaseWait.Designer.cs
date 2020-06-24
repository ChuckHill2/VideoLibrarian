namespace MovieGuide
{
    partial class PleaseWait
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
            this.m_lblMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_lblMessage
            // 
            this.m_lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_lblMessage.Location = new System.Drawing.Point(0, 0);
            this.m_lblMessage.Margin = new System.Windows.Forms.Padding(0);
            this.m_lblMessage.Name = "m_lblMessage";
            this.m_lblMessage.Padding = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.m_lblMessage.Size = new System.Drawing.Size(238, 74);
            this.m_lblMessage.TabIndex = 0;
            this.m_lblMessage.Text = "label1";
            this.m_lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.m_lblMessage.UseWaitCursor = true;
            // 
            // PleaseWait
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(238, 74);
            this.Controls.Add(this.m_lblMessage);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PleaseWait";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Please Wait...";
            this.UseWaitCursor = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label m_lblMessage;
    }
}