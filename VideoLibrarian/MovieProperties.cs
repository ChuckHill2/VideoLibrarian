//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="MovieProperties.cs" company="Chuck Hill">
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
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

//-------------------------------------------------------------
// Regex pattern testing:
// https://regex101.com
// https://regexr.com/2vckg
// http://www.i2symbol.com/abc-123/h
//-------------------------------------------------------------

namespace VideoLibrarian
{
    /// <summary>
    /// All the requisite properties to display in the UI.
    /// This class populates itself from an xml cache or scraped from the internet if the cache doesn't exist.
    /// </summary>
    public class MovieProperties
    {
        public const string MovieExtensions = "|.3g2|.3gp|.3gp2|.3gpp|.amv|.asf|.avi|.bik|.bin|.crf|.divx|.drc|.dv|.dvr-ms|.evo|.f4v|.flv|.gvi|.gxf|.iso|.m1v|.m2v|.m2t|.m2ts|.m4v|.mkv|.mov|.mp2|.mp2v|.mp4|.mp4v|.mpe|.mpeg|.mpeg1|.mpeg2|.mpeg4|.mpg|.mpv2|.mts|.mtv|.mxf|.mxg|.nsv|.nuv|.ogg|.ogm|.ogv|.ogx|.ps|.rec|.rm|.rmvb|.rpl|.thp|.tod|.tp|.ts|.tts|.txd|.vob|.vro|.webm|.wm|.wmv|.wtv|.xesc|";
        public const string ImageExtensions = "|.bmp|.dib|.rle|.emf|.gif|.jpg|.jpeg|.jpe|.jif|.jfif|.png|.tif|.tiff|.xif|.wmf|";
        public const string EmptyTitleID = "tt0000000"; //IMDB empty title id, used here specifically to represent non-IMDB movies. 
        private const RegexOptions RE_options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
        private Task _getMoviePropertyTask = null;  //Async state for slowly extracting video file properties from video file. Needed to make Serialization() wait before serializing.
        private string _movieName = "";             //Load-on-demand property variables
        private string _moviePosterPath = null;
        private Image  _moviePosterImg = null;
        private string _fullName = null;
        private string _sortKey = null;

        #region Properties
        // Retrieved from current folder

        /// <summary>The IMDB title ɦttps:// style url to extract this property info from OR fіle:/// style folder url </summary>
        public string UrlLink { get; set; } = "";
        /// <summary>The IMDB poster url ɦttps:// or fіle:/// style url where to retrieve image from.</summary>
        public string MoviePosterUrl { get; set; } = "";

        /// <summary>Name of URL shortcut file (.url) containing this IMDB url.</summary>
        [XmlIgnore] public string ShortcutPath { get; set; } = "";
        /// <summary>Path of cached movie poster jpg file.</summary>
        [XmlIgnore] public string MoviePosterPath { get { return _moviePosterPath; } set { _moviePosterPath = value; _moviePosterImg = null; } }

        /// <summary>Name of XML file containing this property info.</summary>
        [XmlIgnore] public string PropertiesPath { get; set; } = "";
        /// <summary>Path to downloaded web page file. Minor effiency when just the XML cache file is corrupted or missing.</summary>
        [XmlIgnore] public string HtmlPath { get; set; } = "";
        /// <summary>Full path to video file.</summary>
        [XmlIgnore] public string MoviePath { get; set; } = "";
        /// <summary>Name (not path) of parent folder containing these files. Used for sorting.</summary>
        [XmlIgnore] public string FolderName { get; set; } = "";

        // Extracted from IMDB html

        /// <summary>The name of this movie as found in IMDB</summary>
        public string MovieName
        {
            get { return _movieName; }
            set { _movieName = value; _fullName = ""; _sortKey = ""; }
        }
        /// <summary>Comma-delimited list of genres</summary>
        public string[] Genre { get; set; } = new string[0];
        /// <summary>The type of movie/video</summary>
        public string MovieClass { get; set; } = "";
        /// <summary>Year movie was released</summary>
        public int Year { get; set; }           //release year
        /// <summary>Full mm/dd/yyyy date movie was released.</summary>
        [XmlElement(DataType = "date")]
        public DateTime ReleaseDate { get; set; }
        /// <summary>Popularity of movie between 0.0 -> 10.0 where 0.0 == unrated</summary>
        public float MovieRating { get; set; }
        /// <summary>Short description</summary>
        public string Plot { get; set; } = "";
        /// <summary>Long description</summary>
        public string Summary { get; set; } = "";
        /// <summary>Comma-delimited list of principal creators</summary>
        public string Creators { get; set; } = "";
        /// <summary>Comma-delimited list of main directors</summary>
        public string Directors { get; set; } = "";
        /// <summary>Comma-delimited list of principal writers</summary>
        public string Writers { get; set; } = "";
        /// <summary>Comma-delimited list of principal actors</summary>
        public string Cast { get; set; } = "";

        // TV Series Episodes

        /// <summary>The final year that the TV series ran. Zero if on-going. Ignored if not a series header.</summary>
        public int YearEnd { get; set; }
        /// <summary>TV Series season. If zero, this is a movie or TV series header, not a series episode</summary>
        public int Season { get; set; }
        /// <summary>Episode# within this TV series season. May be any number, even negative!</summary>
        public int Episode { get; set; }
        /// <summary>Total number of episodes in this TV series. If non-zero, this must be a series header.</summary>
        public int EpisodeCount { get; set; }
        /// <summary>Array of associated episode movie properties for this TV series header. Populated by caller</summary>
        [XmlIgnore] public List<MovieProperties> Episodes = null;

        // Local video file properties (see GetMovieFileProperties())

        /// <summary>Date video was downloaded (e.g. file created date)</summary>
        [XmlElement(DataType = "dateTime")]
        public DateTime DownloadDate { get; set; }
        /// <summary>Duration of video in minutes.</summary>
        public int Runtime { get; set; }
        /// <summary>Aspect ratio of video (e.g. "16:9")</summary>
        public string DisplayRatio { get; set; } = "0:0";
        /// <summary>Video width in pixels</summary>
        public int DisplayWidth { get; set; }
        /// <summary>Video height in pixels</summary>
        public int DisplayHeight { get; set; }

        /// <summary>Date video was viewed by user. Unwatched=='0001-01-01' aka DateTime.MinValue</summary>
        [XmlElement(DataType = "date")]
        public DateTime Watched { get; set; }

        // Derived/Computed info.

