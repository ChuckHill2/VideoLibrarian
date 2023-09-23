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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public enum ViewType { SmallTiles, MediumTiles, LargeTiles };

    public partial class FormMain : Form
    {
        private ToolTipHelp tt;

        public SettingProperties Settings; //updated by SettingsDialog()
        private ViewType View;  //current view type (e.g. small, medium, or large)
        private SortProperties SortKeys;
        private FilterProperties Filters = null;
        private KeyValueList<string,int> ScrollPositions;
        private int MaxLoadedProperties;

        private readonly Dictionary<string, ViewTiles> Views = new Dictionary<string, ViewTiles>(); //Cache
        private ViewTiles CurrentViewTiles = new ViewTiles();
        private List<MovieProperties> MoviePropertiesList = new List<MovieProperties>();
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
            if (!Screen.GetWorkingArea(new Point(10, 10)).IntersectsWith(data.DesktopBounds))
                data.DesktopBounds = Screen.GetWorkingArea(new Point(10, 10));

            this.Tag = (Rectangle)data.DesktopBounds; //Set AFTER all tiles have been loaded. See LoadTiles()

            this.Settings = data.Settings;
            this.View =  data.View;
            this.SortKeys = new SortProperties(data.SortKey);
            this.Filters = data.Filters;
            this.ScrollPositions = data.ScrollPositions;
            this.MaxLoadedProperties = data.MaxLoadedProperties;
            Log.SeverityFilter = Settings.LogSeverity;

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
                MiniMessageBox.ShowDialog(this, "Welcome! This is the first time running VideoLibrarian. " +
                    "Please read the information in the About dialog. This will help you setup your movie layout. " +
                    "When complete, go to File->Settings to enter the root folder of all your movies.",
                    "Initial VideoLibrarian Startup",MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            LoadMovieInfo();

            if (MoviePropertiesList.Count == 0)
            {
                MiniMessageBox.ShowDialog(this, "The media folders specified in File->Settings are invalid " +
                    "or no longer contain any movies. Please review File->Status Log for any errors.",
                    "Movie Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadTiles();
            EnableMenuBar(MoviePropertiesList.Count > 0);
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
            SaveState();
            SaveAllMovieProperties();
            base.OnFormClosing(e);
        }

        private void SaveAllMovieProperties()
        {
            try
            {
                foreach (var p in MoviePropertiesList)
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
        }

        private static void DeleteFileCache(MovieProperties p)
        {
            if (p.DeleteFileCacheUponExit == VideoLibrarian.MovieProperties.FileCacheScope.All)
            {
                Log.Write(Severity.Verbose, $"Deleting all properties for {p.MoviePath}");
                //FileEx.FileDelete(p.MoviePosterPath);
                FileEx.Delete(p.PropertiesPath);
                FileEx.Delete(p.HtmlPath);
                FileEx.Delete(p.PathPrefix + "S.png");
                FileEx.Delete(p.PathPrefix + "M.png");
                FileEx.Delete(p.PathPrefix + "L.png");
            }
            else if (p.DeleteFileCacheUponExit == VideoLibrarian.MovieProperties.FileCacheScope.ImagesOnly)
            {
                Log.Write(Severity.Verbose, $"Deleting only tile images for {p.MoviePath}");
                FileEx.Delete(p.PathPrefix + "S.png");
                FileEx.Delete(p.PathPrefix + "M.png");
                FileEx.Delete(p.PathPrefix + "L.png");
            }
        }

        private void SaveState()
        {
            try
            {
                //Nothing to save if there are no movie property objects for whatever reason. 
                //e.g. Media folder no longer exists or is empty or USB drive is unplugged. 
                //See LoadMovieInfo().
                if (MoviePropertiesList.Count == 0) return;

                //In case this function is running in a thread.
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

                //Update current view final position.
                if (this.CurrentViewTiles != null)
                    this.ScrollPositions[this.CurrentViewTiles.Key] = m_flowPanel.VerticalScroll.Value;

                //Unclutter scroll positions serialized list by removing scroll positions that equal zero as this is the default anyway.
                for (int i=ScrollPositions.Count - 1; i >= 0; i--)
                {
                    if (ScrollPositions[i].Value > 0) continue;
                    ScrollPositions.RemoveAt(i);
                }

                data.ScrollPositions = this.ScrollPositions;
                data.MaxLoadedProperties = this.MaxLoadedProperties;
                data.Serialize();
            }
            catch (Exception ex)
            {
                Log.Write(Severity.Error, $"SaveState: {ex}");
            }
        }

        private ViewTiles GetView(string titleId = null, List<MovieProperties> mp = null)
        {
            ViewTiles view;

            if (mp == null || mp == MoviePropertiesList)
            {
                mp = MoviePropertiesList;
                titleId = string.Empty;
            }

            if (this.CurrentViewTiles!=null && this.CurrentViewTiles.Key != null)
                this.ScrollPositions[this.CurrentViewTiles.Key] = m_flowPanel.VerticalScroll.Value;

            var key = ViewTiles.CreateKey(this.View, titleId);

            if (Views.TryGetValue(key, out view))
            {
                if (mp == MoviePropertiesList)
                {
                    if (Filters != null && Filters.Filter(view.Tiles)) view.ScrollPosition = -1; //flag caller to compute position based upon 'current' tile.
                    if (SortKeys != null && SortKeys.Sort(view.Tiles)) view.ScrollPosition = -1; 
                }
                view.LastUsed = Environment.TickCount;
                return view;
            }

            //---------
            //Tiles are not in the cache. We have to build the tile list from the movie properties.
            //---------

            view = new ViewTiles();
            view.Key = key;
            view.LastUsed = Environment.TickCount;

            var maxTiles = 0;
            Func<int,int,int> min = (a, b) => a > b ? b : a;
            switch(this.View)
            {
             #if (!IN_DESIGNER) //Use lightweight tiles using minimal window handles.  
                //Use of too many tiles will run out of window handles (aka USER Objects).
                //Note: max tiles empirically determined by using taskManageer->DetailedView, User Objects column for this process.
                case ViewType.SmallTiles:
                    maxTiles = 3316;  //3 window handles/tile == (10000 max handles - 50 padding) / (windowHandles/tile)
                    PurgeCache(3 * min(mp.Count,maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileSmallLite.Create(m)).ToArray(); //uses 4 window handles 
                    break;
                case ViewType.MediumTiles:
                    maxTiles = 3316;  //3 window handles/tile
                    PurgeCache(3 * min(mp.Count, maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileMediumLite.Create(m)).ToArray();
                    break;
                case ViewType.LargeTiles:
                    maxTiles = 2487;   //4 window handles/tile
                    PurgeCache(4 * min(mp.Count, maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileLargeLite.Create(m)).ToArray(); //Can't use .AsParallel().AsOrdered() because WinForms is strictly single-threaded!
                    break;
             #else //Using template tiles for verifying layout. and testing UserObject handling.
                case ViewType.SmallTiles:
                    maxTiles = 901;
                    PurgeCache(11 * min(mp.Count,maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileSmall.Create(m)).ToArray(); //uses 11 window handles/tile
                    break;
                case ViewType.MediumTiles:
                    maxTiles = 583; 
                    PurgeCache(17 * min(mp.Count,maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileMedium.Create(m)).ToArray(); //uses 17 window handles/tile
                    break;
                case ViewType.LargeTiles:
                    maxTiles = 291;  //81 UserObjects Base window + sort dialog (busiest dialog); 34 UserObjects/Tile;  actual value=9959
                    PurgeCache(34 * min(mp.Count,maxTiles), key);
                    view.Tiles = mp.Take(maxTiles).Select(m => TileLarge.Create(m)).ToArray();  //uses 34 window handles/tile
                    break;
             #endif
                default: throw new InvalidEnumArgumentException("Unknown ViewType Enum"); //should never occur
            }

            if (mp.Count > maxTiles)
                Log.Write(Severity.Warning, $"Number of movies found ({mp.Count}) exceeds the {maxTiles} memory limit for {this.View}. Truncating list.");

            Views.Add(key, view);
            if (mp == MoviePropertiesList) 
            {
                if (Filters != null) Filters.Filter(view.Tiles);
                if (SortKeys != null) SortKeys.Sort(view.Tiles);
            }
            var tile = view.Tiles.FirstOrDefault(v => v.IsVisible);
            if (tile == null && view != null && view.Tiles.Length > 0) tile = view.Tiles[0];
            view.CurrentTile = tile;
            view.ScrollPosition = this.ScrollPositions[key];
            
            return view;
        }

        public void LoadTiles(string titleId = null, List<MovieProperties> mp = null)
        {
            CurrentViewTiles.CurrentTile = (ITile)m_flowPanel.CurrentVisibleControl;
            CurrentViewTiles.ScrollPosition = m_flowPanel.VerticalScroll.Value;
            PleaseWait.Show(this, "Creating, sorting, filtering tiles.  Be patient. This may take awhile.", (state) =>
            {
                CurrentViewTiles = GetView(titleId, mp);
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
                catch (System.ComponentModel.Win32Exception ex) //Error creating window handle. ex.ErrorCode == 0x80004005
                {
                    //Error at System.Windows.Forms.NativeWindow.CreateHandle(CreateParams cp)
                    if (i == 1) throw;
                    Log.Write(Severity.Error, "Error loading tiles. Purge view cache and retry: {1}", ex);
                    PurgeAllCache(ViewTiles.CreateKey(this.View, titleId));
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

            EnableMenuBar(mp == null || mp == MoviePropertiesList);
            m_miBack.Enabled = (mp != null && mp != MoviePropertiesList);

            if (CurrentViewTiles.Tiles.Length > 0 && !CurrentViewTiles.Tiles.Any(m => m.IsVisible))
            {
                MiniMessageBox.ShowDialog(this, "Oops! The filter is too restrictive. Please try again.", "Movie Filtering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void LoadMovieInfo()
        {
            Func<string, int> episodeIndexOf = ss =>
            {
                int ii = ss.IndexOf('\xAD') - 1;
                return ii < 0 ? ss.Length : ii;
            };

            //This may take awhile. Don't lock up the UI.
            PleaseWait.Show(this, "Finding, extracting, loading movie info. Be patient. This may take awhile.", (state) =>
            {
                var dupeCheck = new Dictionary<string, MovieProperties>(); //check for duplicate movies and warn but do not remove from list.

                foreach (var mf in Settings.MediaFolders)
                {
                    if (!Directory.Exists(mf))
                    {
                        Log.Write(Severity.Warning, $"Media folder {mf} does not exist.");
                        continue;
                    }

                    string fx = "beginning of search";
                    HashSet<string> hs;
                    try
                    {
                        // This is evaluated independent of Parallel.ForEach() in order to quickly identify integrity of folder tree and drive itself (Maybe corrupted?)
                        hs = DirectoryEx.EnumerateAllFiles(mf, SearchOption.AllDirectories)
                                .Where(m => m.EndsWith(".url", StringComparison.OrdinalIgnoreCase))
                                .Select(m => Path.GetDirectoryName(m))
                                .Select(m => { fx = m; return m; })   //In case of exception, we know where it broke.
                                .Where(m => m != mf & !MovieProperties.IgnoreFolder(m))  //Ignore shortcuts in the root folder AND if shortcut is in a bracketed folder (or any of its child folders) the folder is ignored.
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);  //There may be multiple shortcuts in a folder, but we may only list the folder once.

                        // May need to restrict max threads if there are too many downloads needed (aka new
                        // MovieProperty xml files) because flooding the IMDB webserver will drop downloads.
                        // But we want to load full speed (max threads) when loading local properties only.
                        // This example will do this.
                        //
                        // var re = RegexCache.RegEx(@"\\tt[0-9]+\.xml$", RegexOptions.IgnoreCase);
                        // Dictionary<bool,HashSet<string>> dict = DirectoryEx.EnumerateAllFiles(mf, SearchOption.AllDirectories)
                        //        .Where(m => m.EndsWith(".url", StringComparison.OrdinalIgnoreCase))
                        //        .Select(m => Path.GetDirectoryName(m))
                        //        .Select(m => { fx = m; return m; })   //In case of exception, we know where it broke.
                        //        .Where(m => m != mf & !MovieProperties.IgnoreFolder(m))  //Ignore shortcuts in the root folder AND if shortcut is in a bracketed folder (or any of its child folders) the folder is ignored.
                        //        .GroupBy(m => DirectoryEx.EnumerateAllFiles(m).Where(n => re.IsMatch(n)).FirstOrDefault() == null)
                        //        .ToDictionary(m => m.Key, n => n.ToHashSet(StringComparer.OrdinalIgnoreCase));
                        // key==true==missing tt*.xml
                        // key==false==has tt*.xml
                        // elapsed 375 ms, 60% slower
                    }
                    catch (Exception ex) //System.IO.IOException: The file or directory is corrupted and unreadable.
                    {
                        Log.Write(Severity.Error, $"{ex.GetType().FullName}: {ex.Message}\nFatal Error enumerating movie folder immediately following {fx}.");
                        throw new System.IO.IOException($"{ex.Message} Fatal Error enumerating movie folder immediately following {fx}.", ex);
                    }

                    if (hs == null || hs.Count == 0)
                    {
                        Log.Write(Severity.Warning, $"Media folder {mf} has no movies.");
                        continue;
                    }

                    var lockObj = new Object();
                    int added = 0;

                    // Need to restrict max threads if there are too many downloads needed (aka new
                    // MovieProperty xml files) because flooding the IMDB webserver will drop downloads.
                    // Testing showed that the web server started failing after 500 concurrent downloads.
                    var options = new ParallelOptions();
                    // if (hs.AsParallel().Sum(m => Directory.EnumerateFiles(m, "tt*.xml").Count() == 0 ? 1 : 0) > 400)
                    //     options.MaxDegreeOfParallelism = 100; //default == -1 infinite.
                    // The question is do I really need to support this for normal usage?

                    Parallel.ForEach(hs.OrderBy(x => x), options, folder =>
                    {
                        try
                        {
                            var p = new MovieProperties(folder, forcePropertiesUpdate); //may throw exceptions.

                            lock (lockObj)
                            {
                                MoviePropertiesList.Add(p);
                                added++;

                                if (dupeCheck.TryGetValue(p.TitleId,out var dupeProp))
                                    Log.Write(Severity.Warning, $"Duplicate titleID for \"{p.FullMovieName}\" found in both \"{Path.GetDirectoryName(dupeProp.PropertiesPath)}\" and \"{Path.GetDirectoryName(p.PropertiesPath)}\"");
                                else dupeCheck.Add(p.TitleId, p);
                            }

                            if (needsCacheRebuild) TileBase.PurgeTileImages(folder); //DPI changed
                        }
                        catch (Exception ex)
                        {
                            Log.Write(Severity.Error, $"Video property failed to load from {folder}: {ex.Message}");
                        }
                    });

                    Log.Write(Severity.Info, $"{added} video properties loaded from {mf}");
                }
                if (MoviePropertiesList.Count == 0) return;
                dupeCheck.Clear();

                needsCacheRebuild = false;
                forcePropertiesUpdate = false;

                //Sort and group tvEpisodes under parent tvSeries
                MoviePropertiesList.Sort(Comparer<MovieProperties>.Create((a, b) => string.CompareOrdinal(a.SortKey, b.SortKey)));

                //Now move matching tvEpisodes in each tvSeries episodes list
                var totalMovieProperties = MoviePropertiesList.Count;
                var totalSeries = 0;
                var totalEpisodes = 0;
                int kount = MoviePropertiesList.Count;
                MovieProperties series = null; //when this has a value, this is the current tvSeries
                for (int i = 0; i < kount; i++)
                {
                    var p = MoviePropertiesList[i];
                    if (p.EpisodeCount > 0)  //this is a tvSeries
                    {
                        series = p;
                        series.Episodes = new List<MovieProperties>();
                        totalSeries++;
                        continue;
                    }

                    if (series != null && p.Season != 0) //tvEpisode that is preceded by tvSeries
                    {
                        //tvSeries moviename == "Series Name", (tvEpisode moviename=="Series Name - Episode name" OR tvEpisode moviename=="Series Name")
                        if (series.MovieName.Length != episodeIndexOf(p.MovieName)) //edge case: tvSeries+episodes followed by another orphaned tvEpisode.
                        {
                            if (series.Episodes.Count == 0) series.Episodes = null; //if tvSeries has no episodes
                            series = null;
                            totalSeries--;
                            continue;
                        }

                        series.Episodes.Add(p);
                        MoviePropertiesList.RemoveAt(i);
                        totalEpisodes++;
                        i--;
                        kount--;
                        continue;
                    }

                    //Only gets here if this movieproperty is not a tvSeries or tvEpisode
                    if (series != null) //previous was a tvSeries, so cleanup so succeeding orphaned tvEpisodes won't be added.
                    {
                        if (series.Episodes.Count == 0) series.Episodes = null; //if tvSeries has no episodes
                        series = null;
                    }
                }

                Log.Write(Severity.Info, $"Total Videos={totalMovieProperties}, Movies={totalMovieProperties- totalSeries- totalEpisodes}, Series={totalSeries}, Episodes={totalEpisodes}.");

                if (this.MaxLoadedProperties > 0 && MoviePropertiesList.Count >= this.MaxLoadedProperties)
                {
                    Log.Write(Severity.Warning, $"Number of movies found ({MoviePropertiesList.Count}) exceeds the {this.MaxLoadedProperties} user limit MovieProperties. Truncating list.");
                    MoviePropertiesList.RemoveRange(this.MaxLoadedProperties, MoviePropertiesList.Count - this.MaxLoadedProperties);
                }

                MoviePropertiesList.TrimExcess();
                FilterProperties.InitAvailableValues(MoviePropertiesList);
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
            bool foldersChanged = (result.MediaFolders.Length != Settings.MediaFolders.Length ||
                                  !result.MediaFolders.OrderBy(m => m).SequenceEqual(Settings.MediaFolders.OrderBy(m => m)));
            Settings = result;

            if (foldersChanged)
            {
                SaveAllMovieProperties();
                MoviePropertiesList.Clear();
                LoadMovieInfo();
                m_flowPanel.SuspendLayout();
                m_flowPanel.Controls.Clear();
                Log.Write(Severity.Info, "Purge view cache due to new movie folders.");
                PurgeAllCache(null); 
                LoadTiles();
                EnableMenuBar(MoviePropertiesList.Count > 0);
            }
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
        /// Purge enough of the oldest views from cache to allow enough free space for the newest view.
        /// Too many open MovieProperty tiles (e.g. controls) may cause out-of-resources error.
        /// If there are too many movies, an out-of-window-handles (aka USER Objects) error 
        /// may occur and lock up the application where the only way out is to terminate via
        /// the Task Manager. A Win32 process may have a maximum of 10000 open USER Objects. 
        /// The following code helps mitigate this problem at the expense of performance.
        /// Rules:
        ///    **Small Tile Lite consists of 2 window handles. 10000/2 = 5000 movies
        ///    **Medium Tile Lite consists of 3 window handles. 10000/3 = 3333 movies
        ///    **Large Tile Lite consists of 3 window handles. 10000/3 = 3333 movies
        /// </summary>
        /// <param name="neededUserObjects"></param>
        /// <param name="cacheKey">new potential cache key (e.g. view key) (used for logging)</param>
        private void PurgeCache(int neededUserObjects, string cacheKey)
        {
            Func<int> availableUserObjects = () => 10000 - GetGuiResources(Process.GetCurrentProcess().Handle, 1); //uiFlags == 1 == GR_USEROBJECTS. Return the count of USER objects.

            if (Views.Count == 0) return;
            if (availableUserObjects() > neededUserObjects) return;
            var deletedList = new List<string>(Views.Count);

            var newestMainView = Views.OrderByDescending(m => m.Value.LastUsed).FirstOrDefault(m => m.Key.Contains(":" + ViewTiles.MainKey)); //there may be up to 3 "Main" views (e.g. small,medium, and large)

            foreach(var kv in Views.OrderBy(m=>m.Value.LastUsed)) //order by least used first
            {
                if (kv.Key == newestMainView.Key) continue;
                Log.Write(Severity.Warning, $"Insufficient resources (USER Objects) for new view {cacheKey}. Deleting oldest cached view {kv.Key}.");
                var t = kv.Value.Tiles;
                kv.Value.Tiles = null;  //remove all references so GC is free to collect tiles
                kv.Value.CurrentTile = null;
                DisposeTiles(t,false);
                deletedList.Add(kv.Key);
                if (availableUserObjects() > neededUserObjects) break;
            }

            //Ok, this new view is really, really large. No room for both the new view and parent view.
            if (availableUserObjects() < neededUserObjects)
            {
                var kv = newestMainView;
                Log.Write(Severity.Warning, $"Insufficient resources (USER Objects) for new view {cacheKey}. Deleting oldest cached view {kv.Key}.");
                var t = kv.Value.Tiles;
                kv.Value.Tiles = null;
                kv.Value.CurrentTile = null;
                DisposeTiles(t, false);
                deletedList.Add(kv.Key);
            }

            foreach (var kv in deletedList) { Views.Remove(kv); }

            //This should never happen... If it does, we have a bug.
            if (availableUserObjects() < neededUserObjects)
            {
                Log.Write(Severity.Error, $"Insufficient resources (USER Objects) for new view {cacheKey} even with view cache emptied! Requested UserObjects={neededUserObjects}. Available UserObjects={availableUserObjects()}");
                FatalUserObjectsMessage();
            }
        }

        private void FatalUserObjectsMessage()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(()=>FatalUserObjectsMessage()));
                return;
            }

            MessageBox.Show(this, "Cannot load new view due to insufficient system\nresources. VideoLibrarian cannot continue. Exiting...", "Loading Movies", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.SaveState();
            Environment.Exit(0);
        }

        /// <summary>
        /// Delete all cached tiles except self.
        /// </summary>
        /// <param name="cacheKey">Cache view to exclude. Null to just remove all views and all tiles.</param>
        private void PurgeAllCache(string cacheKey)
        {
            if (cacheKey==null)
            {
                foreach (var kv in Views)
                {
                    var t = kv.Value.Tiles;
                    kv.Value.Tiles = null;
                    kv.Value.CurrentTile = null;
                    DisposeTiles(t, true);
                }
                Views.Clear();
                return;
            }

            var key = ViewTiles.CreateKey(this.View, cacheKey); 
            var deletedList = new List<string>(Views.Count);
            foreach(var kv in Views)
            {
                if (kv.Key.Equals(key)) continue; //do not delete self
                var t = kv.Value.Tiles;
                kv.Value.Tiles = null;
                kv.Value.CurrentTile = null;
                DisposeTiles(t, true);
                deletedList.Add(kv.Key);
            }
            foreach (var kv in deletedList) { Views.Remove(kv); }
        }

        private void DisposeTiles(ITile[] tiles, bool postAction)
        {
            if (tiles.Length == 0) return;
            var c = (Control)tiles[0];
            if (c.InvokeRequired)
            {
                if (postAction) c.BeginInvoke((Action<ITile[], bool>)DisposeTiles, new object[] { tiles, postAction }); //this doesn't wait and returns immediately.
                else c.Invoke((Action<ITile[], bool>)DisposeTiles, new object[] { tiles, postAction }); //this waits for action to complete.
                return;
            }

            foreach (var t in tiles) ((Control)t).Dispose();
            GC.Collect();  //be sure tiles list is closed and memory deallocated.
        }
        #endregion

        #region private class ViewTiles
        /// <summary>
        /// Cached item
        /// </summary>
        private class ViewTiles
        {
            public const string MainKey = "Main"; //label for top-level View

            public ITile[] Tiles;
            public ITile CurrentTile; //used in sorting and filtering in order to track where the current tile is located and adjust the scroll position accordingly.
            public int ScrollPosition;
            public int LastUsed;  //used by Purge() to delete oldest list
            public string Key; //The key representing this object in a dictionary.

            //A delimited list of these keys will be serialized into a long string, so we try to make this unique and as small as possible.
            public static string CreateKey(ViewType view, string titleId = null) => $"{view}:{(string.IsNullOrEmpty(titleId) ? MainKey : titleId)}";
        }
        #endregion
    }
}
