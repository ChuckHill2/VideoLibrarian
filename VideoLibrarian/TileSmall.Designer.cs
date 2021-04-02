namespace VideoLibrarian
{
    partial class TileSmall
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TileSmall));
            this.m_pbPoster = new System.Windows.Forms.PictureBox();
            this.m_tblLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_lblTitle = new System.Windows.Forms.Label();
            this.m_lblYear = new System.Windows.Forms.Label();
            this.m_lblDuration = new System.Windows.Forms.Label();
            this.m_pbImdbLink = new System.Windows.Forms.PictureBox();
            this.m_pnlRating = new VideoLibrarian.Rating();
            ((System.ComponentModel.ISupportInitialize)(this.m_pbPoster)).BeginInit();
            this.m_tblLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_pbImdbLink)).BeginInit();
            this.SuspendLayout();
            //
            // m_pbPoster
            //
            this.m_pbPoster.BackColor = System.Drawing.Color.Black;
            this.m_pbPoster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_pbPoster.Location = new System.Drawing.Point(0, 0);
            this.m_pbPoster.Name = "m_pbPoster";
            this.m_pbPoster.Size = new System.Drawing.Size(153, 220);
            this.m_pbPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.m_pbPoster.TabIndex = 1;
            this.m_pbPoster.TabStop = false;
            //
            // m_tblLayout
            //
            this.m_tblLayout.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.InsetDouble;
            this.m_tblLayout.ColumnCount = 1;
            this.m_tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_tblLayout.Controls.Add(this.m_lblTitle, 0, 0);
            this.m_tblLayout.Controls.Add(this.m_lblYear, 0, 1);
            this.m_tblLayout.Controls.Add(this.m_lblDuration, 0, 3);
            this.m_tblLayout.Controls.Add(this.m_pbImdbLink, 0, 4);
            this.m_tblLayout.Controls.Add(this.m_pnlRating, 0, 2);
            this.m_tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_tblLayout.Font = new System.Drawing.Font("Lucida Sans", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_tblLayout.Location = new System.Drawing.Point(0, 0);
            this.m_tblLayout.Name = "m_tblLayout";
            this.m_tblLayout.RowCount = 5;
            this.m_tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.m_tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.m_tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.m_tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.m_tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.m_tblLayout.Size = new System.Drawing.Size(153, 220);
            this.m_tblLayout.TabIndex = 2;
            this.m_tblLayout.Click += new System.EventHandler(this.Plot_Click);
            //
            // m_lblTitle
            //
            this.m_lblTitle.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_lblTitle.AutoSize = true;
            this.m_lblTitle.UseMnemonic = false;
            this.m_lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.m_lblTitle.Font = new System.Drawing.Font("Lucida Sans", 10F, System.Drawing.FontStyle.Bold);
            this.m_lblTitle.ForeColor = System.Drawing.Color.Black;
            this.m_lblTitle.Location = new System.Drawing.Point(8, 15);
            this.m_lblTitle.Margin = new System.Windows.Forms.Padding(0, 12, 0, 12);
            this.m_lblTitle.Name = "m_lblTitle";
            this.m_lblTitle.Size = new System.Drawing.Size(136, 32);
            this.m_lblTitle.TabIndex = 0;
            this.m_lblTitle.Text = "Captain America - The First Avenger";
            //
            // m_lblYear
            //
            this.m_lblYear.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_lblYear.AutoSize = true;
            this.m_lblYear.UseMnemonic = false;
            this.m_lblYear.BackColor = System.Drawing.Color.Transparent;
            this.m_lblYear.Location = new System.Drawing.Point(35, 68);
            this.m_lblYear.Margin = new System.Windows.Forms.Padding(6);
            this.m_lblYear.Name = "m_lblYear";
            this.m_lblYear.Size = new System.Drawing.Size(83, 13);
            this.m_lblYear.TabIndex = 1;
            this.m_lblYear.Text = "01/01/2011";
            this.m_lblYear.Click += new System.EventHandler(this.Plot_Click);
            //
            // m_lblDuration
            //
            this.m_lblDuration.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_lblDuration.AutoSize = true;
            this.m_lblDuration.UseMnemonic = false;
            this.m_lblDuration.BackColor = System.Drawing.Color.Transparent;
            this.m_lblDuration.Location = new System.Drawing.Point(19, 129);
            this.m_lblDuration.Margin = new System.Windows.Forms.Padding(6);
            this.m_lblDuration.Name = "m_lblDuration";
            this.m_lblDuration.Size = new System.Drawing.Size(115, 13);
            this.m_lblDuration.TabIndex = 3;
            this.m_lblDuration.Text = "S02E02  122 min";
            this.m_lblDuration.Click += new System.EventHandler(this.Plot_Click);
            //
            // m_pbImdbLink
            //
            this.m_pbImdbLink.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_pbImdbLink.BackColor = System.Drawing.Color.Transparent;
            this.m_pbImdbLink.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.m_pbImdbLink.Image = global::VideoLibrarian.ResourceCache.ImdbIcon;
            this.m_pbImdbLink.Location = new System.Drawing.Point(43, 168);
            this.m_pbImdbLink.Name = "m_pbImdbLink";
            this.m_pbImdbLink.Size = new System.Drawing.Size(66, 32);
            this.m_pbImdbLink.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.m_pbImdbLink.TabIndex = 4;
            this.m_pbImdbLink.TabStop = false;
            //
            // m_pnlRating
            //
            this.m_pnlRating.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_pnlRating.AutoSize = true;
            this.m_pnlRating.BackColor = System.Drawing.Color.Transparent;
            this.m_pnlRating.Font = new System.Drawing.Font("Lucida Sans", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_pnlRating.Location = new System.Drawing.Point(41, 96);
            this.m_pnlRating.Margin = new System.Windows.Forms.Padding(6);
            this.m_pnlRating.MaximumSize = new System.Drawing.Size(999, 16);
            this.m_pnlRating.MinimumSize = new System.Drawing.Size(0, 18);
            this.m_pnlRating.Name = "m_pnlRating";
            this.m_pnlRating.Size = new System.Drawing.Size(70, 18);
            this.m_pnlRating.TabIndex = 5;
            this.m_pnlRating.Value = 6.5F;
            this.m_pnlRating.Click += new System.EventHandler(this.Plot_Click);
            //
            // TileSmall
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_tblLayout);
            this.Controls.Add(this.m_pbPoster);
            this.Font = new System.Drawing.Font("Lucida Sans", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "TileSmall";
            this.Size = new System.Drawing.Size(153, 220);
            ((System.ComponentModel.ISupportInitialize)(this.m_pbPoster)).EndInit();
            this.m_tblLayout.ResumeLayout(false);
            this.m_tblLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_pbImdbLink)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox m_pbPoster;
        private System.Windows.Forms.TableLayoutPanel m_tblLayout;
        private System.Windows.Forms.Label m_lblYear;
        private System.Windows.Forms.Label m_lblDuration;
        private Rating m_pnlRating;
        public System.Windows.Forms.Label m_lblTitle;
        public System.Windows.Forms.PictureBox m_pbImdbLink;

    }
}