        /// <summary>
        /// Get cached/loaded poster image. May be disposed and subsequently reloaded upon demand.
        /// </summary>
        [XmlIgnore] public Image MoviePosterImg
        {
            get
            {
                // if image object not yet loaded or image object disposed... 
                if (_moviePosterImg == null || _moviePosterImg.PixelFormat == System.Drawing.Imaging.PixelFormat.Undefined)
                {
                    if (MoviePosterPath.IsNullOrEmpty() || !File.Exists(MoviePosterPath))
                    {
                        if (MoviePosterUrl.IsNullOrEmpty()) SetMoviePosterUrl(null); //Get the poster URL (not the jpg image) from the IMDB web site.

                        if (!MoviePosterUrl.IsNullOrEmpty())
                        {
                            if (MoviePosterUrl.StartsWith("http"))
                            {
                                var job = new FileEx.Job(null, MoviePosterUrl, this.PathPrefix + FileEx.GetUrlExtension(MoviePosterUrl));
                                if (FileEx.Download(job))
                                {
                                    MoviePosterPath = job.Filename;
                                    TileBase.PurgeTileImages(Path.GetDirectoryName(this.PathPrefix));
                                }
                            }
                            else
                            {
                                var file = MoviePosterUrl.StartsWith("file:///") ? new Uri(MoviePosterUrl).LocalPath : MoviePosterUrl;
                                if (File.Exists(file))
                                {
                                    MoviePosterPath = Path.ChangeExtension(MoviePosterPath, Path.GetExtension(file));
                                    File.Copy(file, MoviePosterPath, true);
                                    TileBase.PurgeTileImages(Path.GetDirectoryName(ShortcutPath));
                                }
                            }
                        }
                    }

                    if (MoviePosterPath.IsNullOrEmpty() || !File.Exists(MoviePosterPath)) _moviePosterImg = CreateBlankPoster(this.MovieName);
                    else
                    {
                        try
                        {
                            _moviePosterImg = new Bitmap(MoviePosterPath);
                        }
                        catch(Exception ex)
                        {
                            File.Delete(MoviePosterPath);
                            Log.Write(Severity.Error, $"Corrupted poster image. Recreating poster image file \"{MoviePosterPath}\".\n{ex}");
                            return MoviePosterImg;
                        }
                    }
                }
                return _moviePosterImg;
            }
        }

        /// <summary>
        /// Create-on-demand filename-friendly movie name 
        /// eg. "moviename (year)" or "seriesname S01E01 desc (year)"
        /// </summary>
        [XmlIgnore] public string FullMovieName
        {
            get
            {
                if (_fullName.IsNullOrEmpty()) _fullName = CreateFullMovieName();
                return _fullName.IsNullOrEmpty() ? "UNKNOWN" : _fullName;
            }
        }

        /// <summary>
        /// Create-on-demand sort key 
        /// eg. "moviename (year)" or "seriesname (year) S01E01"
        /// Used by caller to group movies and TV series.
        /// </summary>
        [XmlIgnore] public string SortKey
        {
            get
            {
                if (_sortKey.IsNullOrEmpty()) _sortKey = CreateSortKey();
                return _sortKey.IsNullOrEmpty() ? "UNKNOWN" :_sortKey;
            }
        }

        /// <summary>
        /// Handy full file path prefix to create full path name. ex. "D:\dir\dir\tt0123456".
        /// Just append the suffix. (e.g. ".xml" or "-myname.png")
        /// </summary>
        [XmlIgnore] public string PathPrefix { get; set; }
        #endregion //Properties

        #region Constructors
        /// <summary>
        /// Instantiate empty movie properties object. Properties are preset to defaults.
        /// Used for deserialization and manually populating these properties. Note that, some properties 
        /// must be already assigned BEFORE calling some of the functions and readonly properties herein. 
        /// The method and propertie descriptions will describe which properties need to be pre-populated.
        /// </summary>
        public MovieProperties() { }

        /// <summary>
        /// Fully instantiate properties for a single movie found in specified folder.
        /// May atttempt to go on internet to retrieve missing cache properties.
        /// </summary>
        /// <param name="path">Full directory path containing the sole IMDB movie page shortcut file.</param>
        /// <param name="forceRefresh">True to update existing xml properties file and tt*.png files from prior versions. See FormMain.cs(72).</param>
        /// <param name="loadAvailable">True to just quietly load cached properties without attempting to retrieve them from the internet.</param>
        public MovieProperties(string path, bool forceRefresh = false, bool loadAvailable = false)
        {
            FindFiles(path, loadAvailable);

            if (!Deserialize(PropertiesPath) && !loadAvailable)
            {
                if (PathPrefix.EndsWith(EmptyTitleID)) throw new XmlException("Content of manually created movie properties file {PropertiesPath} is invalid.");

                _getMoviePropertyTask = Task.Run(() => GetVideoFileProperties());
                if (File.Exists(HtmlPath)) ParseImdbPage(new FileEx.Job(null, UrlLink, HtmlPath));
                else
                {
                    // File.Delete(HtmlPath);
                    var job = new FileEx.Job(null, UrlLink, HtmlPath);
                    if (!FileEx.Download(job)) return; // FileEx.Download() logs its own errors. It will also update data.Url to redirected path and job.Filename
                    HtmlPath = job.Filename;
                    ParseImdbPage(job);
                }

                // Rename url shortcut filename to friendly name.
                var dst = Path.Combine(Path.GetDirectoryName(this.ShortcutPath), this.FullMovieName + Path.GetExtension(this.ShortcutPath));
                if (ShortcutPath != dst) File.Move(ShortcutPath, dst);
                ShortcutPath = dst;

                this.Serialize();
                Log.Write(Severity.None, "Added new movie \"{0}\" at {1}", this.FullMovieName, Path.GetDirectoryName(PropertiesPath));
            }
            else if (forceRefresh && !loadAvailable)
            {
                if (PathPrefix.EndsWith(EmptyTitleID)) throw new DataException("Manually created movie properties file {PropertiesPath} cannot be automatically recreated.");
                // if (!HtmlPath.IsNullOrEmpty()) File.Delete(HtmlPath) //Deep refresh. Will download web page again.
                File.Delete(PropertiesPath); //Force re-parse of downloaded web page.
                var p = new MovieProperties(path);

                // Ignore all derived properties including all derived directory/file paths 
                // SetAllProperties(p, this, new string[] { "MoviePosterUrl", "Episode", "Watched" });
                // SetOnlyTheseProperties(p, this, new string[] { "Creators" });
                if (this.Creators != p.Creators || 
                    this.Directors != p.Directors || 
                    this.Writers != p.Writers || 
                    this.Cast != p.Cast)
                {
                    this.Creators = p.Creators;
                    this.Directors = p.Directors;
                    this.Writers = p.Writers;
                    this.Cast = p.Cast;

                    TileBase.PurgeTileImages(Path.GetDirectoryName(ShortcutPath));
                    this.Serialize();
                }
            }
        }
        #endregion

        /// <summary>
        /// Populate the following path properties:
        ///    UrlLinkPath     (must have exactly one valid shortcut file, else exception is thrown)
        ///    UrlLink         (must refer to imdb.com, else exception is thrown)
        ///    PathPrefix      (generated from directory and url link)
        ///    PropertiesPath  (set to PathPrefix + ".xml". File may or may not exist)
        ///    HtmlPath        (set to PathPrefix + ".htm". File may or may not exist)
        ///    MoviePosterPath (set to PathPrefix + ".jpg". File may or may not exist)
        ///    MoviePath       (must have exactly 0 or 1 movie in this folder, else exception is thrown)
        ///    FolderName      (generated from directory path)
        /// </summary>
        /// <param name="dir">Full directory path containing the sole IMDB movie page shortcut file.</param>
        /// <param name="loadAvailable">True to just quietly load cached properties that currently exist only. For VideoOrganizer property editor.</param>
        private void FindFiles(string dir, bool loadAvailable)
        {
            if (dir.IsNullOrEmpty()) throw new ArgumentNullException("dir");
            if (!Directory.Exists(dir)) throw new DirectoryNotFoundException(string.Format("Directory \"{0}\" not found.", dir));

            foreach (var f in Directory.EnumerateFiles(dir, "*.url"))
            {
                var link = GetUrlFromShortcut(f);
                if (link.IsNullOrEmpty()) continue;


                var tt = GetTitleId(link);
                if (tt.IsNullOrEmpty()) continue;
                UrlLink = link;
                ShortcutPath = f;
                if (tt == EmptyTitleID)  //must be file:///, manually generated movie properties.
                {
                    var p = new Uri(dir).ToString();
                    if (link != p)  //Tidyness: Recreate shortcut since folder url shortcut does not point to this folder. User moved folder?
                    {
                        File.WriteAllText(f, $"[InternetShortcut]\nURL={p}\nIconIndex=129\nIconFile=C:\\Windows\\System32\\SHELL32.dll\nAuthor=VideoLibrarian.exe");
                    }
                }

                PathPrefix = string.Concat(dir, "\\", tt);
            }

            if (!loadAvailable && ShortcutPath.IsNullOrEmpty()) throw new FileNotFoundException("IMDB movie shortcut not found", dir);

            if (!ShortcutPath.IsNullOrEmpty())
            {
                // These have strict filenames.
                PropertiesPath = PathPrefix + ".xml";
                if (!loadAvailable && PathPrefix.EndsWith(EmptyTitleID) && !File.Exists(PropertiesPath)) throw new FileNotFoundException("Manually created movie properties not found", PropertiesPath);
                HtmlPath = PathPrefix + ".htm";
                MoviePosterPath = PathPrefix + ".jpg";

                FolderName = Path.GetFileName(dir); //for user sorting
            }

            foreach (var f in Directory.EnumerateFiles(dir, "*.*"))
            {
                if (IsVideoFile(f))
                {
                    if (!MoviePath.IsNullOrEmpty()) throw new DuplicateNameException(string.Format("{0}: Movie file already exists. There can only be one in this folder.", dir));
                    MoviePath = f;
                }
            }
        }

