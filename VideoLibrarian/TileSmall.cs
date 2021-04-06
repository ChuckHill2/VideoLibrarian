//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="TileSmall.cs" company="Chuck Hill">
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
using System.Windows.Forms;

namespace VideoLibrarian
{
    public partial class TileSmall : TileBase
    {        
        public static TileSmall Create(MovieProperties mp, bool ctrlImageOnly = false)
        {
            var ctrl = new TileSmall(mp);

            if (ctrlImageOnly)
            {
                ctrl.m_tblLayout.Visible = true;
                ctrl.m_pbPoster.Visible = false;

                ctrl.m_pbImdbLink.Image = null;
            }

            return ctrl;
        }

        private TileSmall(MovieProperties mp)
        {
            InitializeComponent();

            #region Event Handlers -- Cannot use in designer code as handlers are in base class.
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
            m_lblYear.Text = mp.ReleaseDate.ToString("d");

            if (mp.MovieRating == 0) m_pnlRating.Visible = false;
            else m_pnlRating.Value = mp.MovieRating;

            //m_lblDuration.Text = mp.EpisodeCount > 0 ? string.Format("{0} episodes", mp.EpisodeCount) : string.Format("{0} min", mp.Runtime == 0 ? "?" : mp.Runtime.ToString());

            if (mp.EpisodeCount > 0)
            {
                m_lblDuration.Text = string.Format("{0} episodes", mp.EpisodeCount);
            }
            else
            {
                if (mp.Season > 0)
                {
                    m_lblDuration.Text = string.Format("S{0:00}E{1:00}  {2} min", mp.Season, mp.Episode, mp.Runtime == 0 ? "?" : mp.Runtime.ToString());
                }
                else
                {
                    m_lblDuration.Text = string.Format("{0} min", mp.Runtime == 0 ? "?" : mp.Runtime.ToString());
                }
            }

            m_pbPoster.Image = mp.MoviePosterImg;

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
