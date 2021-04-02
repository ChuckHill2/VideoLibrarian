using System;
using System.IO;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public class TileMediumLite : TileBase
    {
        public  PictureBox m_pbPoster = new PictureBox();
        private PictureBox m_pbImdbLink = new PictureBox();
        private RectangleRef m_lblTitle = new RectangleRef();
        private Watched m_chkWatched = new Watched();
        private RectangleRef m_lblPlot = new RectangleRef();

        public static TileMediumLite Create(MovieProperties mp)
        {
            var p = mp.PathPrefix + "M.png";  //keep name short to minimize full path from exceeding the maximum path length.

            if (File.Exists(p))
            {
                var bmp = GDI.FastLoadFromFile(p);
                var tile = new TileMediumLite(mp);
                var controls = new object[] { tile.m_lblTitle, tile.m_lblPlot, tile.m_chkWatched, tile.m_pbImdbLink };
                TileBase.LoadTileImage(tile, bmp, controls);
                return tile;
            }

            var tile2 = TileMedium.Create(mp,true);
            Control[] controls2 = new Control[] { tile2.m_lblTitle, tile2.m_lblPlot, tile2.m_chkWatched, tile2.m_pbImdbLink };
            TileBase.SaveTileImage(tile2, p, controls2, mp.MoviePosterImg);

            return TileMediumLite.Create(mp);
        }

        private TileMediumLite(MovieProperties mp)
        {
            #region InitializeComponent
            //Container and Control positions and size are undefined as they are explicitly set during the Create method. 
            //Forcing layout events more than once is a performance hit when we are creating thousands of tiles.

            SuspendLayout();
      
            m_pbPoster.BackColor = System.Drawing.Color.Transparent;
            m_pbPoster.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            m_pbPoster.Dock = System.Windows.Forms.DockStyle.Fill;
            m_pbPoster.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            m_pbPoster.Name = "m_pbPoster";
            m_pbPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
    
            m_pbImdbLink.Anchor = System.Windows.Forms.AnchorStyles.None;
            m_pbImdbLink.BackColor = System.Drawing.Color.Transparent;
            m_pbImdbLink.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            m_pbImdbLink.Cursor = System.Windows.Forms.Cursors.Hand;
            m_pbImdbLink.Image = global::VideoLibrarian.ResourceCache.ImdbIcon;
            m_pbImdbLink.Name = "m_pbImdbLink";
            m_pbImdbLink.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            m_pbImdbLink.Click += new System.EventHandler(m_pbImdbLink_Click);
            m_pbImdbLink.MouseEnter += new System.EventHandler(Highlight_MouseEnter);
            m_pbImdbLink.MouseLeave += new System.EventHandler(Highlight_MouseLeave);
            
            //m_chkWatched - nothing to do
            
            //AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            //AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            Controls.Add(m_pbPoster);
            Controls.Add(m_chkWatched);
            Controls.Add(m_pbImdbLink);
            Margin = new Padding(4);
            Name = "TileMediumLite";

            ResumeLayout(false);
            #endregion

            MovieProps = mp;
            IsVisible = true;
            AddVirtualControl(m_lblPlot, m_lblPlot_Click, mp.Plot, global::VideoLibrarian.ResourceCache.FontMedium);
            EventHandler click = base.m_lblTitle_Click;
            if (DisableTitleLink()) click = null;
            AddVirtualControl(m_lblTitle, click, mp.MovieName, global::VideoLibrarian.ResourceCache.FontLargeBold);

            m_chkWatched.CheckDate = mp.Watched;
            m_pbPoster.Visible = true;
        }

        public override void MouseEntered(bool visible)
        {
            //Diagnostics.WriteLine("{0}: PosterVisible={1}", MovieProps.MovieName, visible);

            //Don't toggle if summary popup is displayed.
            if (SummaryPopup.Visible) return;

            if (visible)
            {
                if (!m_pbPoster.Visible) m_pbPoster.Visible = true;
            }
            else
            {
                if (m_pbPoster.Visible) m_pbPoster.Visible = false;
            }

            base.MouseEntered(visible);
        }
    }
}