        private void ParseImdbPage(FileEx.Job data)
        {
            MatchCollection mc;
            string html = FileEx.ReadHtml(data.Filename);  //no duplicate whitespace, no whitespace before '<' and no whitespace after '>'

            if (Regex.IsMatch(html, @"<script id='__NEXT_DATA__' type='application/json'>", RE_options))
            {
                var html2 = File.ReadAllText(data.Filename);
                mc = Regex.Matches(html2, @"<script id=""__NEXT_DATA__"" type=""application/json"">(?<JSON>.+?)</script><script nomodule", RE_options);
                if (mc.Count > 0)
                {
                    try
                    {
                        ParseIMDBJson(mc[0].Groups["JSON"].Value);
                        return;
                    }
                    catch(Exception ex)
                    {
                        Log.Write(Severity.Error, $"Error retrieving values from JSON string embedded in \"{data.Filename}\". Fallback to web scraping.\n{ex}");
                    }
                }
            }

            // <link rel='canonical' href='https://www.imdb.com/title/tt0062622/'/>
            // <meta property='og:url' content='http://www.imdb.com/title/tt0062622/'/>
            mc = Regex.Matches(html, @"(<link rel='canonical' href='|<meta property='og:url' content=')(?<URL>[^']+)'/>", RE_options);
            if (mc.Count > 0) UrlLink = mc[0].Groups["URL"].Value;

            // <title>2001: A Space Odyssey (1968) - IMDb</title>
            // <meta name='title' content='2001: A Space Odyssey (1968) - IMDb'/>
            // <meta property='og:title' content='2001: A Space Odyssey (1968)'/>
            // <meta property='og:title' content='Epoch: Evolution (TV Movie 2003)'>
            mc = Regex.Matches(html, @"(?:<title>|<meta name='title' content='|<meta property='og:title' content=')(?<TITLE>.+?)(?:<\/title>|'>|'\/>)", RE_options);
            if (mc.Count > 0)
            {
                var title = WebUtility.HtmlDecode(mc[0].Groups["TITLE"].Value);
                // Childhood's End (TV Mini-Series 2015) - IMDb
                // Gotham (TV Series 2014- ) - IMDb
                // Gotham (TV Series 2014-2019) - IMDb
                // "Gotham" A Dark Knight: The Fear Reaper (TV Episode 2017) - IMDb
                // "BrainDead" Back to Work: A Behind-the-Scenes Look at Congress and How It Gets Things Done (and Often Doesn't) (TV Episode 2016) - IMDb
                // 1492: Conquest of Paradise (1992) - IMDb
                // Primeval: New World (TV Series 2013) - IMDb
                // Primeval: New World (TV Series 2012–2013) - IMDb
                // "Primeval: New World" The New World (TV Episode 2012) - IMDb
                // Ant-Man (2015) - IMDb
                // "Threshold" Trees Made of Glass: Part 1 (TV Episode 2005) - IMDb
                // Epoch: Evolution (TV Movie 2003)
                mc = Regex.Matches(title, @"^(?:(?:""(?<TITLE>[^""]+)"" (?<EPISODE>.+) (?=\([^\(]+$))|(?<TITLE2>.+))\((?:(?<TYPE>.+?) )?(?<YEAR>[0-9]{4,4})[–-]?(?<YEAREND>[0-9]{4,4})?", RE_options);
                if (mc.Count > 0)
                {
                    var titlex = mc[0].Groups["TITLE"].Value.Trim();
                    if (titlex.IsNullOrEmpty()) titlex = mc[0].Groups["TITLE2"].Value.Trim();
                    var episode = mc[0].Groups["EPISODE"].Value.Trim();
                    MovieName = episode.Length > 1 ? string.Concat(titlex, " \xAD ", episode) : titlex;
                    var year = mc[0].Groups["YEAR"].Value;
                    if (!year.IsNullOrEmpty()) { Year = int.Parse(year); ReleaseDate = new DateTime(Year, 1, 1); }
                    var yearend = mc[0].Groups["YEAREND"].Value;
                    if (!yearend.IsNullOrEmpty()) YearEnd = int.Parse(yearend);
                    var type = mc[0].Groups["TYPE"].Value;
                    MovieClass = type.IsNullOrEmpty() ? "Feature Movie" : type;
                }
            }

            // <div class='summary_text'>Humanity finds a mysterious, obviously artificial object buried beneath the Lunar surface and, with the intelligent computer HAL 9000, sets off on a quest.</div>
            // <div class="summary_text">For all intents and purposes, the Apocalypse has happened. "Aftermath" is the story of those who have survived. But their nightmare has only begun. In a world where those infected kill and ...<a href="/title/tt2584642/plotsummary?ref_=tt_ov_pl">See full summary</a>&nbsp;»</div>
            mc = Regex.Matches(html, @"<div class='summary_text'>(?<PLOT>.+?)</div>", RE_options);
            if (mc.Count > 0)
            {
                Plot = mc[0].Groups["PLOT"].Value;
                if (Plot.Contains("...<")) Plot = Plot.Substring(0, Plot.LastIndexOf("...<") + 3); //remove trailing "<a href="/title/tt2584642/plotsummary?ref_=tt_ov_pl">See full summary</a>&nbsp;»"
                if (Plot.Any(c => c == '<')) //string contains '<a href>' links. We remove them here.
                {
                    var e = Plot.Split(new char[] { '<', '>' });
                    var sb = new StringBuilder(Plot.Length);
                    for (int i = 0; i < e.Length; i += 2)
                    {
                        sb.Append(e[i].Trim());
                        sb.Append(' ');
                    }
                    sb.Length -= 1;
                    Plot = sb.ToString();
                }
            }
            else
            {
                // <meta name='description' content='Directed by Stanley Kubrick.  With Keir Dullea, Gary Lockwood, William Sylvester, Daniel Richter. Humanity finds a mysterious, obviously artificial object buried beneath the Lunar surface and, with the intelligent computer HAL 9000, sets off on a quest.'/>
                // <meta property='og:description' content='Directed by Stanley Kubrick.  With Keir Dullea, Gary Lockwood, William Sylvester, Daniel Richter. Humanity finds a mysterious, obviously artificial object buried beneath the Lunar surface and, with the intelligent computer HAL 9000, sets off on a quest.'/>
                mc = Regex.Matches(html, @"(?:<meta name='description' content='|<meta property='og:description' content=')(?<PLOT>.+?)(?:'>|'\/>)", RE_options);
                if (mc.Count > 0)
                {
                    Plot = mc[0].Groups["PLOT"].Value;
                }
            }

            // <h2>Storyline</h2><div class='inline canwrap'><p><span>'2001' is a story of evolution. Sometime in the distant past, someone or something nudged evolution by placing a monolith on Earth (presumably elsewhere throughout the universe as well).</span>
            // <h2>Storyline</h2><div class='inline canwrap'><p><span>In the aftermath of<a href='/title/tt3498820?ref_=tt_stry_pl'>Captain America: Civil War</a>(2016), Scott Lang grapples with the consequences of his choices as both a superhero and a father.</span>
            // <h2>Storyline</h2><div class='inline canwrap'><p><span>In the aftermath of<a href='/title/tt3498820?ref_=tt_stry_pl'>Captain America: Civil War</a>(2016), Scott Lang grapples with the consequences of his choices as both a superhero and a father. As he struggles to rebalance his home life with his responsibilities as <a href='/title/tt3498820?ref_=tt_stry_pl'>Ant-Man</a>, he's confronted by Hope van Dyne and Dr. Hank Pym with an urgent new mission.</span>
            // <h2>Storyline</h2><div class='inline canwrap'><p><span>In the aftermath of<a href='/title/tt3498820?ref_=tt_stry_pl'>Captain America: Civil War</a>(2016), Scott Lang grapples with the consequences of his choices as both a superhero and a father. As he struggles to rebalance his home life with his responsibilities as <a href='/title/tt3498820?ref_=tt_stry_pl'>Ant-Man</a>, he's confronted by Hope van Dyne and Dr. Hank Pym with an urgent new mission. Scott must once again put on the suit and learn to fight alongside <a href='/title/tt3498820?ref_=tt_stry_pl'>The Wasp</a> as the team works together to uncover secrets from their past.</span>
            mc = Regex.Matches(html, @"<h2>Storyline<\/h2>.+?<span>(?<SUMMARY>.+?)<\/span>", RE_options);
            if (mc.Count > 0)
            {
                Summary = mc[0].Groups["SUMMARY"].Value;
                if (Summary.Any(c=>c=='<')) //string contains '<a href>' links. We remove them here.
                {
                    var e = Summary.Split(new char[] { '<','>' });
                    var sb = new StringBuilder(Summary.Length);
                    for(int i=0; i< e.Length; i+=2)
                    {
                        sb.Append(e[i].Trim());
                        sb.Append(' ');
                    }
                    sb.Length -= 1;
                    Summary = sb.ToString();
                }
            }

            // <div class='credit_summary_item'><h4 class='inline'>Director:</h4><a href='/name/nm0000040/?ref_=tt_ov_dr' >Stanley Kubrick</a></div>
            // <div class='credit_summary_item'><h4 class='inline'>Writers:</h4><a href='/name/nm0000040/?ref_=tt_ov_wr' >Stanley Kubrick</a> (screenplay), <a href='/name/nm0002009/?ref_=tt_ov_wr' >Arthur C. Clarke</a> (screenplay)</div>
            // <div class='credit_summary_item'><h4 class='inline'>Stars:</h4><a href='/name/nm0001158/?ref_=tt_ov_st_sm' >Keir Dullea</a>, <a href='/name/nm0516972/?ref_=tt_ov_st_sm' >Gary Lockwood</a>, <a href='/name/nm0843213/?ref_=tt_ov_st_sm' >William Sylvester</a>
            mc = Regex.Matches(html, @"<div class='credit_summary_item'><h4 class='inline'>(?<KEY>Creator|Director|Writer|Star)s?:<\/h4>(?:<a href='[^']+'>(?<VALUE>[^<]+)<\/a>(?:.*?))+<\/div>", RE_options);
            if (mc.Count > 0)
            {
                foreach (Match m in mc)
                {
                    var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase); //unique list of names
                    foreach (Capture c in m.Groups["VALUE"].Captures)
                    {
                        if (c.Value.StartsWith("See", StringComparison.OrdinalIgnoreCase)) continue;  //ignore "See full cast & crew"
                        if (c.Value.Contains("credit")) continue;  //ignore "7 more credits" or "1 more credit"
                        list.Add(c.Value);
                    }
                    var sb = new StringBuilder();
                    foreach (var s in list)
                    {
                        sb.Append(s);
                        sb.Append(", ");
                    }
                    if (sb.Length > 2) sb.Length -= 2;
                    var value = sb.ToString();

                    var key = m.Groups["KEY"].Value.ToUpperInvariant();
                    if (key == "CREATOR") Creators = value;
                    else if (key == "DIRECTOR") Directors = value;
                    else if (key == "WRITER") Writers = value;
                    else if (key == "STAR") Cast = value;
                }
            }

