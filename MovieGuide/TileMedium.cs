using System;

namespace MovieGuide
{
    public partial class TileMedium : TileBase
    {
        //

        public static TileMedium Create(MovieProperties mp, bool ctrlImageOnly = false)
        {
            var ctrl = new TileMedium(mp);

            if (ctrlImageOnly)
            {
                ctrl.m_tblLayout.Visible = true;
                ctrl.m_pbPoster.Visible = false;

                ctrl.m_chkWatched.Enabled = false; //Must be BEFORE setting CheckDate;
                ctrl.m_chkWatched.CheckDate = DateTime.MinValue;
                ctrl.m_pbImdbLink.Image = null;
            }

            return ctrl;
        }

        private TileMedium(MovieProperties mp)
        {
            InitializeComponent();

            #region Event Handlers -- Cannot use in designer code as handlers are in base class.
            this.m_lblPlot.Click += new System.EventHandler(m_lblPlot_Click);
            this.m_lblPlot.MouseEnter += new System.EventHandler(Highlight_MouseEnter);
            this.m_lblPlot.MouseLeave += new System.EventHandler(Highlight_MouseLeave);
            this.m_lblTitle.Click += new System.EventHandler(m_lblTitle_Click);
            this.m_lblTitle.MouseEnter += new System.EventHandler(Highlight_MouseEnter);
            this.m_lblTitle.MouseLeave += new System.EventHandler(Highlight_MouseLeave);
            this.m_pbImdbLink.Click += new System.EventHandler(m_pbImdbLink_Click);
            this.m_pbImdbLink.MouseEnter += new System.EventHandler(Highlight_MouseEnter);
            this.m_pbImdbLink.MouseLeave += new System.EventHandler(Highlight_MouseLeave);
            #endregion

            MovieProps = mp;
            IsVisible = true;
            MaybeDisableTitleLink(m_lblTitle);
    
            m_lblTitle.Text = mp.MovieName;

            if (mp.Season > 0) m_lblSeasonEpisode.Text = string.Format("Season {0}, Episode {1}",mp.Season, mp.Episode);
            //else { m_tblLayout.RowStyles[1].SizeType = SizeType.Absolute; m_tblLayout.RowStyles[1].Height = 0; }
            else m_lblSeasonEpisode.Text = mp.MovieClass;

            if (mp.Genre != null && mp.Genre.Length > 0) m_lblGenre.Text = string.Join(" / ", mp.Genre);
            if (mp.ReleaseDate.Day == 1 && mp.ReleaseDate.Month == 1) m_lblYear.Text = mp.Year.ToString();
            else m_lblYear.Text = mp.ReleaseDate.ToString("MMM dd, yyyy");

            if (mp.MovieRating == 0) m_pnlRating.Visible = false;
            else m_pnlRating.Value = mp.MovieRating;

            m_lblDuration.Text = mp.EpisodeCount > 0 ? string.Format("{0} eps", mp.EpisodeCount) : string.Format("{0} min", mp.Runtime == 0 ? "?" : mp.Runtime.ToString());
            m_lblPlot.Text = mp.Plot;
            m_pbPoster.Image = mp.MoviePosterImg;

            m_chkWatched.CheckDate = mp.Watched;

            m_tblLayout.Visible = false;
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
                if (m_tblLayout.Visible) m_tblLayout.Visible = false;
            }
            else
            {
                if (m_pbPoster.Visible) m_pbPoster.Visible = false;
                if (!m_tblLayout.Visible) m_tblLayout.Visible = true;
            }

            base.MouseEntered(visible);
        }
    }
}
