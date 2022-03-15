//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="FormMain.cs" company="Chuck Hill">
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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace VideoLibrarian
{
    using VKEY = System.Collections.Generic.KeyValuePair<ViewType, string>;
    public enum ViewType { SmallTiles, MediumTiles, LargeTiles };

    public partial class FormMain : Form
    {
        private ToolTipHelp tt;

        public SettingProperties Settings;
        private ViewType View;
        private SortProperties SortKeys;
        private FilterProperties Filters = null;
        private int ScrollPosition;
        private int MaxLoadedProperties;

        private ViewTiles CurrentViewTiles = new ViewTiles();
        private List<MovieProperties> MovieProperties = new List<MovieProperties>();
        private GlobalMouseHook mouseHook;
        private bool needsCacheRebuild = false;
        private bool forcePropertiesUpdate = false;

        //Required so Tiles can load sub-tiles aka TV Series.
        private static FormMain _This = null;
        public static FormMain This { get { return _This; } }

        public FormMain()
        {
            _This = this;
            InitializeComponent();
            tt = new ToolTipHelp(this);
            EnableMenuBar(false);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            mouseHook = new GlobalMouseHook();
            mouseHook.MouseMove += mouseHook_MouseMove;
            mouseHook.MouseLeave += mouseHook_MouseLeave;

            FormMainProperties data = FormMainProperties.Deserialize();
            //if DesktopBounds NOT visible on screen, reset it to fullscreen.
            if (!Screen.GetWorkingArea(new Point(10, 10)).IntersectsWith(data.DesktopBounds)) data.DesktopBounds = Screen.GetWorkingArea(new Point(10, 10));

            //this.DesktopBounds = data.DesktopBounds;
            this.Tag = (Rectangle)data.DesktopBounds; //Set AFTER all tiles have been loaded. See LoadTiles()

            this.Settings = data.Settings;
            this.View =  data.View;
            this.SortKeys = new SortProperties(data.SortKey);
            this.Filters = data.Filters;
            this.ScrollPosition = data.ScrollPosition;
            this.MaxLoadedProperties = data.MaxLoadedProperties;
            Log.SeverityFilter = data.LogSeverity;

            SetViewMenu(this.View);

            needsCacheRebuild = (data.DPIScaling != GDI.DpiScalingFactor());
            if (needsCacheRebuild)
                Log.Write(Severity.Info, "Rebuilding image cache due to screen resolution change (aka DPI scaling).");

            if (data.Version != Assembly.GetExecutingAssembly().GetName().Version)
            {
                //Not right now as this has not been released to the public....

                //Log.Write(Severity.Info, $"Upgrading movie property info from {data.Version} to {Assembly.GetExecutingAssembly().GetName().Version}");
                //forcePropertiesUpdate = true;
            }
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            if (this.Settings.MediaFolders.Length == 0)
            {
                MiniMessageBox.Show(this, "Welcome! This is the first time running VideoLibrarian. Please read\nthe information in the About dialog. This will help you setup your\nmovie layout. When complete, go to File->Settings to enter the\nroot folder of all your movies.", "Initial VideoLibrarian Startup");
                return;
            }

            var prevCacheSize = Regex.CacheSize; //==15
            Regex.CacheSize = 32; //MovieProperties.ParseImdbPage() uses ~22 compiled regex patterns, so we boost the cache size to avoid expensive churn.

            LoadMovieInfo();

            Regex.CacheSize = prevCacheSize;

            if (MovieProperties.Count == 0)
            {
                MiniMessageBox.Show(this, "The media folders specified in File->Settings\nare invalid or no longer contain any movies.\nPlease review File->Status Log for any errors.", "Movie Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadTiles();
            EnableMenuBar(MovieProperties.Count > 0);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED to reduce flicker.
                //var parms = base.CreateParams;
                //parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return cp;
            }
        }

        private ITile prevTile;
        void mouseHook_MouseMove(object sender, GlobalMouseEventArgs e)
        {
            var tile = ((Control)sender).FindParent<ITile>();
            if (tile != null)
            {
                if (prevTile != null && tile != prevTile) prevTile.MouseEntered(true);
                if (tile != prevTile) tile.MouseEntered(false);
                prevTile = tile;
            }
            else
            {
                if (prevTile != null) prevTile.MouseEntered(true);
                prevTile = null;
            }
        }

        void mouseHook_MouseLeave(object sender, GlobalMouseEventArgs e)
        {
            if (prevTile != null) prevTile.MouseLeftApp();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            mouseHook.Dispose();
            try { SaveState(); }
            catch (Exception ex) { Log.Write(Severity.Error, $"SaveState: {ex}"); }
            PleaseWait.Show(this, "Saving Movie Info...", (state) =>
            {
                try
                {
                    foreach (var p in MovieProperties)
                    {
                        if (p.Episodes != null)
                        {
                            foreach (var p2 in p.Episodes)
                            {
                                p2.Serialize();
                                DeleteFileCache(p2);
                            }
                        }
                        p.Serialize();
                        DeleteFileCache(p);
                    }
                }
                catch (Exception ex) { Log.Write(Severity.Error, $"Serializing MovieProperties: {ex}"); }
            });

            base.OnFormClosing(e);
        }

        private static void DeleteFileCache(MovieProperties p)
        {
            if (p.DeleteFileCacheUponExit == VideoLibrarian.MovieProperties.FileCacheScope.All)
            {
                Log.Write(Severity.Verbose, $"Deleting all properties for {p.MoviePath}");
                FileDelete(p.MoviePosterPath);
                FileDelete(p.PropertiesPath);
                FileDelete(p.HtmlPath);
                FileDelete(p.PathPrefix + "S.png");
                FileDelete(p.PathPrefix + "M.png");
                FileDelete(p.PathPrefix + "L.png");
            }
            else if (p.DeleteFileCacheUponExit == VideoLibrarian.MovieProperties.FileCacheScope.ImagesOnly)
            {
                Log.Write(Severity.Verbose, $"Deleting only tile images for {p.MoviePath}");
                FileDelete(p.PathPrefix + "S.png");
                FileDelete(p.PathPrefix + "M.png");
                FileDelete(p.PathPrefix + "L.png");
            }
        }

        private static bool FileDelete(string fn)
        {
            try
            {
                File.Delete(fn);
                return true;
            }
            catch(Exception ex)
            {
                Log.Write(Severity.Warning, $"Could not delete {fn}: {ex.Message}");
                return false;
            }
        }

        private void SaveState()
        {
            //Nothing to save if there are no movie property objects for whatever reason. 
            //e.g. Media folder no longer exists or is empty or USB drive is unplugged. 
            //See LoadMovieInfo().
            if (MovieProperties.Count == 0) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(SaveState));
                return;
            }

            var data = new FormMainProperties();
            if (this.WindowState != FormWindowState.Minimized)
            {
                data.DesktopBounds = this.DesktopBounds;
            }

            data.View = this.View;
            data.Settings = this.Settings;
            data.SortKey = this.SortKeys.ToString();
            if (this.Filters != null && !this.Filters.FilteringDisabled) data.Filters = this.Filters;

            if (m_miBack.Enabled) //if TVSeries, get scroll position of parent view.
            {
                var key = ViewTiles.CreateKey(this.View);
                ViewTiles view;
                if (Views.TryGetValue(key, out view))
                    data.ScrollPosition = view.ScrollPosition;
                else
                    data.ScrollPosition = 0;
            }
            else
            {
                data.ScrollPosition = m_flowPanel.VerticalScroll.Value;
            }

            data.MaxLoadedProperties = this.MaxLoadedProperties;
            data.LogSeverity = Log.SeverityFilter;
            data.Serialize();
        }

        private readonly Dictionary<VKEY, ViewTiles> Views = new Dictionary<VKEY, ViewTiles>(); //Cache
        private ViewTiles GetView(string title = null, List<MovieProperties> mp = null)
        {
            ViewTiles view;

            if (mp == null || mp == MovieProperties)
            {
                mp = MovieProperties;
                title = string.Empty;
            }

            var key = ViewTiles.CreateKey(this.View, title);

            if (Views.TryGetValue(key, out view)) 
            {
                if (mp == MovieProperties)
                {
                    if (Filters != null && Filters.Filter(view.Tiles)) view.ScrollPosition = -1; //flag caller to compute position based upon 'current' tile.
                    if (SortKeys != null && SortKeys.Sort(view.Tiles)) view.ScrollPosition = -1; 
                }
                return view;
            }

            //---------
            //Tiles are not in the cache. We have to build the tile list from the movie properties.
            //---------

            view = new ViewTiles();
            var maxTiles = 0;
            Func<int,int,int> min = (a, b) => a > b ? b : a;
            switch(this.View)
            {
             #if (!IN_DESIGNER) //Use lightweight tiles using minimal window handles.  
                //Use of too many tiles will run out of window handles (aka USER Objects).
                case ViewType.SmallTiles:
                    maxTiles = 4750;  //2 window handles/tile = (10000 max handles - 500 padding) / (windowHandles/tile)
                    PurgeCache(2 * min(mp.Count,maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileSmallLite.Create(m)).ToArray(); //uses 4 window handles 
                    break;
                case ViewType.MediumTiles:
                    maxTiles = 3167;  //3 window handles/tile
                    PurgeCache(3 * min(mp.Count, maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileMediumLite.Create(m)).ToArray();
                    break;
                case ViewType.LargeTiles:
                    maxTiles = 3167;   //3 window handles/tile
                    PurgeCache(3 * min(mp.Count, maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileLargeLite.Create(m)).ToArray();
                    break;
             #else //Using template tiles for verifying layout.
                case ViewType.SmallTiles:
                    maxTiles = 863;
                    PurgeCache(11 * min(mp.Count,maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileSmall.Create(m)).ToArray(); //uses 11 window handles
                    break;
                case ViewType.MediumTiles:
                    maxTiles = 558;
                    PurgeCache(17 * min(mp.Count,maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileMedium.Create(m)).ToArray(); //uses 17 window handles
                    break;
                case ViewType.LargeTiles:
                    maxTiles = 296;
                    PurgeCache(32 * min(mp.Count,maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileLarge.Create(m)).ToArray();  //uses 32 window handles
                    break;
             #endif
                default: throw new InvalidEnumArgumentException("Unknown ViewType Enum"); //should never occur
            }

            if (mp.Count > maxTiles)
                Log.Write(Severity.Warning, $"Number of movies found ({mp.Count}) exceeds the {maxTiles} memory limit for {this.View}. Truncating list.");

            Views.Add(key, view);
            if (mp == MovieProperties) 
            {
                if (Filters != null) Filters.Filter(view.Tiles);
                if (SortKeys != null) SortKeys.Sort(view.Tiles);
            }
            var tile = view.Tiles.FirstOrDefault(v => v.IsVisible);
            if (tile == null) tile = view.Tiles[0];
            view.CurrentTile = tile;
            view.ScrollPosition = this.ScrollPosition; //initial scroll position at program startup
            this.ScrollPosition = 0;
            
            return view;
        }

        public void LoadTiles(string title = null, List<MovieProperties> mp = null)
        {
            CurrentViewTiles.CurrentTile = (ITile)m_flowPanel.CurrentVisibleControl;
            CurrentViewTiles.ScrollPosition = m_flowPanel.VerticalScroll.Value;
            PleaseWait.Show(this, "Creating/sorting/filtering tiles.  Be patient. This may take awhile.", (state) =>
            {
                CurrentViewTiles = GetView(title, mp);
            });

            if (this.Tag != null) //Set in FormMain_Load()
            {
                //Now restore the last instance's size and position.
                var screen = Screen.FromControl(this).WorkingArea;
                var bounds = (Rectangle)this.Tag;
                this.Tag = null;
                if (bounds.Width >= screen.Width && bounds.Height >= screen.Height)
                {
                    this.DesktopBounds = screen;
                    this.WindowState = FormWindowState.Maximized;
                }
                else this.DesktopBounds = bounds;
                Application.DoEvents(); //let resized window background paint itself to avoid flickering because the following will still take a second. 
            }

            m_flowPanel.SuspendLayout();
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    m_flowPanel.Controls.Clear();
                    m_flowPanel.Controls.AddRange((Control[])CurrentViewTiles.Tiles);
                }
                catch (System.ComponentModel.Win32Exception) //Error creating window handle. ex.ErrorCode == 0x80004005
                {
                    //Error at System.Windows.Forms.NativeWindow.CreateHandle(CreateParams cp)
                    if (i == 1) throw;
                    PurgeAll(title);
                    continue;
                }
                break;
            }

            //GetView() flagged that filter/sorting has modified tiles[]. ScrollPosition needs 
            //to be updated based the latest location of the 'current' tile. However, tile 
            //location is not updated until AFTER the tiles have been added to FlowLayoutPanel.
            if (CurrentViewTiles.ScrollPosition == -1 && CurrentViewTiles.CurrentTile != null)
            {
                Control c = (Control)CurrentViewTiles.CurrentTile;
                var ei = ScrollPanel.GetExtraInfo(c);
                if (ei!=null) CurrentViewTiles.ScrollPosition = ei.Location.Y - ((m_flowPanel.ClientSize.Height/2 - (c.Height+c.Margin.Vertical)/2));
            }

            if (CurrentViewTiles.ScrollPosition < 0) CurrentViewTiles.ScrollPosition = 0;

            m_flowPanel.VerticalScroll.Value = CurrentViewTiles.ScrollPosition;
            m_flowPanel.ResumeLayout();

            EnableMenuBar(mp == null || mp == MovieProperties);
            m_miBack.Enabled = (mp != null && mp != MovieProperties);

            if (CurrentViewTiles.Tiles.Length > 0 && !CurrentViewTiles.Tiles.Any(m => m.IsVisible))
            {
                MiniMessageBox.Show(this, "Oops! The filter is too restrictive.\nPlease try again.", "Movie Filtering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //Bug in Regex.Escape(@"~`'!@#$%^&*(){}[].,;+_=-"). It doesn't escape ']'
        //const string BracketPattern = @"\\[~`'!@\#\$%\^&\*\(\{\[\.,;\+_=-][^\\]+[~`'!@\#\$%\^&\*\)\}\]\.,;\+_=-]\\";  //fast and loose bracket pattern.
        //Create strict bracket pattern; whatever bracket char folder name starts with, it must also end with.
        const string BracketPattern = @"\\(
            (~[^\\]+~)|
            (`[^\\]+`)|
            ('[^\\]+')|
            (![^\\]+!)|
            (@[^\\]+@)|
            (\#[^\\]+\#)|
            (\$[^\\]+\$)|
            (%[^\\]+%)|
            (\^[^\\]+\^)|
            (&[^\\]+&)|
            (\*[^\\]+\*)|
            (\.[^\\]+\.)|
            (,[^\\]+,)|
            (;[^\\]+;)|
            (\+[^\\]+\+)|
            (_[^\\]+_)|
            (=[^\\]+=)|
            (-[^\\]+-)|
            (\([^\\]+\))|
            (\{[^\\]+\})|
            (\[[^\\]+\])
            )\\";
        private static readonly Regex reIgnoredFolder = new Regex(BracketPattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        public void LoadMovieInfo()
        {
            //This may take awhile. Don't lock up the UI.
            PleaseWait.Show(this, "Finding, extracting, loading movie info...  Be patient. This may take awhile.", (state) =>
            {
                foreach (var mf in Settings.MediaFolders)
                {
                    if (!Directory.Exists(mf))
                    {
                        Log.Write(Severity.Error, $"Media folder {mf} does not exist.");
                        continue;
                    }

                    var hs = new HashSet<string>(StringComparer.OrdinalIgnoreCase); //There may be multiple shortcuts in a folder, but we may only list the folder once. 
                    string fx = "beginning of search";
                    try
                    {
                        foreach (string f in Directory.EnumerateFiles(mf, "*.url", SearchOption.AllDirectories))
                        {
                            fx = f;
                            var folder = Path.GetDirectoryName(f);
                            if (folder == mf) continue; //ignore shortcuts in the root folder

                            //Special: if shortcut is in a bracketed folder (or any of its child folders) the video is ignored. 
                            if (reIgnoredFolder.IsMatch(folder + "\\")) continue;

                            hs.Add(folder);
                        }
                    }
                    catch(Exception ex) //System.IO.IOException: The file or directory is corrupted and unreadable.
                    {
                        var emsg = $"{ex.GetType().FullName}: {ex.Message}\nFatal Error enumerating movie folder immediately following {fx}.";
                        MessageBox.Show(this, emsg+"\n\nPress OK to exit.", "Enumerating Media Folders", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log.Write(Severity.Error, emsg);
                        Log.Dispose();
                        Environment.Exit(1);
                    }

                    int added = 0;
                    foreach (string d in hs.OrderBy(x => x))
                    {
                        try
                        {
                            var p = new MovieProperties(d, forcePropertiesUpdate);
                            if (p.ToString() == "UNKNOWN") //Incomplete/corrupted movie property. See log file. Ignore for now.
                                throw new InvalidDataException($"Incomplete/corrupted movie property for folder: {d}");

                            MovieProperties.Add(p);
                            added++;

                            if (needsCacheRebuild) TileBase.PurgeTileImages(d); //DPI changed
                        }
                        catch (Exception ex)
                        {
                            Log.Write(Severity.Error, $"Movie property failed to load from {d}: {ex.Message}");
                        }
                    }

                    Log.Write(Severity.Info, $"{added} movie properties loaded from {mf}");
                }
                if (MovieProperties.Count == 0) return;

                needsCacheRebuild = false;
                forcePropertiesUpdate = false;

                //Group series under parent.
                MovieProperties.Sort(Comparer<MovieProperties>.Create((a, b) => string.CompareOrdinal(a.SortKey, b.SortKey)));

                int kount = MovieProperties.Count;
                MovieProperties series = null;
                bool seriesFound = false; //handle parent followed by non-series movie followed by the series
                for (int i = 0; i < kount; i++)
                {
                    var p = MovieProperties[i];
                    if (p.EpisodeCount > 0)  //MovieClass="TV Series"
                    {
                        series = p;
                        series.Episodes = new List<MovieProperties>();
                        seriesFound = false;
                        continue;
                    }
                    if (series != null && p.Season != 0) //MovieClass="TV Episode"
                    {
                        seriesFound = true;
                        series.Episodes.Add(p);
                        MovieProperties.RemoveAt(i);
                        i--;
                        kount--;
                        continue;
                    }
                    if (seriesFound) { seriesFound = false; series = null; } //force to null so series without a parent are handled as normal movies
                }

                if (this.MaxLoadedProperties > 0 && MovieProperties.Count >= this.MaxLoadedProperties)
                {
                    Log.Write(Severity.Warning, $"Number of movies found ({MovieProperties.Count}) exceeds the {this.MaxLoadedProperties} user limit MovieProperties. Truncating list.");
                    MovieProperties.RemoveRange(this.MaxLoadedProperties, MovieProperties.Count - this.MaxLoadedProperties);
                }

                MovieProperties.TrimExcess();
                FilterProperties.InitAvailableValues(MovieProperties);
            });
        }

        #region ToolStripMenuItems
        private void m_miBack_Click(object sender, EventArgs e)
        {
            LoadTiles();
        }

        private void m_miSettings_Click(object sender, EventArgs e)
        {
            var result = SettingsDialog.Show(this, Settings);
            if (result == null) return;
            bool foldersChanged = (result.MediaFolders.Length != Settings.MediaFolders.Length || !result.MediaFolders.SequenceEqual(Settings.MediaFolders));
            Settings = result;

            EnableMenuBar(MovieProperties.Count > 0);
            if (foldersChanged) { LoadMovieInfo(); LoadTiles(); EnableMenuBar(MovieProperties.Count > 0); }
        }

        private void m_miStatusLog_Click(object sender, EventArgs e)
        {
            ProcessEx.OpenExec(this.Settings.LogViewer, Log.LogName);
        }

        private void m_miExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void m_miView_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem[] menuitems = new ToolStripMenuItem[] { m_miViewSmall, m_miViewMedium, m_miViewLarge };
            var mi = sender as ToolStripMenuItem;

            for(int i=0; i<menuitems.Length; i++)
            {
                if (menuitems[i] == mi) this.View = (ViewType)i;
            }

            SetViewMenu(this.View);

            LoadTiles();
        }

        private void SetViewMenu(ViewType v)
        {
            //The MenuItem check mark sucks so we use our own. 
            ToolStripMenuItem[] menuitems = new ToolStripMenuItem[] { m_miViewSmall, m_miViewMedium, m_miViewLarge };
            for(int i=0; i<menuitems.Length; i++)
            {
                if (i==(int)v)
                {
                    //menuitems[i].Checked = true;
                    menuitems[i].Image = global::VideoLibrarian.Properties.Resources.CheckMark16;
                }
                else
                {
                    //menuitems[i].Checked = false;
                    menuitems[i].Image = null;
                }
            }
        }

        private void m_miSort_Click(object sender, EventArgs e)
        {
            SortProperties result = SortDialog.Show(this, SortKeys);
            if (result == null) return;
            SortKeys = result;

            LoadTiles();
        }

        private void m_miFilter_Click(object sender, EventArgs e)
        {
            var filter = FilterDialog.Show(this, Filters);
            if (filter == null) return;
            Filters = filter;

            LoadTiles();
        }

        private void m_miAbout_Click(object sender, EventArgs e)
        {
            AboutBox.Show(this);
        }

        private void EnableMenuBar(bool enable)
        {
            //These menu items are not allowed when there are no tiles.
            //These menu items are not allowed when TV Series tiles are shown (e.g. back arrow is enabled).
            m_miView.Enabled = enable;
            m_miSort.Enabled = enable;
            m_miFilter.Enabled = enable;
        }

        #endregion

        #region private void PurgeCache(int neededUserObjects)
        [DllImport("User32.dll")]
        private static extern int GetGuiResources(IntPtr hProcess, int uiFlags);
        /// <summary>
        /// Purge cache if the new MovieProperty tiles may cause out-of-resources error.
        /// If there are too many movies, an out-of-window-handles (aka USER Objects) error 
        /// may occur and lock up the application where the only way out is to terminate via
        /// the Task Manager. A Win32 process may have a maximum of 10000 open USER Objects. 
        /// The following code helps mitigate this problem at the expense of performance.
        /// Rules:
        ///    **Small Tile Lite consists of 4 window handles. 10000/4 = 2500 movies
        ///    **Medium Tile Lite consists of 5 window handles. 10000/5 = 2000 movies
        ///    **Large Tile Lite consists of 5 window handles. 10000/5 = 2000 movies
        /// </summary>
        /// <param name="neededUserObjects"></param>
        /// <param name="newView">new cache key (e.g. view name)</param>
        private void PurgeCache(int neededUserObjects, VKEY newView)
        {
            Func<int> availableUserObjects = () => 10000 - GetGuiResources(Process.GetCurrentProcess().Handle, 1); //uiFlags == 1 == GR_USEROBJECTS. Return the count of USER objects.
            Func<VKEY, string> toString = (kv) =>
            {
                if (string.IsNullOrEmpty(kv.Value)) return kv.Key.ToString();
                return string.Concat(kv.Key.ToString(), "-Series:", kv.Value);
            };

            if (Views.Count == 0) return;
            if (availableUserObjects() > neededUserObjects) return;
            var deletedList = new List<VKEY>(Views.Count);

            //Sort cache values to delete by smallest cache values first.
            var order = new List<ViewType>(3) { ViewType.SmallTiles, ViewType.MediumTiles, ViewType.LargeTiles };
            order.Remove(newView.Key);
            order.Add(newView.Key); //put current view last.

            var views = Views.ToArray();
            Comparison<KeyValuePair<VKEY, ViewTiles>> comparer = (x, y) =>
            {
                int v = string.CompareOrdinal(y.Key.Value, x.Key.Value);
                if (v != 0) return v;

                v = order.IndexOf(x.Key.Key) - order.IndexOf(y.Key.Key);
                if (v != 0) return v;

                return x.Value.Tiles.Length - y.Value.Tiles.Length;
            };
            Array.Sort(views, Comparer<KeyValuePair<VKEY, ViewTiles>>.Create(comparer));

            foreach(var kv in views)
            {
                Log.Write(Severity.Warning, $"Insufficient resources (USER Objects) for new view {toString(newView)}. Deleting cached view {toString(kv.Key)}.");
                var gen = kv.Value.Tiles!=null && kv.Value.Tiles.Length > 0 ? GC.GetGeneration(kv.Value.Tiles[0]) : 0;
                DisposeTiles(kv.Value.Tiles);
                kv.Value.Tiles = null;
                kv.Value.CurrentTile = null;
                deletedList.Add(kv.Key);
                GC.Collect(gen,GCCollectionMode.Default,true);  //be sure tiles list is closed and memory deallocated.
                if (availableUserObjects() > neededUserObjects) break;
            }
            foreach(var kv in deletedList) { Views.Remove(kv); }
        }

        /// <summary>
        /// Delete all cached tiles except self.
        /// </summary>
        /// <param name="seriesTitle">Series title or "" if all movies</param>
        private void PurgeAll(string seriesTitle)
        {
            var key = ViewTiles.CreateKey(this.View, seriesTitle); 
            var deletedList = new List<VKEY>(Views.Count);
            foreach(var kv in Views)
            {
                if (kv.Key.Equals(key)) continue; //do not delete self
                var gen = kv.Value.Tiles != null && kv.Value.Tiles.Length > 0 ? GC.GetGeneration(kv.Value.Tiles[0]) : 0;
                DisposeTiles(kv.Value.Tiles);
                kv.Value.Tiles = null;
                kv.Value.CurrentTile = null;
                deletedList.Add(kv.Key);
                GC.Collect(gen, GCCollectionMode.Default, false);  //be sure tiles list is closed and memory deallocated.
            }
            foreach (var kv in deletedList) { Views.Remove(kv); }
        }

        private void DisposeTiles(ITile[] tiles)
        {
            if (tiles.Length == 0) return;
            var c = (Control)tiles[0];
            if (c.InvokeRequired)
            {
                c.BeginInvoke((Action<ITile[]>)DisposeTiles, new object[] { tiles });
                return;
            }

            foreach (var t in tiles) ((Control)t).Dispose();
        }
        #endregion

        #region private class ViewTiles
        /// <summary>
        /// Cached item
        /// </summary>
        private class ViewTiles
        {
            public ITile[] Tiles;
            public ITile CurrentTile;
            public int ScrollPosition;

            public static VKEY CreateKey(ViewType view, string title = null)
            {
                return new KeyValuePair<ViewType, string>(view, title ?? string.Empty);
            }
        }
        #endregion
    }
}