            // <h4 class='inline'>Genres:</h4><a href='/genre/Adventure?ref_=tt_stry_gnr'>Adventure</a>&nbsp;<span>|</span><a href='/genre/Sci-Fi?ref_=tt_stry_gnr'>Sci-Fi</a></div>
            // <a href='/genre/Adventure?ref_=tt_ov_inf'>Adventure</a>,<a href='/genre/Sci-Fi?ref_=tt_ov_inf'>Sci-Fi</a>
            // <a href='/search/title?genres=adventure&explore=title_type,genres&ref_=tt_ov_inf'>Adventure</a>,<a href='/search/title?genres=sci-fi&explore=title_type,genres&ref_=tt_ov_inf'>Sci-Fi</a>
            mc = Regex.Matches(html, @"<a href='.+?genre[^']+'>(?<GENRE>[^<]+)<\/a>", RE_options);
            if (mc.Count > 0)
            {
                var list = new HashSet<string>();
                foreach (Match m in mc)
                {
                    var value = m.Groups["GENRE"].Value;
                    if (value.StartsWith("Most", StringComparison.OrdinalIgnoreCase)) continue;
                    list.Add(value);
                }
                Genre = list.ToArray();
            }

            // <a href='/title/tt0062622/releaseinfo?ref_=tt_ov_inf' title='See more release dates'>12 May 1968 (UK)</a>
            // <a href='/title/tt4525842/releaseinfo?ref_=tt_ov_inf' title='See more release dates'>Episode aired 26 October 2015</a>
            // <h4 class='inline'>Release Date:</h4> 12 May 1968 (UK)<
            mc = Regex.Matches(html, @"(?:<h4 class='inline'>Release Date:<\/h4>|<a href='\/title\/[^\/]+\/releaseinfo[^']*'[^<]+>).*?(?<DATE>[0-9]{1,2} [a-z]+ [0-9]{4,4})", RE_options);
            if (mc.Count > 0)
            {
                DateTime dtMin = DateTime.MaxValue;
                DateTime dt;
                foreach (Match m in mc)
                {
                    if (DateTime.TryParse(m.Groups["DATE"].Value, out dt)) { if (dt < dtMin) dtMin = dt; }
                }
                ReleaseDate = dtMin;
                if (Year == 0) Year = dtMin.Year; 
            }

            // Set this.MoviePosterUrl 
            SetMoviePosterUrl(html);

