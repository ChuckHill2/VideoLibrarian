//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="TileBase.cs" company="Chuck Hill">
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VideoLibrarian
{
    /// <summary>
    /// Base interface for all tiles (small,medium,large, in both template & light flavors)
    /// </summary>
    public interface ITile
    {
        /// <summary>
        /// Called by FormMain.mouseHook_MouseMove() when mouse cursor enters or 
        /// leaves a container tile. Child controls do not capture and mask the 
        /// mouse movement. This method must be overridden in order to do something.
        /// </summary>
        /// <param name="visible">True when enters tile. False when mouse leaves tile</param>
        void MouseEntered(bool visible);

        /// <summary>
        /// Called by FormMain.mouseHook_MouseLeave() when mouse cursor leaves the app to reset any text highlights.
        /// </summary>
        void MouseLeftApp();

        /// <summary>
        /// Tile visibility in gallery. May be safely set asynchronously via non-GUI threads.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets movie properties associated with this tile.
        /// </summary>
        MovieProperties MovieProps { get; }
    }

    /// <summary>
    /// Base class for all tiles (small,medium,large, in both template and light flavors).
    /// <para>&#160;</para>
    /// Remarks: One CANNOT set focus to a container control, only controls within it. This causes a lot
    /// glitchy problems with scrolling in the parent container control because we cannot set
    /// focus to the tile. The ONLY solution is to switch the base class to Control.
    /// Unfortunately the designer will no longer work unless we switch back to UserControl.
    /// <para>&#160;</para>
    /// See: httpx://stackoverflow.com/questions/3562235/panel-not-getting-focus/3562449#3562449<para/>
    /// See: httpx://stackoverflow.com/questions/39471972/how-do-i-set-focus-to-a-usercontrol
    /// <para>&#160;</para>
    /// Note: Ever since we created our own scrollable container, we no longer need tiles (aka containers) 
    ///       to have focus. The new scrollable container maintains the 'active' tile.
    /// </summary>
    public class TileBase : UserControl, ITile
    {
        //Determine if tile class is a TileXXXLite tile. 
        //Must use normal painting when generating a background image from a template tile.
        //Plus 'virtual' controls are not used in template tiles.
        //The sole purpose of template tiles is to make a background image for the normal 'Lite' tiles.
        private bool IsLiteTile = true;

        // When triggering paint of the virtual controls via Invalidate(vcRect,false), it always unnecessarily repaints the entire panel!
        // We temporarily disable OnPaintBackground() and paint ONLY the virtual control background during OnPaint().
        // Repainting the real control backgrounds, properly repaints ONLY the control background as expected.
        private bool VirtualPaint = false; 

        public TileBase()
        {
            IsLiteTile = this.GetType().Name.EndsWith("Lite");
        }

        private bool _mouseMoveEntered = false; //used exclusively by OnMouseMove()
        private List<VirtualControl> VirtualControls = new List<VirtualControl>();
        private struct VirtualControl 
        {
            public RectangleRef Bounds; 
            public EventHandler Click; 
            public string Text; 
            public Font Font; 
            public Color Normal; 
            public Color Highlight; 
        }
        private int CurrentVirtualIndex = -1;

        /// <summary>
        /// Add a rectangle to be treated as a virtual control (e.g. clickable area).
        /// </summary>
        /// <param name="bounds">Virtual rectangle bounds to add.</param>
        /// <param name="click">Virtual rectangle mouse click handler.</param>
        /// <param name="text">Optional virtual rectangle text.</param>
        /// <param name="font">Optional font used to draw text (if any).</param>
        protected void AddVirtualControl(RectangleRef bounds, EventHandler click, string text=null, Font font=null)
        {
            if (text != null && font == null) throw new ArgumentNullException("Font cannot be null when Text is a valid string.");

            VirtualControls.Add(new VirtualControl() 
            {
                Bounds = bounds, 
                Click = click, 
                Text = text,
                Font = font,
                Normal    = (click == null ? Color.Gray : Color.Black),
                Highlight = (click == null ? Color.Gray : Color.LightSlateGray)
            });

            //Resize the virtual rectangle surrounding the text to the actual size of the text.
            if (text != null)
            {
                //The RectangleRef content is set later, so we update the text bounds then.
                bounds.HeightChanged += bounds_Changed;  //height is always the last value changed
                bounds.Tag = VirtualControls[VirtualControls.Count - 1];
            }
        }

        void bounds_Changed(RectangleRef bounds, int oldHeight, int newHeight)
        {
            bounds.HeightChanged -= bounds_Changed; //only once
            var vc = (VirtualControl)bounds.Tag;
            bounds.Tag = null;    //tidy: no longer needed.
            var siz = TextRenderer.MeasureText(vc.Text, vc.Font, bounds.Size, TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            bounds.Size = siz;
        }

        /// <summary>
        /// Remove a rectangle that is treated aa a virtual control.
        /// </summary>
        /// <param name="bounds">Virtual rectangle bounds to remove.</param>
        protected void RemoveVirtualControl(RectangleRef bounds)
        {
            for (int i = VirtualControls.Count-1; i >= 0; i--)
            {
                if (bounds != VirtualControls[i].Bounds) continue;
                VirtualControls.RemoveAt(i);
                break;
            }
        }

        /// <summary>
        /// Occurs when the mouse pointer is moves over virtual child control. When
        /// the mouse moves over a virtual control rectangle, the cursor is changed
        /// to a 'Hand' picker and the event is NOT passed on to base mouse handler.
        /// </summary>
        /// <param name="e">The mouse event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            for (int i = 0; i < VirtualControls.Count; i++)
            {
                if (VirtualControls[i].Bounds.Contains(e.Location))
                {
                    if (_mouseMoveEntered) return;
                    _mouseMoveEntered = true;
                    VirtualControl vc;

                    CurrentVirtualIndex = i;
                    vc = VirtualControls[CurrentVirtualIndex];
                    if (vc.Click != null)
                    {
                        this.Cursor = Cursors.Hand;
                        if (vc.Text == null) return;
                        VirtualPaint = true;
                        Invalidate(vc.Bounds, false);
                    }
                    return;
                }
            }

            if (_mouseMoveEntered)
            {
                _mouseMoveEntered = false;
                var vc = VirtualControls[CurrentVirtualIndex];
                if (vc.Click != null)
                {
                    this.Cursor = Cursors.Default;
                    VirtualPaint = true;
                    Invalidate(vc.Bounds, false);
                }
                CurrentVirtualIndex = -1;
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Occurs when this control is clicked by the mouse. When a virtual 
        /// control rectangle is clicked, the associated click handler is 
        /// called and the event is NOT passed on to the base mouse handler.
        /// </summary>
        /// <param name="e">The mouse event data.</param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (CurrentVirtualIndex!=-1)
            {
                var vc = VirtualControls[CurrentVirtualIndex];
                if (vc.Click != null) vc.Click(this, EventArgs.Empty);
                return;
            }

            base.OnMouseClick(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!IsLiteTile)
            {
                base.OnPaintBackground(e);
                return;
            }

            if (VirtualPaint)
            {
                VirtualPaint = false;
                //return;  //a little too aggressive...:-\
            }

            //Manually paint background on large tiles to stop flickering when moving cursor off the poster (only large tiles have a background image).
            if (this.BackgroundImage != null)
            {
                var rc = this.ClientRectangle;
                rc.Intersect(e.ClipRectangle);
                e.Graphics.DrawImage(this.BackgroundImage, rc, rc, GraphicsUnit.Pixel);
                Diagnostics.WriteLine($"TileBase.OnPaintBackground: {{{rc.X},{rc.Y},{rc.Width},{rc.Height}}} {MovieProps.MovieName}");
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!IsLiteTile) return;

            Diagnostics.WriteLine($"TileBase.OnPaint: {{{e.ClipRectangle.X},{e.ClipRectangle.Y},{e.ClipRectangle.Width},{e.ClipRectangle.Height}}} {MovieProps.MovieName}");

            for (int i = 0; i < VirtualControls.Count; i++)
            {
                var vc = VirtualControls[i];
                if (vc.Text == null) continue;
                if (!e.ClipRectangle.IntersectsWith(vc.Bounds)) continue;
                var clr = i == CurrentVirtualIndex ? vc.Highlight : vc.Normal;
                e.Graphics.DrawImage(this.BackgroundImage, vc.Bounds, vc.Bounds, GraphicsUnit.Pixel);
                TextRenderer.DrawText(e.Graphics, vc.Text, vc.Font, vc.Bounds, clr, TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            }
        }

        /// <summary>
        /// Occurs when the mouse pointer is over the control and a mouse 
        /// button is pressed. It first sets focus to this control and then
        /// passes the event to the base mouse handler.
        /// </summary>
        /// <param name="e">The mouse event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Gets the required creation parameters when the control handle is created.
        /// </summary>
        protected override CreateParams CreateParams { get { var cp = base.CreateParams; cp.ExStyle |= 0x02000000; return cp; } } //WS_EX_COMPOSITED

        /// <summary>
        /// Returns the full movie name that represents the current tile.
        /// </summary>
        /// <returns>The full movie name that represents the current tile.</returns>
        public override string ToString() { return MovieProps==null ? base.ToString(): MovieProps.ToString(); }

        /// <summary>
        /// Called by FormMain.mouseHook_MouseMove() when mouse cursor enters or 
        /// leaves a container tile. Child controls do not capture and mask the 
        /// mouse movement. This method must be overridden in order to do something.
        /// </summary>
        /// <param name="visible">True when enters tile. False when mouse leaves tile</param>
        public virtual void MouseEntered(bool visible)
        {
        }

        /// <summary>
        /// Called by FormMain.mouseHook_MouseLeave() when mouse cursor leaves the app to reset any text highlights.
        /// </summary>
        public void MouseLeftApp()
        {
            OnMouseMove(new MouseEventArgs(MouseButtons.None,0,32767,32767,0));
        }

        /// <summary>
        /// Tile visibility in gallery. May be safely set asynchronously via non-GUI threads.
        /// </summary>
        public bool IsVisible 
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                var ei = ScrollPanel.GetExtraInfo(this);
                if (ei!=null) ei.Visible = value;
                if (!this.InvokeRequired) this.Visible = value;
            }
        }
        private bool _isVisible = true;
        protected override void OnParentChanged(EventArgs e) 
        {
            //For some mysterious reason, changing the visibility to true causes the child control 
            //positions to change! This occurs when the app starts up with a filtered list and then 
            //one un-filters the list. The newly visible child items are in wrong y-axis positions.

            Rectangle[] Bounds = null;
            if (this.Visible != IsVisible && !this.Visible)
            {
                Bounds = new Rectangle[Controls.Count];
                for (int i = 0; i < Controls.Count; i++)
                {
                    var c = Controls[i] as Control;
                    Bounds[i] = c.Bounds;
                }
            }

            this.Visible = IsVisible; 

            if (Bounds!=null)
            {
                for (int i = 0; i < Controls.Count; i++)
                {
                    var c = Controls[i] as Control;
                    if (c.Bounds != Bounds[i]) c.Bounds = Bounds[i];
                }
            }

            base.OnParentChanged(e); 
        }

        /// <summary>
        /// Gets movie properties associated with tile. May be safely set asynchronously via non-GUI threads.
        /// </summary>
        public MovieProperties MovieProps { get; protected set; }

        /// <summary>
        /// Set the active/highlight on the specified control.
        /// </summary>
        /// <param name="sender">The control wanting to be highlighted.</param>
        /// <param name="e">EventArgs.Empty</param>
        public static void Highlight_MouseEnter(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox; //always m_pbImdbLink
            if (pb != null)
            {
                pb.Image = global::VideoLibrarian.ResourceCache.ImdbIconHover;
                return;
            }

            Control c = sender as Control;
            c.ForeColor = Color.LightSlateGray;
        }

        /// <summary>
        /// Set the normal/unhighlight on the specified control.
        /// </summary>
        /// <param name="sender">The control wanting to be unhighlighted.</param>
        /// <param name="e">EventArgs.Empty</param>
        public static void Highlight_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox; //always m_pbImdbLink
            if (pb !=null)
            {
                pb.Image = global::VideoLibrarian.ResourceCache.ImdbIcon;
                return;
            }

            Control c = sender as Control;
            c.ForeColor = Color.Black;
        }

        /// <summary>
        /// Check if underlying movie file exists. If it doesn't, disable tile movie title so the title is not clickable.
        /// </summary>
        /// <param name="c">Title label control</param>
        protected void MaybeDisableTitleLink(Label c)
        {
            var tile = c.FindParent<ITile>();
            var mp = tile.MovieProps;
            //Not TVSeries root and movie not found, disable title click.
            if ((mp.Episodes == null || mp.Episodes.Count == 0) && (mp.MoviePath.IsNullOrEmpty() || !File.Exists(mp.MoviePath)))
            {
                c.Click -= m_lblTitle_Click;
                c.MouseEnter -= Highlight_MouseEnter;
                c.MouseLeave -= Highlight_MouseLeave;
                c.ForeColor = Color.Gray;
                c.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Check if underlying movie file exists. If it doesn't, disable tile movie title so the title is not clickable.
        /// Used for virtual title label.
        /// </summary>
        protected bool DisableTitleLink()
        {
            var mp = this.MovieProps;
            //Not TVSeries root and movie not found, disable title click.
            if ((mp.Episodes == null || mp.Episodes.Count == 0) && (mp.MoviePath.IsNullOrEmpty() || !File.Exists(mp.MoviePath)))
            {
                return true;
            }
            return false;
        }

        protected void m_lblTitle_Click(object sender, EventArgs e)
        {
            var mp = this.MovieProps;

            if (mp.Episodes != null && mp.Episodes.Count > 0)
            {
                //Must be a TV series so just load the TV Series tiles.
                FormMain.This.LoadTiles(mp.SortKey, mp.Episodes);
                return;
            }
            //This should never occur since title clicking is disabled in the tile. Well, just in case!
            if (mp.MoviePath.IsNullOrEmpty() || !File.Exists(mp.MoviePath)) { MiniMessageBox.ShowDialog(this, "Movie not found.", "Missing Movie", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            ProcessEx.OpenExec(FormMain.This.Settings.VideoPlayer, mp.MoviePath);
        }

        protected void m_lblPlot_Click(object sender, EventArgs ev)
        {
            SummaryPopup.Create(this, this.MovieProps.Summary.IsNullOrEmpty() ? this.MovieProps.Plot : this.MovieProps.Summary, 1.65);
        }

        protected void m_lblLocation_Click(object sender, EventArgs e)
        {
            var fn = Path.GetDirectoryName(this.MovieProps.PropertiesPath);
            if (fn.Contains(' ')) fn = String.Concat("\"", fn, "\"");
            Log.Write(Severity.Info, $"Exec: {fn}");

            Process.Start(fn);
        }

        protected void m_pbImdbLink_Click(object sender, EventArgs e)
        {
            if (this.MovieProps.UrlLink.IsNullOrEmpty() || this.MovieProps.UrlLink.StartsWith("file:///", StringComparison.OrdinalIgnoreCase)) return;
            ProcessEx.OpenExec(FormMain.This.Settings.Browser, this.MovieProps.UrlLink);
        }

        /// <summary>
        /// The tile label fields are fixed width and variable height. However, the max height must be limited so it will not push the following fields off the tile.
        /// The plot or title may exceed the maximum size for the field in the tile. This will truncate the string on word boundries so it will not overrun the field.
        /// </summary>
        /// <param name="s">String field to test.</param>
        /// <param name="width">Maximum width of field</param>
        /// <param name="maxLines">The maximum allowed number of lines for the field</param>
        /// <param name="f">Font used to draw the string</param>
        /// <returns>Possibly truncated string with an ellipsis appended.</returns>
        protected string FitInRect(string s, int width, int maxLines, Font f)
        {
            const TextFormatFlags flags = TextFormatFlags.HidePrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
            if (string.IsNullOrEmpty(s)) return s;
            s = s.Trim();
            if (s.Length==0 || !s.Contains(' ')) return s; //no word boundries!
            var maxHeight = maxLines * f.Height;
            if (TextRenderer.MeasureText(s, f, new Size(width, 99999), flags).Height <= maxHeight) return s; //string fits ok.

            var size = new Size(0, 99999);
            while (size.Height > maxHeight)
            {
                var i = s.LastIndexOf(' ');
                if (i == -1) return s;
                s = s.Substring(0, i) + "…";  //remove last word and try again.
                size = TextRenderer.MeasureText(s, f, new Size(width, 99999), flags);
            }

            return s;
        }

        /// <summary>
        /// Load bitmap into tile and position any child controls.
        /// Used exclusively in the Create() static method of each TileXxxLite tile.
        /// </summary>
        /// <param name="tile">TileXxxLite control.</param>
        /// <param name="bmp">Bitmap to load into tile</param>
        /// <param name="children">tile child controls to re-position.</param>
        protected static void LoadTileImage(Control tile, Bitmap bmp, object[] children)
        {
            var userComment = bmp.UserComment(); //bmp may already be disposed below, so do this first!

            if (tile is TileLargeLite)
            {
                tile.Bounds = new Rectangle(0, 0, bmp.Width, bmp.Height);
                tile.BackgroundImage = bmp;
                tile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            }
            else
            {
                //split foreground image and background image from bitmap
                tile.Bounds = new Rectangle(0, 0, bmp.Width / 2, bmp.Height);
                tile.BackgroundImage = bmp.Clone(new Rectangle(bmp.Width / 2, 0, bmp.Width / 2, bmp.Height), bmp.PixelFormat);
                tile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
                Bitmap moviePosterImg = bmp.Clone(new Rectangle(0, 0, bmp.Width / 2, tile.Height), bmp.PixelFormat);
                if (tile is TileSmallLite) ((TileSmallLite)tile).m_pbPoster.Image = moviePosterImg;
                else ((TileMediumLite)tile).m_pbPoster.Image = moviePosterImg;
                bmp.Dispose();
            }

            if (!string.IsNullOrEmpty(userComment))
            {
                var dimensions = userComment.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (dimensions.Length == children.Length)
                {
                    for (int i = 0; i < dimensions.Length; i++)
                    {
                        var dim = dimensions[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Int32.Parse(x)).ToArray();
                        if (children[i] is Control) ((Control)children[i]).Bounds = new Rectangle(dim[0], dim[1], dim[2], dim[3]);
                        else if (children[i] is RectangleRef) ((RectangleRef)children[i]).SetValues(dim[0], dim[1], dim[2], dim[3]);
                    }
                }
            }
        }

        /// <summary>
        /// Save bitmap image of tile to disk.
        /// Used exclusively in the Create() static method of each TileXxxLite tile.
        /// </summary>
        /// <param name="tile">TileXxx template control.</param>
        /// <param name="filename">filename to save tile image to.</param>
        /// <param name="children">Child controls of template tile to save positions from.</param>
        /// <param name="moviePosterImg">Optional: Image of movie poster. Required for TileSmall and TileMedium tiles only. Null assumes TileLarge.</param>
        protected static void SaveTileImage(Control tile, string filename, Control[] children, Image moviePosterImg = null)
        {
            var sb = new StringBuilder();
            foreach (var ctrl in children)
            {
                var rc = tile.ToParentRect(ctrl);
                sb.AppendFormat("{0},{1},{2},{3}|", rc.X, rc.Y, rc.Width, rc.Height);
            }
            sb.Length -= 1;
            var userComment = sb.ToString();

            Bitmap bmp;
            if (moviePosterImg != null)
            {
                //save both foreground (e.g. poster) and background images.
                bmp = new Bitmap(tile.Width * 2, tile.Height, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(bmp))
                    g.DrawImage(moviePosterImg, new Rectangle(0, 0, tile.Width, tile.Height));
                bmp = tile.ToImage(bmp, new Rectangle(tile.Width, 0, tile.Width, tile.Height), filename, userComment);
            }
            else
            {
                bmp = tile.ToImage(filename, userComment);
            }

            bmp.Dispose();
            tile.Dispose();
        }

        /// <summary>
        /// Show large tile that fits the entire, full screen. Only works with 'large' tiles.
        /// </summary>
        /// <param name="tile">Must be of type TileLarge or TileLargeLite</param>
        /// <param name="tilePosterBounds">Poster bounds within the tile.</param>
        public static void ShowFullscreen(TileLarge tile, Rectangle tilePosterBounds) { ShowFullscreenInternal(tile, tilePosterBounds); }

        /// <summary>
        /// Show large tile that fits the entire, full screen. Only works with 'large' tiles.
        /// </summary>
        /// <param name="tile">Must be of type TileLarge or TileLargeLite</param>
        /// <param name="tilePosterBounds">Poster bounds within the tile.</param>
        public static void ShowFullscreen(TileLargeLite tile, Rectangle tilePosterBounds) { ShowFullscreenInternal(tile, tilePosterBounds); }

        private static void ShowFullscreenInternal(ITile tile, Rectangle tilePosterBounds)
        {
            string moviePosterPath = tile.MovieProps.MoviePosterPath; //Full file path to original poster file.
            Control ctrl = (Control)tile;
            var screen = Screen.FromControl(ctrl).Bounds;
            var scaleFactor = screen.Width / (float)ctrl.Width;
            var szTileResized = new Size(screen.Width, (int)Math.Round(scaleFactor * ctrl.Height));
            var bmpTile = ctrl.ToImage();  //convert tile to image
            var bmpTileResized = bmpTile.Resize(szTileResized); //scale up to full-screen size so poster image can be replaced with hi-rez image.  
            bmpTile.Dispose();

            #region Replace blown up (fuzzy) poster in tile image with original hi-rez poster image.
            if (!moviePosterPath.IsNullOrEmpty() && File.Exists(moviePosterPath))
            {
                using (var graphics = Graphics.FromImage(bmpTileResized))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        var rcPosterWindow = new RectangleF(tilePosterBounds.X * scaleFactor, tilePosterBounds.Y * scaleFactor, tilePosterBounds.Width * scaleFactor, tilePosterBounds.Height * scaleFactor);
                        graphics.FillRectangle(Brushes.Black, Rectangle.Round(rcPosterWindow));
                        var bmpPoster = new Bitmap(moviePosterPath);

                        //Find new poster size and centered in black poster window frame.
                        float newWidth, newHeight;
                        newHeight = bmpPoster.Height * rcPosterWindow.Width / bmpPoster.Width;
                        if (newHeight <= rcPosterWindow.Height) newWidth = rcPosterWindow.Width;
                        else
                        {
                            newWidth = bmpPoster.Width * rcPosterWindow.Height / bmpPoster.Height;
                            newHeight = rcPosterWindow.Height;
                        }
                        float x = (rcPosterWindow.Width - newWidth) / 2.0f; //calculate where to place the image to center it:
                        float y = (rcPosterWindow.Height - newHeight) / 2.0f;
                        var rcPoster = new RectangleF(x + rcPosterWindow.X, y + rcPosterWindow.Y, newWidth, newHeight);

                        graphics.DrawImage(bmpPoster, Rectangle.Round(rcPoster), 0, 0, bmpPoster.Width, bmpPoster.Height, GraphicsUnit.Pixel, wrapMode);
                        bmpPoster.Dispose();
                    }
                }
            }
            #endregion

            //Initialize PictureBox control with fullscreen-sized tile image
            var pb = new PictureBox();
            pb.BackColor = Color.Black;
            pb.SizeMode = PictureBoxSizeMode.CenterImage; //no resizing needed because it is already resized above.
            pb.Margin = new Padding(0);
            pb.Cursor = Cursors.Hand;
            pb.Dock = DockStyle.Fill;
            pb.Image = bmpTileResized;

            var fullScreen = new Form();
            fullScreen.Owner = FormMain.This;
            fullScreen.FormBorderStyle = FormBorderStyle.None;
            fullScreen.WindowState = FormWindowState.Maximized;
            //fullScreen.TopMost = true; //locks up machine when key is pressed.
            fullScreen.ShowInTaskbar = false;
            fullScreen.StartPosition = FormStartPosition.Manual;
            fullScreen.Controls.Add(pb);

            Action pb_Click = () =>
            {
                fullScreen.Hide();
                fullScreen.Controls.RemoveAt(0);
                pb.Image.Dispose();
                pb.Dispose();
                fullScreen.Close();
            };
            pb.Click += (s, e) => pb_Click();
            pb.PreviewKeyDown += (s, e) => pb_Click();
            pb.MouseWheel += (s, e) => pb_Click();

            fullScreen.Show();
            pb.Focus(); //enable seeing keystrokes
        }

        /// <summary>
        /// Delete all cached tile images e.g. "tt*.png"
        /// </summary>
        /// <param name="folder">path containing cached images to to delete</param>
        public static void PurgeTileImages(string folder)
        {
            //Valid filenames: "tt[0-9]+(S|M|L).png" ex. tt0000000S.png, tt0000000M.png, tt0000000L.png
            foreach (var f in Directory.GetFiles(folder, "tt*.png"))
            {
                var sml = char.ToUpperInvariant(f[f.Length - 5]);
                try
                {
                    if ("SML".IndexOf(sml) == -1) continue; //faster than regex!
                    File.Delete(f);
                }
                catch (Exception ex)
                {
                    Log.Write(Severity.Error, $"Deleting {f}: {ex.Message}");
                }
            }
        }
    }
}
