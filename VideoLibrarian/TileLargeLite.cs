//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="TileLargeLite.cs" company="Chuck Hill">
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
    public class TileLargeLite : TileBase
    {
        private RectangleRef m_lblTitle = new RectangleRef();
        private RectangleRef m_lblLocation = new RectangleRef();
        private PictureBox m_pbImdbLink = new PictureBox();
        private Watched m_chkWatched = new Watched();
        private RectangleRef m_pbPoster = new RectangleRef();
        private RectangleRef m_lblPlot = new RectangleRef();

        public static TileLargeLite Create(MovieProperties mp)
        {
            var p = mp.PathPrefix + "L.png";  //keep name short to minimize full path from exceeding the maximum path length.

            if (File.Exists(p))
            {
                var bmp = GDI.FastLoadFromFile(p);
                //var bmp = GDI.FastLoadFromFileStream(p);
                var tile = new TileLargeLite(mp);
                var controls = new object[] { tile.m_pbPoster, tile.m_lblTitle, tile.m_lblPlot, tile.m_lblLocation, tile.m_pbImdbLink, tile.m_chkWatched };
                TileBase.LoadTileImage(tile, bmp, controls);
                return tile;
            }

            var tile2 = TileLarge.Create(mp, true);
            var controls2 = new Control[] { tile2.m_pbPoster, tile2.m_lblTitle, tile2.m_lblPlot, tile2.m_lblLocation, tile2.m_pbImdbLink, tile2.m_chkWatched };
            TileBase.SaveTileImage(tile2, p, controls2);

            return TileLargeLite.Create(mp);
        }

        private TileLargeLite(MovieProperties mp)
        {
            #region InitializeComponent
            //Container and Control positions and size are undefined as they are explicitly set during the Create method. 
            //Forcing layout events more than once is a performance hit when we are creating thousands of tiles.

            SuspendLayout();
       
            m_pbImdbLink.BackColor = System.Drawing.Color.Transparent;
            m_pbImdbLink.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            m_pbImdbLink.Cursor = System.Windows.Forms.Cursors.Hand;
            m_pbImdbLink.Image = global::VideoLibrarian.ResourceCache.ImdbIcon;
            m_pbImdbLink.Name = "m_pbImdbLink";
            m_pbImdbLink.Click += new System.EventHandler(m_pbImdbLink_Click);
            m_pbImdbLink.MouseEnter += new System.EventHandler(Highlight_MouseEnter);
            m_pbImdbLink.MouseLeave += new System.EventHandler(Highlight_MouseLeave);

            //m_chkWatched - nothing to do
   
            //AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            //AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            Controls.Add(m_pbImdbLink);
            Controls.Add(m_chkWatched);
            Margin = new Padding(0);
            Name = "TileLargeLite";

            ResumeLayout(false);
            #endregion

            MovieProps = mp;
            IsVisible = true;

            AddVirtualControl(m_pbPoster, this.m_pbPoster_Click);
            AddVirtualControl(m_lblPlot, m_lblPlot_Click, mp.Plot, global::VideoLibrarian.ResourceCache.FontMedium);
            AddVirtualControl(m_lblLocation, this.m_lblLocation_Click, Path.GetDirectoryName(mp.PropertiesPath), global::VideoLibrarian.ResourceCache.FontRegular);
            EventHandler click = base.m_lblTitle_Click;
            if (DisableTitleLink()) click = null;
            AddVirtualControl(m_lblTitle, click, mp.MovieName, global::VideoLibrarian.ResourceCache.FontLargeBold);

            m_chkWatched.CheckDate = mp.Watched;
        }

        /// <summary>
        /// Problem: Movie Location cannot be hardcoded into background image, because user may change folder names.
        /// Painting may not always occur leaving the location area on screen empty. (Due to double buffering??)
        /// Drawing the location string on the background image may solve the problem.
        /// See TileBase.OnPaint().
        /// </summary>
        /// <param name="e"></param>
        //protected override void OnLoad(EventArgs e)
        //{
        //    if (this.BackgroundImage == null) return;
        //    using (var g = Graphics.FromImage(this.BackgroundImage))
        //    {
        //        //// Close but a few stray pixels around edges. Looks a little odd.
        //        //g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        //        //RectangleF loc = m_lblLocation;
        //        //loc.Offset(1, 0);
        //        //g.DrawString(Path.GetDirectoryName(MovieProps.PropertiesPath),
        //        //    global::VideoLibrarian.ResourceCache.FontRegular,
        //        //    Brushes.Black, loc);

        //        //// Makes text bold
        //        //TextRenderer.DrawText(g, Path.GetDirectoryName(MovieProps.PropertiesPath),
        //        //    global::VideoLibrarian.ResourceCache.FontRegular, m_lblLocation,
        //        //    Color.Black, TextFormatFlags.WordBreak);
        //    }
        //}

        public override void MouseEntered(bool visible)
        {
            //Close detailed plot summary popup when mouse is moved off the tile.
            if (visible) SummaryPopup.Dispose();
            //Nothing else to do. Mainly used for poster flipping in SmallTile and LargeTile controls.
            base.MouseEntered(visible);
        }

        private void m_pbPoster_Click(object sender, EventArgs e)
        {
            SummaryPopup.Dispose();  //Don't capture plot summary popup. Updated poster image replaces part of it.
            ShowFullscreen(this, m_pbPoster);
        }
    }
}