            // Movie Type?
            // <meta property='og:type' content='video.movie' />  == Movie (default value)
            // <meta property='og:type' content='video.tv_show'>  == TV Show
            // <meta property='og:type' content='video.episode'>  == TV Episode'
            // <meta property='og:type' content='video.tv_show'>  == TV Mini

            // <div class='bp_description'><div class='bp_heading'>Episode Guide</div><span class='bp_sub_heading'>101 episodes</span></div>
            mc = Regex.Matches(html, @"<span class='bp_sub_heading'>(?<EPISODES>[0-9]+) episodes</span>", RE_options);
            if (mc.Count > 0)
            {
                int i;
                if (int.TryParse(mc[0].Groups["EPISODES"].Value, out i)) EpisodeCount = i;
            }

            // <div class='bp_description'><div class='bp_heading'>Season 4 <span class='ghost'>|</span> Episode 2</div></div>
            mc = Regex.Matches(html, @"<div class='bp_heading'>Season (?<S>[0-9]{1,2}).+?Episode (?<E>[0-9]{1,2})</div>", RE_options);
            if (mc.Count > 0)
            {
                int i;
                if (int.TryParse(mc[0].Groups["S"].Value, out i)) Season = i;
                if (int.TryParse(mc[0].Groups["E"].Value, out i)) Episode = i;
            }

            // <div class='ratingValue'><strong title='2.0 based on 2,085 user ratings'><span>2.0</span></strong><span class='grey'>/</span><span class='grey'>10</span></div>
            // <div class='ratingValue'><strong title='8.3 based on 516,780 user ratings'><span itemprop='ratingValue'>8.3</span></strong><span class='grey'>/</span><span class='grey' itemprop='bestRating'>10</span></div>
            mc = Regex.Matches(html, @"<div class='ratingValue'>.+?<span[^>]*>(?<RATING>[0-9\.]+)<\/span>", RE_options);
            if (mc.Count > 0)
            {
                float i;
                if (float.TryParse(mc[0].Groups["RATING"].Value, out i)) MovieRating = i > 10f ? i / 10f : i;
            }
            else
            {
                // <div class='metacriticScore score_favorable titleReviewBarSubItem'><span>82</span></div>
                mc = Regex.Matches(html, @"<div class='metacriticScore[^']*'><span>(?<RATING>[0-9]+)</span></div>", RE_options);
                if (mc.Count > 0)
                {
                    int i;
                    if (int.TryParse(mc[0].Groups["RATING"].Value, out i)) MovieRating = i / 10f;
                }
            }
        }

        private void ParseIMDBJson(string json)
        {
            int i;
            float f;
            var js = SimpleJSON.JSON.Parse(json);
            UrlLink = js["head"].Linq.FirstOrDefault(p => p.Value[0].Value == "link" && p.Value[1]["rel"].Value == "canonical").Value?[1]["href"].Value ?? "";

            // The required data is in 2 node trees. It's a bit fuzzy which node actually contains the values we are looking for, so we look in both.
            // It 'appears' the node containing the fewest properties contains most of the values we are looking for. Magic...
            var tt = js["props"]["requestContext"]["pageConst"].Value;
            var urqls = js["props"]["urqlState"].Linq.Where(p => p.Value["data"]["title"]["id"] == tt).OrderBy(p => p.Value["data"]["title"].Count).ToArray();
            var props1 = urqls[0].Value["data"]["title"];
            var props2 = urqls[1].Value["data"]["title"];

            var movieClass = props1["titleType"]["id"].Value;
            MovieClass = props1["titleType"]["text"].Value;
            if (MovieClass == "Movie") MovieClass = "Feature Movie";
            Year = int.TryParse(props1["releaseYear"]["year"], out i) ? i : 1900;

            MovieName = props1["series"]?["series"]?["titleText"]?["text"]?.Value;
            if (MovieName.IsNullOrEmpty())
                MovieName = props1["titleText"]?["text"]?.Value;
            else
                MovieName = string.Concat(MovieName, " \xAD ", props1["titleText"]?["text"]?.Value ?? "");

            var year  = int.TryParse(props1["releaseDate"]["year"], out i) ? i : Year;
            var month = int.TryParse(props1["releaseDate"]["month"], out i) ? i : 1;
            var day   = int.TryParse(props1["releaseDate"]["day"], out i) ? i : 1;
            ReleaseDate = new DateTime(year, month, day);
            if (Year == 1900) Year = year;

            YearEnd = int.TryParse(props1["releaseYear"]["endYear"], out i) ? i : 0;

            if (MoviePath.IsNullOrEmpty() || !File.Exists(this.MoviePath))
            {
                //This is normally set from async extract of video file properties: GetVideoFileProperties()
                Runtime = int.TryParse(props1["runtime"]["seconds"], out i) ? i / 60 : 0;
                if (Runtime == 0) Runtime = int.TryParse(props2["runtime"]["seconds"], out i) ? i / 60 : 0;
            }

            MovieRating = float.TryParse(props1["ratingsSummary"]["aggregateRating"], out f) ? f : 0;
            MoviePosterUrl = props1["primaryImage"]["url"]?.Value ?? "";

            var glist = new List<string>();
            foreach (var g in props1["genres"]["genres"])
            {
                glist.Add(g.Value["text"].Value);
            }
            Genre = glist.ToArray();

            Plot = props1["plot"]?["plotText"]?["plainText"]?.Value;
            if (Plot.IsNullOrEmpty()) Plot = props2["plot"]?["plotText"]?["plainText"]?.Value;
            if (!Plot.IsNullOrEmpty() && Plot.Contains('&')) Plot = WebUtility.HtmlDecode(Plot);

            Summary = props1["summaries"]?["edges"]?[0]?["node"]?["plotText"]?["plaidHtml"]?.Value;
            if (Summary.IsNullOrEmpty()) Summary = props2["summaries"]?["edges"]?[0]?["node"]?["plotText"]?["plaidHtml"]?.Value;
            if (!Summary.IsNullOrEmpty() && Summary.Contains('&')) Summary = WebUtility.HtmlDecode(Summary);

            var sb = new StringBuilder();
            foreach (var pc in props1["principalCredits"])
            {
                var id = pc.Value["category"]["id"].Value;  //director, writer, cast
                string comma = string.Empty;
                foreach (var cr in pc.Value["credits"])
                {
                    sb.Append(comma);
                    sb.Append(cr.Value["name"]["nameText"]["text"].Value);
                    comma = ", ";
                }
                switch (id)
                {
                    case "director": Directors = sb.ToString(); break;
                    case "writer": Writers = sb.ToString(); break;
                    case "cast": Cast = sb.ToString(); break;
                    case "creator": Creators = sb.ToString(); break;
                }
                sb.Length = 0;
            }

            Episode = int.TryParse(props1["series"]?["episodeNumber"]?["episodeNumber"], out i) ? i : 0;
            if (Episode == 0) Episode = int.TryParse(props2["series"]?["episodeNumber"]?["episodeNumber"], out i) ? i : 0;

            Season = int.TryParse(props1["series"]?["episodeNumber"]?["seasonNumber"], out i) ? i : 0;
            if (Season == 0) Season = int.TryParse(props2["series"]?["episodeNumber"]?["seasonNumber"], out i) ? i : 0;

            EpisodeCount = int.TryParse(props1["episodes"]?["episodes"]?["total"], out i) ? i : 0;
            if (EpisodeCount == 0) EpisodeCount = int.TryParse(props2["episodes"]?["episodes"]?["total"], out i) ? i : 0;
        }

