namespace VideoLibrarian
{
    partial class Rating
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Rating));
            this.m_pbStar = new System.Windows.Forms.PictureBox();
            this.m_lblRating = new System.Windows.Forms.Label();
            this.m_lbl10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.m_pbStar)).BeginInit();
            this.SuspendLayout();
            // 
            // m_pbStar
            // 
            this.m_pbStar.Dock = System.Windows.Forms.DockStyle.Left;
            this.m_pbStar.Image = global::VideoLibrarian.ResourceCache.RatingsStar;
            this.m_pbStar.Location = new System.Drawing.Point(0, 0);
            this.m_pbStar.Margin = new System.Windows.Forms.Padding(0);
            this.m_pbStar.Name = "m_pbStar";
            this.m_pbStar.Size = new System.Drawing.Size(20, 16);
            this.m_pbStar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.m_pbStar.TabIndex = 0;
            this.m_pbStar.TabStop = false;
            // 
            // m_lblRating
            // 
            this.m_lblRating.AutoSize = true;
            this.m_lblRating.Dock = System.Windows.Forms.DockStyle.Left;
            this.m_lblRating.Font = new System.Drawing.Font("Lucida Sans", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblRating.Location = new System.Drawing.Point(20, 0);
            this.m_lblRating.Margin = new System.Windows.Forms.Padding(0);
            this.m_lblRating.Name = "m_lblRating";
            this.m_lblRating.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.m_lblRating.Size = new System.Drawing.Size(27, 15);
            this.m_lblRating.TabIndex = 1;
            this.m_lblRating.Text = "6.5";
            // 
            // m_lbl10
            // 
            this.m_lbl10.AutoSize = true;
            this.m_lbl10.Dock = System.Windows.Forms.DockStyle.Left;
            this.m_lbl10.Font = new System.Drawing.Font("Lucida Sans", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lbl10.ForeColor = System.Drawing.Color.LightSlateGray;
            this.m_lbl10.Location = new System.Drawing.Point(47, 0);
            this.m_lbl10.Margin = new System.Windows.Forms.Padding(0);
            this.m_lbl10.Name = "m_lbl10";
            this.m_lbl10.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.m_lbl10.Size = new System.Drawing.Size(23, 16);
            this.m_lbl10.TabIndex = 2;
            this.m_lbl10.Text = "∕10";
            // 
            // Rating
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.m_lbl10);
            this.Controls.Add(this.m_lblRating);
            this.Controls.Add(this.m_pbStar);
            this.Font = new System.Drawing.Font("Lucida Sans", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MaximumSize = new System.Drawing.Size(999, 16);
            this.MinimumSize = new System.Drawing.Size(73, 16);
            this.Name = "Rating";
            this.Size = new System.Drawing.Size(73, 16);
            ((System.ComponentModel.ISupportInitialize)(this.m_pbStar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox m_pbStar;
        private System.Windows.Forms.Label m_lblRating;
        private System.Windows.Forms.Label m_lbl10;
    }
}
