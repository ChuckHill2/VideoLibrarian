//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="TileLarge.cs" company="Chuck Hill">
// Copyright (c) 2020 Chuck Hill.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1
// of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// The GNU Lesser General Public License can be viewed at
// http://www.opensource.org/licenses/lgpl-license.php. If
// you unfamiliar with this license or have questions about
// it, here is an http://www.gnu.org/licenses/gpl-faq.html.
//
// All code and executables are provided "as is" with no warranty
// either express or implied. The author accepts no liability for
// any damage or loss of business that this product may cause.
// </copyright>
// <repository>https://github.com/ChuckHill2/VideoLibrarian</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
﻿using System;
using System.IO;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public partial class TileLarge : TileBase
    {
        //
        //

        public static TileLarge Create(MovieProperties mp, bool ctrlImageOnly = false)
        {
            var ctrl = new TileLarge(mp);

            if (ctrlImageOnly)
            {
                ctrl.m_lblLocation.Visible = false;
                ctrl.m_pbImdbLink.Image = null;
                ctrl.m_chkWatched.Enabled = false; //Must be BEFORE setting CheckDate;
                ctrl.m_chkWatched.CheckDate = DateTime.MinValue;
            }

            return ctrl;
        }

        private TileLarge(MovieProperties mp)
        {
            InitializeComponent();

            #region Event Handlers -- Cannot use in designer code as handlers are in base class.
            this.m_pbPoster.Click += new System.EventHandler(this.m_pbPoster_Click);

            this.m_lblTitle.Click += new System.EventHandler(m_lblTitle_Click);
            this.m_lblTitle.MouseEnter += new System.EventHandler(Highlight_MouseEnter);
            this.m_lblTitle.MouseLeave += new System.EventHandler(Highlight_MouseLeave);
            this.m_lblPlot.Click += new System.EventHandler(m_lblPlot_Click);
            this.m_lblPlot.MouseEnter += new System.EventHandler(Highlight_MouseEnter);
            this.m_lblPlot.MouseLeave += new System.EventHandler(Highlight_MouseLeave);
            this.m_lblLocation.Click += new System.EventHandler(m_lblLocation_Click);
            this.m_lblLocation.MouseEnter += new System.EventHandler(Highlight_MouseEnter);
            this.m_lblLocation.MouseLeave += new System.EventHandler(Highlight_MouseLeave);
            this.m_pbImdbLink.Click += new System.EventHandler(m_pbImdbLink_Click);
            this.m_pbImdbLink.MouseEnter += new System.EventHandler(Highlight_MouseEnter);
            this.m_pbImdbLink.MouseLeave += new System.EventHandler(Highlight_MouseLeave);
            #endregion

            MovieProps = mp;
            IsVisible = true;
            MaybeDisableTitleLink(m_lblTitle);
  
            m_pbPoster.Image = mp.MoviePosterImg;
            m_lblTitle.Text = mp.MovieName;

            m_lblClass.Text = mp.MovieClass;
            m_lblSeasonEpisode.Text = mp.Season > 0 ? string.Format("Season {0}, Episode {1}", mp.Season, mp.Episode) : string.Empty;
            if (mp.MovieRating == 0) m_pnlRating.Visible = false;
            else m_pnlRating.Value = mp.MovieRating;

            m_lblDuration.Text = mp.EpisodeCount > 0 ? string.Format("{0} eps", mp.EpisodeCount) : string.Format("{0} min", mp.Runtime == 0 ? "?" : mp.Runtime.ToString());
            
            if (mp.Genre !=null && mp.Genre.Length > 0) m_lblGenre.Text = string.Join(" / ", mp.Genre);
            if (mp.ReleaseDate.Day==1 && mp.ReleaseDate.Month==1)  m_lblReleaseDate.Text = mp.ReleaseDate.Year.ToString();
            else m_lblReleaseDate.Text = mp.ReleaseDate.ToDateString();

            const int maxplotlen = 517; //maximum string length that will fit in tile.
            var plot = mp.Plot.IsNullOrEmpty() ? mp.Summary : mp.Plot;
            if (plot.Length > maxplotlen) plot = plot.Substring(0, maxplotlen) + "…";
            m_lblPlot.Text = plot;

            if (mp.Creators.IsNullOrEmpty()) { tableLayoutPanel3.RowStyles[0].SizeType = SizeType.Absolute; tableLayoutPanel3.RowStyles[0].Height = 0; }
            else m_lblCreators.Text = mp.Creators;

            if (mp.Directors.IsNullOrEmpty()) { tableLayoutPanel3.RowStyles[1].SizeType = SizeType.Absolute; tableLayoutPanel3.RowStyles[1].Height = 0; }
            else m_lblDirectors.Text = mp.Directors;

            if (mp.Writers.IsNullOrEmpty()) { tableLayoutPanel3.RowStyles[2].SizeType = SizeType.Absolute; tableLayoutPanel3.RowStyles[2].Height = 0; }
            else m_lblWriters.Text = mp.Writers;

            if (mp.Cast.IsNullOrEmpty()) { tableLayoutPanel3.RowStyles[3].SizeType = SizeType.Absolute; tableLayoutPanel3.RowStyles[3].Height = 0; }
            else m_lblCast.Text = mp.Cast;

            //RowStyles[4] == m_pbDivider so there is nothing to do.

            if (mp.DisplayWidth==0) { tableLayoutPanel3.RowStyles[5].SizeType = SizeType.Absolute; tableLayoutPanel3.RowStyles[5].Height = 0; }
            m_lblDisplay.Text = string.Format("{0}x{1} ({2})", mp.DisplayWidth, mp.DisplayHeight, mp.DisplayRatio);

            m_lblLocation.Text = Path.GetDirectoryName(mp.PropertiesPath);
            m_chkWatched.CheckDate = mp.Watched;
        }

        public override void MouseEntered(bool visible)
        {
            //Close detailed plot summary popup when mouse is moved off the tile.
            if (visible) SummaryPopup.Dispose();
            //Nothing to do. For poster flipping in SmallTile and LargeTile controls.
            base.MouseEntered(visible);
        }

        private void m_pbPoster_Click(object sender, EventArgs e)
        {
            SummaryPopup.Dispose();  //Don't capture plot summary popup. Updated poster image replaces part of it.
            ShowFullscreen(this, m_pbPoster.Bounds);
        }
    }
}