        /// <summary>
        /// Set URL to movie poster if it doesn't already exist.
        /// Used by ParseImdbPage() and MoviePosterImg getter property.
        /// </summary>
        /// <param name="html"></param>
        /// <returns>True if this.MoviePosterUrl modified.</returns>
        private bool SetMoviePosterUrl(string html)
        {
            if (!this.MoviePosterUrl.IsNullOrEmpty()) return false;

            if (html.IsNullOrEmpty())  //Occurs when this method is lazily called by MoviePosterImg getter so we must load our cached copy of the IMDB movie page.
            {
                if (!File.Exists(HtmlPath)) //Oops. The cached IMDB movie page does not exist. Retrieve it.
                {
                    var job = new FileEx.Job(null, UrlLink, HtmlPath);
                    if (!FileEx.Download(job)) return false; // FileEx.Download() logs its own errors. It will also update data.Url to redirected path and job.Filename
                    HtmlPath = job.Filename;
                }

                html = FileEx.ReadHtml(HtmlPath);  //no duplicate whitespace, no whitespace before '<' and no whitespace after '>'
            }

            // <div class='poster'><a href='/title/tt0401729/mediaviewer/rm3022336?ref_=tt_ov_i'><img alt='John Carter Poster' title='John Carter Poster' src='https://m.media-amazon.com/images/M/MV5BMDEwZmIzNjYtNjUwNS00MzgzLWJiOGYtZWMxZGQ5NDcxZjUwXkEyXkFqcGdeQXVyNTIzOTk5ODM@._V1_UY268_CR6,0,182,268_AL_.jpg'></a></div>
            var mc = Regex.Matches(html, @"<div class='poster'><a href='(?<URL>\/title\/[^\/]+\/mediaviewer\/(?<ID>[0-9a-z]+)[^']+).+? src='(?<POSTER>[^']+)'", RE_options);
            if (mc.Count > 0)
            {
                var posterUrl = mc[0].Groups["POSTER"].Value; //get the small poster image in the page, but we continue to look for a larger/better image.
                var id = mc[0].Groups["ID"].Value;
                var mediaViewerUrl = FileEx.GetAbsoluteUrl(this.UrlLink, mc[0].Groups["URL"].Value);

                var fn = Path.Combine(Path.GetDirectoryName(this.MoviePosterPath), "MediaViewer.htm"); //temporary uncached web page containing large poster images.
                var job = new FileEx.Job(null, mediaViewerUrl, fn);
                if (FileEx.Download(job))
                {
                    var html2 = FileEx.ReadHtml(job.Filename);
                    File.Delete(job.Filename); //no longer needed. extension already used for this cache file.
                    // {'editTagsLink':'/registration/signin','id':'rm3022336','h':1000,'msrc':'https://m.media-amazon.com/images/M/MV5BMDEwZmIzNjYtNjUwNS00MzgzLWJiOGYtZWMxZGQ5NDcxZjUwXkEyXkFqcGdeQXVyNTIzOTk5ODM@._V1_SY500_CR0,0,364,500_AL_.jpg','src':'https://m.media-amazon.com/images/M/MV5BMDEwZmIzNjYtNjUwNS00MzgzLWJiOGYtZWMxZGQ5NDcxZjUwXkEyXkFqcGdeQXVyNTIzOTk5ODM@._V1_SY1000_CR0,0,728,1000_AL_.jpg','w':728,'imageCount':164,'altText':'Taylor Kitsch in John Carter (2012)','caption':'<a href=\'/name/nm2018237/\'>Taylor Kitsch</a> in <a href=\'/title/tt0401729/\'>John Carter (2012)</a>','imageType':'poster','relatedNames':[{'constId':'nm2018237','displayName':'Taylor Kitsch','url':'/name/nm2018237?ref_=tt_mv'}],'relatedTitles':[{'constId':'tt0401729','displayName':'John Carter','url':'/title/tt0401729?ref_=tt_mv'}],'reportImageLink':'/registration/signin','tracking':'/title/tt0401729/mediaviewer/rm3022336/tr','voteData':{'totalLikeVotes':0,'userVoteStatus':'favorite-off'},'votingLink':'/registration/signin'}],'baseUrl':'/title/tt0401729/mediaviewer','galleryIndexUrl':'/title/tt0401729/mediaindex','galleryTitle':'John Carter (2012)','id':'tt0401729','interstitialModel':
                    mc = Regex.Matches(html2, string.Concat("'id':'", id, "'.+?'src':'(?<POSTER>[^']+)'"), RE_options);
                    if (mc.Count > 0) posterUrl = mc[0].Groups["POSTER"].Value;
                    else
                    {
                        // <meta property='og:image' content='https://m.media-amazon.com/images/M/MV5BMDEwZmIzNjYtNjUwNS00MzgzLWJiOGYtZWMxZGQ5NDcxZjUwXkEyXkFqcGdeQXVyNTIzOTk5ODM@._V1_SY500_CR0,0,364,500_AL_.jpg'/>
                        // <meta itemprop='image' content='https://m.media-amazon.com/images/M/MV5BMDEwZmIzNjYtNjUwNS00MzgzLWJiOGYtZWMxZGQ5NDcxZjUwXkEyXkFqcGdeQXVyNTIzOTk5ODM@._V1_SY500_CR0,0,364,500_AL_.jpg'/>
                        // <meta name='twitter:image' content='https://m.media-amazon.com/images/M/MV5BMDEwZmIzNjYtNjUwNS00MzgzLWJiOGYtZWMxZGQ5NDcxZjUwXkEyXkFqcGdeQXVyNTIzOTk5ODM@._V1_SY500_CR0,0,364,500_AL_.jpg'/>
                        mc = Regex.Matches(html2, @"<meta (?:property='og:image'|itemprop='image'|name='twitter:image') content='(?<POSTER>[^']+)'", RE_options);
                        foreach (Match m in mc)
                        {
                            var x = m.Groups["POSTER"].Value;
                            // Value may contain "...\imdb_logo.png". All real posters are jpg files.
                            if (!x.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)) continue;
                            posterUrl = x;
                            break;
                        }
                    }
                }

                this.MoviePosterUrl = posterUrl;
                return true;
            }

            // Added 05/29/2021. Meta image properties now contain large poster image url's

            //<meta property='og:image' content='https://m.media-amazon.com/images/M/MV5BMTY5MzM2MzkxNF5BMl5BanBnXkFtZTgwNTMzMDcyNzM@._V1_FMjpg_UX1000_.jpg'/>
            //<meta property='twitter:image' content='https://m.media-amazon.com/images/M/MV5BMTY5MzM2MzkxNF5BMl5BanBnXkFtZTgwNTMzMDcyNzM@._V1_FMjpg_UX1000_.jpg'/>
            mc = Regex.Matches(html, @"<meta property='(?:og|twitter):image' content='(?<URL>https:[^']+)'", RE_options);
            if (mc.Count > 0)
            {
                this.MoviePosterUrl = mc[0].Groups["URL"].Value;
                return true;
            }

            //If it doesn't exist, get first image from poster carousel.

            //Url always contains 'mediaviewer' with property '?ref_=tt_ov_i'
            mc = Regex.Matches(html, @"href='(?<URL>\/title\/tt[^\/]+\/mediaviewer\/rm[^\/]+\/\?ref_=tt_ov_i)'", RE_options);
            if (mc.Count > 0)
            {
                var mediaViewerUrl = FileEx.GetAbsoluteUrl(this.UrlLink, mc[0].Groups["URL"].Value);

                var fn = Path.Combine(Path.GetDirectoryName(this.MoviePosterPath), "MediaViewer.htm"); //temporary uncached web page containing large poster images.
                var job = new FileEx.Job(null, mediaViewerUrl, fn);
                if (FileEx.Download(job))
                {
                    var html2 = FileEx.ReadHtml(job.Filename);
                    File.Delete(job.Filename); //no longer needed. extension already used for this cache file.
                    mc = Regex.Matches(html2, @"'(?<URL>https:\/\/m\.media-amazon\.com\/images\/[^' ]+\.jpg)'", RE_options);
                    if (mc.Count > 0)
                    {
                        this.MoviePosterUrl = mc[0].Groups["URL"].Value;
                        return true;
                    }
                }
            }

