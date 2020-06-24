using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MovieGuide
{
    public class TileSmallLite : TileBase
    {
        public  PictureBox m_pbPoster = new PictureBox();
        private RectangleRef m_lblTitle = new RectangleRef();
        private PictureBox m_pbImdbLink = new PictureBox();

        public static TileSmallLite Create(MovieProperties mp)
        {
            var p = mp.PathPrefix + "S.png";  //keep name short to minimize full path from exceeding the maximum path length.

            if (File.Exists(p))
            {
                var bmp = GDI.FastLoadFromFile(p);
                var tile = new TileSmallLite(mp);
                object[] controls = new object[] { tile.m_lblTitle, tile.m_pbImdbLink };
                TileBase.LoadTileImage(tile, bmp, controls);
                return tile;
            }

            var tile2 = TileSmall.Create(mp, true);
            Control[] controls2 = new Control[] { tile2.m_lblTitle, tile2.m_pbImdbLink };
            TileBase.SaveTileImage(tile2, p, controls2, mp.MoviePosterImg);
     
            return TileSmallLite.Create(mp);
        }
 
        private TileSmallLite(MovieProperties mp)
        {
            #region InitializeComponent
            //Container and Control positions and size are undefined as they are explicitly set during the Create method. 
            //Forcing layout events more than once is a performance hit when we are creating thousands of tiles.
            
            SuspendLayout();

            m_pbPoster.BackColor = System.Drawing.Color.Transparent;
            m_pbPoster.Dock = System.Windows.Forms.DockStyle.Fill;
            m_pbPoster.Name = "m_pbPoster";
            m_pbPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            m_pbImdbLink.Anchor = System.Windows.Forms.AnchorStyles.None;
            m_pbImdbLink.BackColor = System.Drawing.Color.Transparent;
            m_pbImdbLink.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            m_pbImdbLink.Image = global::MovieGuide.ResourceCache.ImdbIcon;
            m_pbImdbLink.Name = "m_pbImdbLink";
            m_pbImdbLink.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            m_pbImdbLink.Cursor = System.Windows.Forms.Cursors.Hand;
            m_pbImdbLink.Click += new System.EventHandler(m_pbImdbLink_Click);
            m_pbImdbLink.MouseEnter += new System.EventHandler(Highlight_MouseEnter);
            m_pbImdbLink.MouseLeave += new System.EventHandler(Highlight_MouseLeave);

            //AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            //AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            Controls.Add(m_pbPoster);
            Controls.Add(m_pbImdbLink);
            Margin = new Padding(3);
            Name = "TileSmallLite";

            ResumeLayout(false);
            #endregion

            MovieProps = mp;
            IsVisible = true;

            EventHandler click = base.m_lblTitle_Click;
            if (DisableTitleLink()) click = null;
            AddVirtualControl(m_lblTitle, click, mp.MovieName, new Font("Lucida Sans", 10F, FontStyle.Bold));

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

        protected override void OnMouseClick(MouseEventArgs e)
        {
            Plot_Click(this, EventArgs.Empty);
            base.OnMouseClick(e);
        }

        private void Plot_Click(object sender, EventArgs ev)
        {
            SummaryPopup.Create(this, MovieProps.Summary, 1.15);
        }
    }
}