            return false; //no change. couldn't find poster url.
        }

        public void GetVideoFileProperties()
        {
            if (!MoviePath.IsNullOrEmpty() && File.Exists(this.MoviePath))
            {
                try
                {
                    this.DownloadDate = FileEx.GetCreationDate(this.MoviePath);
                    var info = MediaInfo.GetInfo(this.MoviePath);
                    if (info == null) throw new NullReferenceException("Media info is null.");
                    this.Runtime = (int)Math.Ceiling(info.Duration.TotalMinutes);
                    var stream = info.Streams.FirstOrDefault(p => p.CodecType.EqualsI("video"));
                    if (stream == null) throw new NullReferenceException("Media info video stream not found.");
                    this.DisplayWidth = stream.Width;
                    this.DisplayHeight = stream.Height;
                    this.DisplayRatio = ComputeNormalizedDisplayRatio(stream.Width, stream.Height);
                }
                catch(Exception ex)
                {
                    Log.Write(Severity.Error, "GetMovieFileProperties(\"{0}\"): {1}", MoviePath.IsNullOrEmpty() ? "null" : MoviePath, ex.Message);
                }
            }
            else //must be a TV Series root with no movie. Just child directories contining episode movies.
            {
                // Assign the first episode download date to this series property.
                string path = !ShortcutPath.IsNullOrEmpty() ? ShortcutPath : MoviePath;
                if (path.IsNullOrEmpty()) return;
                var dir = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
                foreach (var f in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories).OrderBy(s=>s,StringComparer.OrdinalIgnoreCase))
                {
                    if (!IsVideoFile(f)) continue;
                    path = f;
                    break;
                }
                this.DownloadDate = FileEx.GetCreationDate(path);
            }
        }

        private string CreateFullMovieName()
        {
            if (this.MovieName.IsNullOrEmpty()) return "";
            var name = this.MovieName;

            var chars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            char prevC = '\0';
            foreach (var c in name)
            {
                if (c == ':')
                {
                    if (prevC == ' ') sb.Length -= 1;
                    sb.Append(" - ");
                    prevC = ' ';
                    continue;
                }
                if (prevC == ' ' && c == ' ') continue;
                if (chars.Contains(c)) continue;
                sb.Append(c);
                prevC = c;
            }
            name = sb.ToString();

            if (this.Season > 0)
            {
                var i = name.IndexOf(" \xAD ");
                string n1 = name, n2 = "";
                if (i > 1)
                {
                    n1 = name.Substring(0, i);
                    n2 = name.Substring(i + 3);
                }
                name = string.Format("{0} S{1:00}E{2:00} {3} ({4})", n1, Season, Episode, n2, Year);
            }
            else name = string.Format("{0} ({1})", name, Year);

            return name;
        }

        private string CreateSortKey()
        {
            if (this.MovieName.IsNullOrEmpty()) return "";
            var name = this.MovieName;

            if (this.Season > 0)
            {
                var i = name.IndexOf(" \xAD "); //unicode dash
                string n1 = name;
                if (i > 1)
                {
                    n1 = name.Substring(0, i);
                    // n2 = name.Substring(i + 3); //ignore episode title
                }
                name = string.Format("{0} ({1}) S{2:00}E{3:00}", n1, Year, Season, Episode);
            }
            else name = string.Format("{0} ({1})", name, Year);

            return name;
        }

        private static string ComputeNormalizedDisplayRatio(int width, int height)
        {
            // The following computes accurate display ratio from width and height but because of 
            // slight variations width and/or height, the result comes out as non-standard ratios. 
            // Therefore, we just find the closest fit to the known video display ratios.

            // var isProgressive = stream.PixelFormat[stream.PixelFormat.Length - 1] == 'p';
            // private static int gcd(int a, int b) { return (b == 0) ? a : gcd(b, a % b); } //greatest common divisor
            // var r = gcd(width, height); //greatest common divisor
            // DisplayRatio = string.Format("{0}:{1}", stream.Width / r, stream.Height / r);
            // DisplayResolution = string.Format("{0}x{1}{2}", stream.Width, stream.Height, isProgressive ? "p" : "i");

            var asp = new KeyValuePair<string, double>[]
            {
                new KeyValuePair<string,double>("5:4",     5/4.0),  //1.25
                new KeyValuePair<string,double>("4:3",     4/3.0),  //1.3333333
                new KeyValuePair<string,double>("3:2",     3/2.0),  //1.5
                new KeyValuePair<string,double>("16:10", 16/10.0),  //1.6
                new KeyValuePair<string,double>("16:9",   16/9.0),  //1.777777
                new KeyValuePair<string,double>("1.85:1",  1.850),  //1.85
                new KeyValuePair<string,double>("2.39:1",  2.390),  //2.39
            };

            double actual = width / (double)height;
            double residual = 999;
            var index = -1;
            for (int i = 0; i < asp.Length; i++)
            {
                var r = Math.Abs(actual - asp[i].Value);
                if (r < residual) { residual = r; index = i; }
            }

            return asp[index].Key;
        }

        public static string GetUrlFromShortcut(string path)
        {
            using(var sr = new StreamReader(path))
            {
                string line = null;
                while((line=sr.ReadLine())!=null)
                {
                    if (line.StartsWith("URL="))
                    {
                        return line.Substring(4).Trim();
                    }
                }
            }
            return "";
        }

        private static Image CreateBlankPoster(string movieName)
        {
            var bmp = new Bitmap(364, 500, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(bmp);

            var gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(0, 0, bmp.Width, bmp.Height));
            var br = new PathGradientBrush(gp);
            br.CenterColor = Color.LightBlue;
            br.SurroundColors = new Color[] { Color.LightSteelBlue };
            g.FillRectangle(br, 0, 0, bmp.Width, bmp.Height);
            br.Dispose();
            gp.Dispose();

            var sf = new StringFormat(StringFormatFlags.LineLimit | StringFormatFlags.FitBlackBox);
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;

            var font = new Font(FontFamily.GenericSansSerif, 28, FontStyle.Bold);
            g.DrawString(movieName ?? "Missing Poster", font, Brushes.Black, new RectangleF(10, 10, bmp.Width - 20, bmp.Height - 20), sf);
            font.Dispose();

            sf.Dispose();
            g.Dispose();

            return bmp;
        }

        #region Serialization/Deserialization
        private MovieProperties OriginalMovieProperties = null;

        public void Serialize()
        {
            if (_getMoviePropertyTask != null) { _getMoviePropertyTask.Wait(); _getMoviePropertyTask.Dispose(); _getMoviePropertyTask = null; }
            if (this.Equals(OriginalMovieProperties)) return;

            XmlIO.Serialize(this, PropertiesPath);
            OriginalMovieProperties = new MovieProperties();
            SetAllProperties(this, OriginalMovieProperties);
        }

        private bool Deserialize(string path)
        {
            if (path.IsNullOrEmpty() || !File.Exists(path)) return false;
            try
            {
                var sd = XmlIO.Deserialize<MovieProperties>(path);
                OriginalMovieProperties = sd;
                SetAllProperties(sd, this);
            }
            catch(Exception ex)
            {
                Log.Write(Severity.Warning, "Deserialization Error. Rebuilding movie properties file.\n" + ex.ToString());
                return false;
            }
            return true;
        }

        private class PropertyList
        {
            public string Name { get; private set; }
            public Func<object, object> Get { get; private set; }
            public Action<object, object> Set { get; private set; }
            public PropertyList(string name, Func<object, object> get, Action<object, object> set)
            {
                Name = name;
                Get = get;
                Set = set;
            }
            public override string ToString() => this.Name;
        }

        private static readonly PropertyList[] PropertySetters = GetPropertyGetterSetters();

        private static PropertyList[] GetPropertyGetterSetters()
        {
            var pis = typeof(MovieProperties).GetProperties();
            var list = new List<PropertyList>(pis.Length);
            foreach (var pi in pis)
            {
                if (!pi.CanWrite) continue;
                if (pi.GetCustomAttribute<XmlIgnoreAttribute>() != null) continue;
                list.Add(new PropertyList(pi.Name, pi.GetValue, pi.SetValue));
            }
            return list.OrderBy(m => m.Name).ToArray();
        }

        private static void SetAllProperties(MovieProperties src, MovieProperties dst, ICollection<string> except = null)
        {
            if (except == null || except.Count == 0)
            {
                foreach (var setter in PropertySetters)
                {
                    setter.Set(dst, setter.Get(src));
                }
            }
            else
            {
                foreach (var setter in PropertySetters)
                {
                    if (except.Any(m => m.Equals(setter.Name, StringComparison.OrdinalIgnoreCase))) continue;
                    setter.Set(dst, setter.Get(src));
                }
            }
        }

        private static void SetOnlyTheseProperties(MovieProperties src, MovieProperties dst, ICollection<string> these)
        {
            if (these == null || these.Count == 0) return;
            foreach(var name in these)
            {
                var setter = PropertySetters.FirstOrDefault(pl => pl.Name.EqualsI(name));
                if (setter == null) continue;
                setter.Set(dst, setter.Get(src));
            }
        }
        #endregion

        public static bool IsVideoFile(string file)
        {
            try
            {
                if (file.StartsWith("http"))
                {
                    if (MovieProperties.MovieExtensions.IndexOf(FileEx.GetUrlExtension(file), StringComparison.CurrentCultureIgnoreCase) == -1) return false;
                }
                else
                {
                    if (file.StartsWith("file:///")) file = new Uri(file).LocalPath;
                    if (MovieProperties.MovieExtensions.IndexOf(Path.GetExtension(file), StringComparison.CurrentCultureIgnoreCase) == -1) return false;
                    if (!File.Exists(file)) return false;
                }
            }
            catch { return false; }

            return true;
        }

        public static bool IsImageFile(string file)
        {
            try
            {
                if (file.StartsWith("http"))
                {
                    if (MovieProperties.ImageExtensions.IndexOf(FileEx.GetUrlExtension(file), StringComparison.CurrentCultureIgnoreCase) == -1) return false;
                }
                else
                {
                    if (file.StartsWith("file:///")) file = new Uri(file).LocalPath;
                    if (MovieProperties.ImageExtensions.IndexOf(Path.GetExtension(file), StringComparison.CurrentCultureIgnoreCase) == -1) return false;
                    if (!File.Exists(file)) return false;
                }
            }
            catch { return false;  }

            return true;
        }

        /// <summary>
        /// Extract IMDB title id from url or empty title id if this is a local folder url.
        /// "ḣttps://www.imdb.com/title/tt0062622/" ==> "tt0062622"
        /// "fіle:///C:/Users/User/Videos" ==> "tt0000000"
        /// <summary>The IMDB poster url https:// or fіle:/// style url where to retrieve image from.</summary>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetTitleId(string url)
        {
            // Possible url permutations... an IMDB title shortcut or any local folder shortcut. 
            // Folder shortcuts may not point to current folder as folder may have been previously moved.
            //   http://www.imdb.com/title/tt2395385/
            //   https://imdb.com/title/tt2395385/?ref=xxxxx
            //   https://www.imdb.com/title/tt2395385/
            //   file:///C:/Users/User/Videos

            if (url.IsNullOrEmpty()) return null; //null or empty string.
            var m = ReTitleId.Match(url);
            if (!m.Success) return string.Empty;  //url not a match
            return m.Groups["TT"].Value == string.Empty ? MovieProperties.EmptyTitleID : m.Groups["TT"].Value;
        }
        private static readonly Regex ReTitleId = new Regex(@"^(?:(?:https?:\/\/(?:www\.)?imdb\.com\/title\/(?<TT>tt[0-9]+))|(?<FILE>file:\/\/\/))", RE_options);

        public override string ToString() => this.FullMovieName;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                foreach(var p in PropertySetters)
                {
                    if (p.Name=="Genre")
                        this.Genre.ForEach(g => { hash = (hash * 7) + g.GetHashCode(); });
                    else
                        hash = (hash * 7) + p.Get(this).GetHashCode();
                }

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MovieProperties)) return false;
            MovieProperties mp = (MovieProperties)obj;

            ////DEBUGGING -- These are all the NOT XmlIgnore'd
            // var _Cast = this.Cast == mp.Cast;
            // var _Creators = this.Creators == mp.Creators;
            // var _Directors = this.Directors == mp.Directors;
            // var _DisplayHeight = this.DisplayHeight == mp.DisplayHeight;
            // var _DisplayRatio = this.DisplayRatio == mp.DisplayRatio;
            // var _DisplayWidth = this.DisplayWidth == mp.DisplayWidth;
            // var _DownloadDate = this.DownloadDate == mp.DownloadDate;
            // var _Episode = this.Episode == mp.Episode;
            // var _EpisodeCount = this.EpisodeCount == mp.EpisodeCount;
            // var _Genre = this.Genre.SequenceEqual(mp.Genre);   //<==not simple comparison
            // var _MovieClass = this.MovieClass == mp.MovieClass;
            // var _MovieName = this.MovieName == mp.MovieName;
            // var _MoviePosterUrl = this.MoviePosterUrl == mp.MoviePosterUrl;
            // var _MovieRating = this.MovieRating == mp.MovieRating;
            // var _Plot = this.Plot == mp.Plot;
            // var _ReleaseDate = this.ReleaseDate == mp.ReleaseDate;
            // var _Runtime = this.Runtime == mp.Runtime;
            // var _Season = this.Season == mp.Season;
            // var _Summary = this.Summary == mp.Summary;
            // var _UrlLink = this.UrlLink == mp.UrlLink;
            // var _Watched = this.Watched == mp.Watched;
            // var _Writers = this.Writers == mp.Writers;
            // var _Year = this.Year == mp.Year;
            // var _YearEnd = this.YearEnd == mp.YearEnd;
            // var total = _Cast && _Creators && _Directors && _DisplayHeight && _DisplayRatio && _DisplayWidth && _DownloadDate && _Episode && _EpisodeCount && _Genre && _MovieClass && _MovieName && _MoviePosterUrl && _MovieRating && _Plot && _ReleaseDate && _Runtime && _Season && _Summary && _UrlLink && _Watched && _Writers && _Year && _YearEnd;

            return  PropertySetters.All((p) => p.Name=="Genre"? true : p.Get(this).Equals(p.Get(mp))) && this.Genre.SequenceEqual(mp.Genre);
        }
    }
}
