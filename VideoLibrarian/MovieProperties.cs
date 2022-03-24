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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
// https://regex101.com  <-- easiest to use
// https://regexr.com/2vckg
// http://www.i2symbol.com/abc-123/h
//  http://regexstorm.net/tester      <-- supports multiple C# regex capture groups
//-------------------------------------------------------------

namespace VideoLibrarian
{
    /// <summary>
    /// All the requisite properties to display in the UI.
    /// This class populates itself from an xml cache or scraped from the internet if the cache doesn't exist.
    /// </summary>
    public class MovieProperties
    {
        #region Variables
        public const string MovieExtensions = "|.3g2|.3gp|.3gp2|.3gpp|.amv|.asf|.avi|.bik|.bin|.crf|.divx|.drc|.dv|.dvr-ms|.evo|.f4v|.flv|.gvi|.gxf|.iso|.m1v|.m2v|.m2t|.m2ts|.m4v|.mkv|.mov|.mp2|.mp2v|.mp4|.mp4v|.mpe|.mpeg|.mpeg1|.mpeg2|.mpeg4|.mpg|.mpv2|.mts|.mtv|.mxf|.mxg|.nsv|.nuv|.ogg|.ogm|.ogv|.ogx|.ps|.rec|.rm|.rmvb|.rpl|.thp|.tod|.tp|.ts|.tts|.txd|.vob|.vro|.webm|.wm|.wmv|.wtv|.xesc|";
        public const string ImageExtensions = "|.bmp|.dib|.rle|.emf|.gif|.jpg|.jpeg|.jpe|.jif|.jfif|.png|.tif|.tiff|.xif|.wmf|";
        public const string EmptyTitleID = "tt0000000"; //IMDB empty title id, used here specifically to represent non-IMDB movies. 
        private const RegexOptions RE_options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
        private Task _getMoviePropertyTask = null;  //Async state for slowly extracting video file properties from video file. Needed to make Serialization() wait before serializing.
        private string _movieName = "";             //Load-on-demand property variables
        private string _moviePosterPath = null;
        private Image _moviePosterImg = null;
        private string _fullName = null;
        private string _sortKey = null;
        #endregion

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
        /// <summary>The type of movie/video : Feature Movie, Short, TV Movie, Video; TV Mini-Series, TV Series, TV Episode</summary>
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
        /// <summary>MD5 hash of movie file content to validate that it is not corrupted.</summary>
        public Guid MovieHash { get; set; }
        /// <summary>Length of movie file to validate that it is not corrupted.</summary>
        public long MovieFileLength { get; set; }

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
                                var job = new FileEx.Job(MoviePosterUrl, this.PathPrefix + FileEx.GetUrlExtension(MoviePosterUrl), "https://www.imdb.com/");
                                if (FileEx.Download(job))
                                {
                                    MoviePosterPath = job.Filename;
                                    TileBase.PurgeTileImages(Path.GetDirectoryName(this.PathPrefix));
                                }
                                else this.DeleteFileCacheUponExit = FileCacheScope.ImagesOnly;
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
                        catch (Exception ex)
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
                return _sortKey.IsNullOrEmpty() ? "UNKNOWN" : _sortKey;
            }
        }

        /// <summary>
        /// Handy full file path prefix to create full path name. ex. "D:\dir\dir\tt0123456".
        /// Just append the suffix. (e.g. ".xml" or "-myname.png")
        /// </summary>
        [XmlIgnore] public string PathPrefix { get; set; }

        /// <summary>
        /// How to handle file cache when there is a download failure.
        /// </summary>
        /// <remarks>
        /// Usecase: Delay load poster image fails to be downloaded, Cached tile background is created with blank poster image.
        /// Now that tile has been created, the true poster jpg will never be downloaded again and now stuck with a blank poster
        /// image for the tile unless tile background is manually deleted.
        /// </remarks>
        [XmlIgnore] public FileCacheScope DeleteFileCacheUponExit { get; set; }
        public enum FileCacheScope { None, All, ImagesOnly }
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

                File.Delete(HtmlPath);
                var job = new FileEx.Job(UrlLink, HtmlPath);
                if (!FileEx.Download(job)) return; // FileEx.Download() logs its own errors. It will also update data.Url to redirected path and job.Filename

                //Update filenames if download http redirect (30x) ocurred. 
                var ttOld = GetTitleId(UrlLink);
                var ttNew = GetTitleId(job.Url);
                if (ttOld != ttNew)
                {
                    foreach (string fOld in Directory.EnumerateFiles(path, $"{ttOld}*.*"))
                    {
                        var fNew = fOld.Replace(ttOld, ttNew);
                        File.Delete(fNew);
                        File.Move(fOld, fNew);
                    }

                    CreateTTShortcut(ShortcutPath, ttNew);
                    UrlLink = $"https://www.imdb.com/title/{ttNew}/";
                    PathPrefix = string.Concat(path, "\\", ttNew);
                    PropertiesPath = PathPrefix + ".xml";
                    HtmlPath = PathPrefix + ".htm";
                    MoviePosterPath = PathPrefix + ".jpg";
                    job.Filename = HtmlPath;
                }

                HtmlPath = job.Filename;
                ParseImdb(job);
                File.Delete(HtmlPath); //We no longer keep the web page because the layout changes sooo frequently!

                // Rename url shortcut filename to friendly name.
                var dst = Path.Combine(Path.GetDirectoryName(this.ShortcutPath), this.FullMovieName + Path.GetExtension(this.ShortcutPath));
                if (ShortcutPath != dst) File.Move(ShortcutPath, dst);
                ShortcutPath = dst;

                this.Serialize();
                Log.Write(Severity.Info, "Added new movie \"{0}\" at {1}", this.FullMovieName, Path.GetDirectoryName(PropertiesPath));
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

        private void ParseImdb(FileEx.Job data)
        {
            MatchCollection mc;
            var html = File.ReadAllText(data.Filename);

            //Initialize to null so as to detect if these were explicitly populated but must be set to empty when these properties are read outside of the parser.
            if (Creators.IsNullOrEmpty()) Creators = null;
            if (Directors.IsNullOrEmpty()) Directors = null;
            if (Writers.IsNullOrEmpty()) Writers = null;
            if (Cast.IsNullOrEmpty()) Cast = null;
            Parser.Clear();

            // Extract JSON properties. Whatever we can't find, we scrape from web page.
            mc = Regex.Matches(html, @"<script id=""__NEXT_DATA__"" type=""application/json"">(?<JSON>.+?)</script>", RE_options);
            if (mc.Count > 0)
            {
                try
                {
                    ParseIMDBJson(mc[0].Groups["JSON"].Value);
                    if (Parser.Count > 0) Log.Write(Severity.Verbose, $"{data.Filename} JSON found {Parser.Count} {(Parser.Count == 1 ? "property" : "properties")}: {Parser.ToString()}");
                    Parser.Clear();
                }
                catch (Exception ex)
                {
                    Log.Write(Severity.Error, $"Retrieving values from JSON string embedded in \"{data.Filename}\".\n{ex}");
                    Parser.Clear();
                }
            }
            else
            {
                Log.Write(Severity.Verbose, $"JSON string not found embedded in \"{data.Filename}\".");
            }

            html = FileEx.ReadHtml(data.Filename, true); //remove all whitespace, javascript, and replace all double-quotes with single-quotes for parsing ease and speed.
            ScrapeImdbPage(html);
            if (Parser.Count > 0) Log.Write(Severity.Verbose, $"{data.Filename} Web page scrape found {Parser.Count} {(Parser.Count == 1 ? "property" : "properties")}: {Parser.ToString()}");
            Parser.Clear();

            //Restore uninitialized properties null strings back to empty for normal runtime usage.
            if (Creators == null) Creators = string.Empty;
            if (Directors == null) Directors = string.Empty;
            if (Writers == null) Writers = string.Empty;
            if (Cast == null) Cast = string.Empty;
        }

        private void ScrapeImdbPage(string html)
        {
            MatchCollection mc;

            if (MovieName.IsNullOrEmpty() || Year == 0 || MovieClass.IsNullOrEmpty())
            {
                // <title>2001: A Space Odyssey (1968) - IMDb</title>
                // <meta name='title' content='2001: A Space Odyssey (1968) - IMDb'/>
                // <meta property='og:title' content='2001: A Space Odyssey (1968)'/>
                // <meta property='og:title' content='Epoch: Evolution (TV Movie 2003)'>
                mc = Regex.Matches(html, @"(?:<title>|<meta name='title' content='|<meta property='og:title' content=')(?<TITLE>.+?)(?:<\/title>|'>|'\/>)", RE_options);
                if (mc.Count > 0)
                {
                    ParsePageTitle(mc[0].Groups["TITLE"].Value, "W");
                }
            }

            if (Plot.IsNullOrEmpty())
            {
                mc = Regex.Matches(html, @"data-testid='plot-xl'[^>]+>(?<PLOT>[^<]+)", RE_options);
                if (mc.Count > 0)
                {
                    Plot = mc[0].Groups["PLOT"].Value;
                    if (!Plot.IsNullOrEmpty() && Plot.Contains('&')) Plot = WebUtility.HtmlDecode(Plot);
                    Parser.Found(Plot, "PlotW");
                }
            }

            if (Summary.IsNullOrEmpty())
            {
                mc = Regex.Matches(html, @"storyline-plot-summary'>.+?<div>(?<SUMMARY>[^<]+)", RE_options);
                if (mc.Count > 0)
                {
                    Summary = mc[0].Groups["SUMMARY"].Value;
                    if (!Summary.IsNullOrEmpty() && Summary.Contains('&')) Summary = WebUtility.HtmlDecode(Summary);
                    Parser.Found(Summary, "SummaryW");
                }
            }

            if (Creators.IsNullOrEmpty() || Directors.IsNullOrEmpty() || Writers.IsNullOrEmpty() || Cast.IsNullOrEmpty())
            {
                // http://regexstorm.net/tester
                mc = Regex.Matches(html, @"<(?:a|span) [^>]+>(?<KEY>Creator|Director|Writer|Star)s?<\/(?:a|span)><div [^>]+><ul [^>]+>(?:(?:<li [^>]+><a [^>]+>([^<]+)<\/a>.*?<\/li>)+)", RE_options);
                if (mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        var credits = string.Join(", ", m.Groups[1].Captures.Cast<Capture>().Select(p => p.Value).ToArray());
                        if (credits.IsNullOrEmpty()) continue;
                        switch (m.Groups["KEY"].Value.ToLowerInvariant())
                        {
                            case "creator": if (!Creators.IsNullOrEmpty()) continue; Creators = credits; Parser.Found("CreatorsW"); break;
                            case "director": if (!Directors.IsNullOrEmpty()) continue; Directors = credits; Parser.Found("DirectorsW"); break;
                            case "writer": if (!Writers.IsNullOrEmpty()) continue; Writers = credits; Parser.Found("WritersW"); break;
                            case "star": if (!Cast.IsNullOrEmpty()) continue; Cast = credits; Parser.Found("CastW"); break;
                        }
                    }
                }
            }

            if (Genre.IsNullOrEmpty())
            {
                mc = Regex.Matches(html, @"<a class=.*?href='\/search\/title\/\?genres[^>]+>(?<GENRE>[^<]+)<\/a>", RE_options);
                if (mc.Count > 0)
                {
                    Genre = mc.Cast<Match>().Select(p => p.Groups["GENRE"].Value).ToArray();
                    Parser.Found(Genre, "GenreW");
                }
            }

            if (ReleaseDate == DateTime.MinValue)
            {
                mc = Regex.Matches(html, @"<a class='ipc-metadata.+?ref_=tt_dt_rdat'>(?<RDATE>[^\(<]+)", RE_options);
                if (mc.Count > 0)
                {
                    ReleaseDate = DateTime.TryParse(mc[0].Groups["RDATE"].Value, out var dt) ? dt : DateTime.MinValue;
                    Parser.Found(ReleaseDate, "ReleaseDateW");
                }
            }

            // Set this.MoviePosterUrl. Must be after ReleaseDate initialization.
            if (SetMoviePosterUrl(html)) Parser.Found("MoviePosterUrlW");

            if (EpisodeCount == 0 && MovieClass.EndsWith("Series"))
            {
                mc = Regex.Matches(html, @"<h3 class='ipc-title__text'>Episodes\s*<span class='ipc-title__subtext'>(?<EPISODECOUNT>[0-9]+)<\/span", RE_options);
                if (mc.Count > 0)
                {
                    EpisodeCount = int.TryParse(mc[0].Groups["EPISODECOUNT"].Value, out var f) ? f : 0;
                    Parser.Found(EpisodeCount, "EpisodeCountW");
                }
            }

            if ((Season == 0 || Episode == 0) && MovieClass == "TV Episode")
            {
                mc = Regex.Matches(html, @"season-episode-numbers-section'>(?:<li[^>]+><span[^>]+>(?:(S|E)([0-9]+))<\/span><\/li>){2,2}", RE_options);
                if (mc.Count > 0)
                {
                    var slabel = mc[0].Groups[1].Captures[0].Value; //=="S"
                    var elabel = mc[0].Groups[1].Captures[1].Value; //=="E"

                    if (Season == 0) Season = int.TryParse(mc[0].Groups[2].Captures[0].Value, out var f) ? f : 0;
                    if (Episode == 0) Episode = int.TryParse(mc[0].Groups[2].Captures[1].Value, out var f) ? f : 0;
                    Parser.Found(Season, "SeasonW");
                    Parser.Found(Episode, "EpisodeW");
                }
            }

            if (MovieRating == 0)
            {
                mc = Regex.Matches(html, @"rating__score[^>]+><span[^>]+>(?<RATING>[0-9\.]+)<", RE_options);
                if (mc.Count > 0)
                {
                    MovieRating = float.TryParse(mc[0].Groups["RATING"].Value, out var f) ? f : 0.0f;
                    Parser.Found(MovieRating, "MovieRatingW");
                }
            }
        }

        private void ParseIMDBJson(string json)
        {
            var found = new List<string>();
            int i;
            float f;
            var js = SimpleJSON.JSON.Parse(json);

            SimpleJSON.JSONNode props1 = null, props2 = null;
            props1 = js["props"]["pageProps"]["aboveTheFoldData"];
            props2 = js["props"]["pageProps"]["mainColumnData"];

            if (MovieClass.IsNullOrEmpty())
            {
                switch(props1["titleType"]["id"].Value) //language independent ID
                {
                    case "movie": MovieClass = "Feature Movie"; break; //Movie
                    case "short": MovieClass = "Short"; break; //Short
                    case "tvEpisode": MovieClass = "TV Episode"; break; //TV Episode
                    case "tvMiniSeries": MovieClass = "TV Mini Series"; break; //TV Mini Series
                    case "tvMovie": MovieClass = "TV Movie"; break; //TV Movie
                    case "tvSeries": MovieClass = "TV Series"; break; //TV Series
                    case "video": MovieClass = "Video"; break; //Video
                    case "tvShort": MovieClass = "TV Short"; break; //TV Short
                    case "videoGame": MovieClass = "Video Game"; break; //Video Game
                    case "tvSpecial": MovieClass = "TV Special"; break; //TV Special
                    case "podcastSeries": MovieClass = "Podcast Series"; break; //Podcast Series
                    case "podcastEpisode": MovieClass = "Podcast Episode"; break; //Podcast Episode
                    case "musicVideo": MovieClass = "Music Video"; break; //Music Video
                    default: MovieClass = props1["titleType"]["text"].Value; break;
                }

                Parser.Found(MovieClass, "MovieClassJ");
            }

            if (MovieName.IsNullOrEmpty())
            {
                MovieName = props1["series"]?["series"]?["titleText"]?["text"]?.Value;
                if (MovieName.IsNullOrEmpty())
                    MovieName = props1["titleText"]?["text"]?.Value;
                else
                    MovieName = string.Concat(MovieName, " \xAD ", props1["titleText"]?["text"]?.Value ?? "");

                Parser.Found(MovieName, "MovieNameJ");
            }

            if (Year == 0)
            {
                Year = int.TryParse(props1["releaseYear"]["year"], out i) ? i : 0;
                Parser.Found(Year, "YearJa");
            }

            if (YearEnd == 0)
            {
                YearEnd = int.TryParse(props1["releaseYear"]["endYear"], out i) ? i : 0;
                Parser.Found(YearEnd, "YearEndJ");
            }

            if (ReleaseDate == DateTime.MinValue)
            {
                var year = int.TryParse(props1["releaseDate"]["year"], out i) ? i : 0;
                var month = int.TryParse(props1["releaseDate"]["month"], out i) ? i : 0;
                var day = int.TryParse(props1["releaseDate"]["day"], out i) ? i : 1; // default to 1 as sometimes null!

                if (year != 0 && month != 0)
                {
                    ReleaseDate = new DateTime(year, month, day);
                    Parser.Found("ReleaseDateJ");
                }
            }

            if (Year == 0 && ReleaseDate != DateTime.MinValue)
            {
                Year = ReleaseDate.Year;
                Parser.Found("YearJb");
            }

            if (YearEnd == 0)
            {
                YearEnd = int.TryParse(props1["releaseYear"]["endYear"], out i) ? i : 0;
                Parser.Found(YearEnd, "YearEndJ");
            }

            if (Runtime==0 && (MoviePath.IsNullOrEmpty() || !File.Exists(this.MoviePath)))
            {
                //This is normally set from async extract of video file properties: GetVideoFileProperties()
                Runtime = int.TryParse(props1["runtime"]["seconds"], out i) ? i / 60 : 0;
                if (Runtime == 0) Runtime = int.TryParse(props2["runtime"]["seconds"], out i) ? i / 60 : 0;
                Parser.Found(Runtime, "RuntimeJ");
            }

            if (MovieRating == 0)
            {
                MovieRating = float.TryParse(props1["ratingsSummary"]["aggregateRating"], out f) ? f : 0;
                Parser.Found(MovieRating, "MovieRatingJ");
            }

            if (MoviePosterUrl.IsNullOrEmpty())
            {
                MoviePosterUrl = props1["primaryImage"]["url"]?.Value ?? string.Empty;
                Parser.Found(MoviePosterUrl, "MoviePosterUrlJ");
            }

            if (Genre.IsNullOrEmpty())
            {
                Genre = props1["genres"]["genres"].Linq.Select(p => p.Value["text"].Value).ToArray();
                Parser.Found(Genre, "GenreJ");
            }

            if (Plot.IsNullOrEmpty())
            {
                Plot = props1["plot"]?["plotText"]?["plainText"]?.Value;
                if (Plot.IsNullOrEmpty()) Plot = props2["plot"]?["plotText"]?["plainText"]?.Value;
                if (!Plot.IsNullOrEmpty() && Plot.Contains('&')) Plot = WebUtility.HtmlDecode(Plot);
                Parser.Found(Plot, "PlotJ");
            }

            if (Summary.IsNullOrEmpty())
            {
                Summary = props2["summaries"]?["edges"]?[0]?["node"]?["plotText"]?["plaidHtml"]?.Value;
                if (Summary.IsNullOrEmpty()) Summary = props2["outlines"]?["edges"]?[0]?["node"]?["plotText"]?["plaidHtml"]?.Value;
                if (!Summary.IsNullOrEmpty() && Summary.Contains('&')) Summary = WebUtility.HtmlDecode(Summary);
                Parser.Found(Summary, "SummaryJ");
            }

            // Directors/Writers/Cast/Creators
            var sb = new StringBuilder();
            foreach (var pc in props1["principalCredits"])
            {
                var id = pc.Value["category"]["id"].Value;  //director, writer, cast, creator
                string comma = string.Empty;
                foreach (var cr in pc.Value["credits"])
                {
                    sb.Append(comma);
                    sb.Append(cr.Value["name"]["nameText"]["text"].Value);
                    comma = ", ";
                }
                var value = sb.ToString();
                sb.Length = 0;
                if (value.IsNullOrEmpty()) continue;
                switch (id)
                {
                    case "director": Directors = value; Parser.Found("DirectorsJ"); break;
                    case "writer": Writers = value; Parser.Found("WritersJ"); break;
                    case "cast": Cast = value; Parser.Found("CastJ"); break;
                    case "creator": Creators = value; Parser.Found("CreatorsJ"); break;
                }
            }

            if (Episode == 0 && MovieClass == "TV Episode")
            {
                Episode = int.TryParse(props1["series"]?["episodeNumber"]?["episodeNumber"], out i) ? i : 0;
                if (Episode == 0) Episode = int.TryParse(props2["series"]?["episodeNumber"]?["episodeNumber"], out i) ? i : 0;
                Parser.Found(Episode, "EpisodeJ");
            }
            if (Season == 0 && MovieClass == "TV Episode")
            {
                Season = int.TryParse(props1["series"]?["episodeNumber"]?["seasonNumber"], out i) ? i : 0;
                if (Season == 0) Season = int.TryParse(props2["series"]?["episodeNumber"]?["seasonNumber"], out i) ? i : 0;
                Parser.Found(Season, "SeasonJ");
            }

            if (EpisodeCount == 0 && MovieClass.EndsWith("Series"))
            {
                EpisodeCount = int.TryParse(props1["episodes"]?["episodes"]?["total"], out i) ? i : 0;
                if (EpisodeCount == 0) EpisodeCount = int.TryParse(props2["episodes"]?["episodes"]?["total"], out i) ? i : 0;
                Parser.Found(EpisodeCount, "EpisodeCountJ");
            }
        }

        private void ParsePageTitle(string title, string sourceId="")
        {
            if (title.IsNullOrEmpty()) return;
            if (!MovieName.IsNullOrEmpty() && Year != 0 && !MovieClass.IsNullOrEmpty()) return;

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
            title = WebUtility.HtmlDecode(title);
            var mc = Regex.Matches(title, @"^(?:(?:""(?<TITLE>[^""]+)"" (?<EPISODE>.+) (?=\([^\(]+$))|(?<TITLE2>.+))\((?:(?<TYPE>.+?) )?(?<YEAR>[0-9]{4,4})[–-]?(?<YEAREND>[0-9]{4,4})?", RE_options);
            if (mc.Count > 0)
            {
                var titlex = mc[0].Groups["TITLE"].Value.Trim();
                if (titlex.IsNullOrEmpty()) titlex = mc[0].Groups["TITLE2"].Value.Trim();
                var episodeTitle = mc[0].Groups["EPISODE"].Value.Trim();
                if (MovieName.IsNullOrEmpty())
                {
                    MovieName = episodeTitle.Length > 1 ? string.Concat(titlex, " \xAD ", episodeTitle) : titlex;
                    Parser.Found("MovieName" + sourceId);
                }

                var year = mc[0].Groups["YEAR"].Value;
                if (Year == 0 && !year.IsNullOrEmpty())
                {
                    Year = int.Parse(year);
                    Parser.Found("Year" + sourceId);
                }

                var yearend = mc[0].Groups["YEAREND"].Value;
                if (YearEnd == 0 && !yearend.IsNullOrEmpty())
                {
                    YearEnd = int.Parse(yearend);
                    Parser.Found("YearEnd" + sourceId);
                }

                var type = mc[0].Groups["TYPE"].Value;
                if (MovieClass.IsNullOrEmpty())
                {
                    MovieClass = type.IsNullOrEmpty() ? "Feature Movie" : type;
                    Parser.Found("MovieClass" + sourceId);
                }
            }
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
            MatchCollection mc;

            if (html.IsNullOrEmpty())  //Occurs when this method is lazily called by MoviePosterImg getter so we must load our cached copy of the IMDB movie page.
            {
                if (!File.Exists(HtmlPath)) //Oops. The cached IMDB movie page does not exist. Retrieve it.
                {
                    var job = new FileEx.Job(UrlLink, HtmlPath);
                    if (!FileEx.Download(job))
                    {
                        this.DeleteFileCacheUponExit = FileCacheScope.ImagesOnly;
                        return false; // FileEx.Download() logs its own errors. It will also update data.Url to redirected path and job.Filename
                    }
                    HtmlPath = job.Filename;
                }

                html = FileEx.ReadHtml(HtmlPath);  //no duplicate whitespace, no whitespace before '<' and no whitespace after '>'
            }

            //// <div class='poster'><a href='/title/tt0401729/mediaviewer/rm3022336?ref_=tt_ov_i'><img alt='John Carter Poster' title='John Carter Poster' src='https://m.media-amazon.com/images/M/MV5BMDEwZmIzNjYtNjUwNS00MzgzLWJiOGYtZWMxZGQ5NDcxZjUwXkEyXkFqcGdeQXVyNTIzOTk5ODM@._V1_UY268_CR6,0,182,268_AL_.jpg'></a></div>
            mc = Regex.Matches(html, @"<div class='poster'><a href='(?<URL>\/title\/[^\/]+\/mediaviewer\/(?<ID>[0-9a-z]+)[^']+).+? src='(?<POSTER>[^']+)'", RE_options);
            if (mc.Count > 0)
            {
                var posterUrl = mc[0].Groups["POSTER"].Value; //get the small poster image in the page, but we continue to look for a larger/better image.
                var id = mc[0].Groups["ID"].Value;
                var mediaViewerUrl = FileEx.GetAbsoluteUrl(this.UrlLink, mc[0].Groups["URL"].Value);

                var fn = Path.Combine(Path.GetDirectoryName(this.MoviePosterPath), "MediaViewer.htm"); //temporary uncached web page containing large poster images.
                var job = new FileEx.Job(mediaViewerUrl, fn);
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
            mc = Regex.Matches(html, @"<meta property='(?:og|twitter):image' content='(?<URL>https:[^']+\.jpg)'", RE_options);
            if (mc.Count > 0)
            {
                this.MoviePosterUrl = mc[0].Groups["URL"].Value;
                return true;
            }

            //If it doesn't exist, get first image from poster carousel.
            // Url always contains 'mediaviewer' with property '?ref_=tt_ov_i'
            mc = Regex.Matches(html, @"href='(?<URL>\/title\/tt[^\/]+\/mediaviewer\/rm[^\/]+\/\?ref_=tt_ov_i)'", RE_options);
            if (mc.Count > 0)
            {
                var mediaViewerUrl = FileEx.GetAbsoluteUrl(this.UrlLink, mc[0].Groups["URL"].Value);

                var fn = Path.Combine(Path.GetDirectoryName(this.MoviePosterPath), "MediaViewer.htm"); //temporary uncached web page containing large poster images.
                var job = new FileEx.Job(mediaViewerUrl, fn);
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

            // The source IMDB page may not be complete because IMDB does not have the poster jpg's yet.
            // It also presumes that the other properties are not up to date either. Maybe the next time VideoLibrarian is run.
            // But give up on movies/episodes that have not added a poster image after a month. Likely an image will never be added...
            // Also if the poster jpg has been manually created by other means don't automatically delete the filecache.
            if (ReleaseDate.AddMonths(1) > DateTime.Now && !File.Exists(this.MoviePosterPath))
                this.DeleteFileCacheUponExit = FileCacheScope.All;
            return false; //no change. couldn't find poster url.
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
            public Type PropType { get; private set; }
            public Func<object, object> Get { get; private set; }
            public Action<object, object> Set { get; private set; }
            public PropertyList(string name, Type t, Func<object, object> get, Action<object, object> set)
            {

                Name = name;
                PropType = t;
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
                list.Add(new PropertyList(pi.Name, pi.PropertyType, pi.GetValue, pi.SetValue));
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

        #region Object Overrides
        public override string ToString() => this.FullMovieName;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                foreach (var p in PropertySetters)
                {
                    if (p.Name == "Genre")
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

            ////DEBUGGING -- These are all the NOT XmlIgnore'd properties.
            // var _UrlLink = p1.UrlLink == p2.UrlLink;
            // var _MoviePosterUrl = p1.MoviePosterUrl == p2.MoviePosterUrl;
            // var _MovieName = p1.MovieName == p2.MovieName;
            // var _Genre = p1.Genre.OrderBy(m => m).SequenceEqual(p2.Genre.OrderBy(m => m));   //<==not sip2le cop2arison
            // var _MovieClass = p1.MovieClass == p2.MovieClass;
            // var _Year = p1.Year == p2.Year;
            // var _ReleaseDate = p1.ReleaseDate.Date == p2.ReleaseDate.Date;
            // var _MovieRating = (int)(p1.MovieRating * 10) == (int)(p2.MovieRating * 10);
            // var _Plot = p1.Plot == p2.Plot;
            // var _Summary = p1.Summary == p2.Summary;
            // var _Creators = p1.Creators == p2.Creators;
            // var _Directors = p1.Directors == p2.Directors;
            // var _Writers = p1.Writers == p2.Writers;
            // var _Cast = p1.Cast == p2.Cast;
            // var _YearEnd = p1.YearEnd == p2.YearEnd;
            // var _Season = p1.Season == p2.Season;
            // var _Episode = p1.Episode == p2.Episode;
            // var _EpisodeCount = p1.EpisodeCount == p2.EpisodeCount;
            // var _DownloadDate = p1.DownloadDate.Date == p2.DownloadDate.Date;
            // var _Runtime = p1.Runtime == p2.Runtime;
            // var _DisplayRatio = p1.DisplayRatio == p2.DisplayRatio;
            // var _DisplayWidth = p1.DisplayWidth == p2.DisplayWidth;
            // var _DisplayHeight = p1.DisplayHeight == p2.DisplayHeight;
            // var _MovieHash = p1.MovieHash == p2.MovieHash;
            // var _MovieFileLength = p1.MovieFileLength == p2.MovieFileLength;
            // var _Watched = p1.Watched.Date == p2.Watched.Date;
            // 
            // var total = _UrlLink &
            //             _MoviePosterUrl &
            //             _MovieName &
            //             _Genre &
            //             _MovieClass &
            //             _Year &
            //             _ReleaseDate &
            //             _MovieRating &
            //             _Plot &
            //             _Summary &
            //             _Creators &
            //             _Directors &
            //             _Writers &
            //             _Cast &
            //             _YearEnd &
            //             _Season &
            //             _Episode &
            //             _EpisodeCount &
            //             _DownloadDate &
            //             _Runtime &
            //             _DisplayRatio &
            //             _DisplayWidth &
            //             _DisplayHeight &
            //             _MovieHash &
            //             _MovieFileLength &
            //             _Watched;
            // return total;

            return PropertySetters.All((p) => p.Name == "Genre" ? this.Genre.OrderBy(m=>m).SequenceEqual(mp.Genre.OrderBy(m => m)) : p.Get(this).Equals(p.Get(mp)));
        }
        #endregion

        #region Used Externally
        /// <summary>
        /// Populate the video properties of the movie.
        /// </summary>
        public void GetVideoFileProperties()
        {
            if (!MoviePath.IsNullOrEmpty() && File.Exists(this.MoviePath))
            {
                try
                {
                    var hashtask = Task.Run(() => { this.MovieHash = FileEx.GetHash(this.MoviePath); });

                    this.DownloadDate = FileEx.GetCreationDate(this.MoviePath);
                    var info = MediaInfo.GetInfo(this.MoviePath);
                    if (info == null) throw new NullReferenceException("Media info is null.");
                    this.Runtime = (int)Math.Ceiling(info.Duration.TotalMinutes);
                    var stream = info.Streams.FirstOrDefault(p => p.CodecType.EqualsI("video"));
                    if (stream == null) throw new NullReferenceException("Media info video stream not found.");
                    this.DisplayWidth = stream.Width;
                    this.DisplayHeight = stream.Height;
                    this.DisplayRatio = ComputeNormalizedDisplayRatio(stream.Width, stream.Height);
                    this.MovieFileLength = new FileInfo(this.MoviePath).Length;

                    hashtask.Wait();
                    hashtask.Dispose();
                }
                catch (Exception ex)
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
                foreach (var f in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories).OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
                {
                    if (!IsVideoFile(f)) continue;
                    path = f;
                    break;
                }
                this.DownloadDate = FileEx.GetCreationDate(path);
            }
        }

        /// <summary>
        /// Validate/verify integrity of movie file.
        /// Used by VideoValidator.exe
        /// </summary>
        /// <returns>True if valid and not corrupted.</returns>
        public bool VerifyVideoFile()
        {
            if (this.MoviePath.IsNullOrEmpty())
            {
                if (this.MovieHash != Guid.Empty)
                {
                    //this.DownloadDate - We do not initialize DownloadDate because root TVSeries folder takes on the file date of the first video in the series.
                    this.Runtime = 0;
                    this.DisplayRatio = "0:0";
                    this.DisplayWidth = 0;
                    this.DisplayHeight = 0;
                    this.MovieHash = Guid.Empty;
                    this.MovieFileLength = 0;
                    this.Serialize();
                    Log.Write(Severity.Warning, $"Movie file name missing in folder {Path.GetDirectoryName(this.PropertiesPath)} but hash exists, but now corrected.");
                    return false;
                }
                if (this.MovieFileLength != 0)
                {
                    this.Runtime = 0;
                    this.DisplayRatio = "0:0";
                    this.DisplayWidth = 0;
                    this.DisplayHeight = 0;
                    this.MovieHash = Guid.Empty;
                    this.MovieFileLength = 0;
                    this.Serialize();
                    Log.Write(Severity.Warning, $"Movie file name missing in folder {Path.GetDirectoryName(this.PropertiesPath)} but file length exists, but now corrected.");
                    return false;
                }
                return true;
            }

            if (!File.Exists(this.MoviePath)) //Should never get here (see FindFiles()).
            {
                Log.Write(Severity.Error, $"Verification of {this.MoviePath} Failed: File not found.");
                return false;
            }

            //Video file exists, now validate it...

            if (this.MovieFileLength == 0)
            {
                this.GetVideoFileProperties();
                this.Serialize();
                Log.Write(Severity.Warning, $"{this.MoviePath} not verified: MovieFileLength undefined, but now corrected.");
                return false;
            }

            if (this.MovieHash == Guid.Empty)
            {
                this.GetVideoFileProperties();
                this.Serialize();
                Log.Write(Severity.Warning, $"{this.MoviePath} not verified: MovieHash undefined, but now corrected.");
                return false;
            }

            if (this.MovieFileLength != new FileInfo(this.MoviePath).Length)
            {
                Log.Write(Severity.Error, $"Verification of {this.MoviePath} Failed: File length mismatch. Video Corrupted.");
                return false;
            }

            if (this.MovieHash != FileEx.GetHash(this.MoviePath))
            {
                Log.Write(Severity.Error, $"Verification of {this.MoviePath} Failed: File hash mismatch. Video Corrupted.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create IMDB shortcut with logo icon
        /// </summary>
        /// <param name="filepath">Name of shortcut (*.url)</param>
        /// <param name="tt">IMDB title id (tt123456) or EmptyTitleID (tt0000000 e.g. non-IMDB movie) or full url (ɦttps://www.imdb.com/title/tt123456/)</param>
        public static void CreateTTShortcut(string filepath, string tt)
        {
            if (File.Exists(filepath)) return;
            //http://www.lyberty.com/encyc/articles/tech/dot_url_format_-_an_unofficial_guide.html

            //One cannot use the website favicon for the shortcut icon directly from the website url. It must be downloaded to a local file before it can be used!
            //https://docs.microsoft.com/en-us/answers/questions/120626/internet-shortcut-url-file-no-longer-supports-remo.html
            var favicon = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures), "favicon_imdb.ico");
            if (!File.Exists(favicon))
            {
                var job = new FileEx.Job("https://www.imdb.com/favicon.ico", favicon);
                FileEx.Download(job);
                if (!favicon.Equals(job.Filename)) File.Move(job.Filename, favicon);
            }

            //Delete all other IMDB shortcuts
            foreach (var f in Directory.EnumerateFiles(Path.GetDirectoryName(filepath), "*.url", SearchOption.TopDirectoryOnly))
            {
                var link = MovieProperties.GetUrlFromShortcut(f);
                if (link == null) continue;
                if (!link.ContainsI("imdb.com/title/tt") && !link.StartsWith("file:///")) continue;
                File.Delete(f);
            }

            if (tt == MovieProperties.EmptyTitleID)
            {
                File.WriteAllText(filepath, $"[InternetShortcut]\r\nURL={new Uri(Path.GetDirectoryName(filepath))}\r\nIconIndex=129\r\nIconFile=C:\\Windows\\System32\\SHELL32.dll\r\nAuthor=VideoLibrarian.exe");
            }
            else
            {
                if (tt.StartsWith("https://"))
                    File.WriteAllText(filepath, $"[InternetShortcut]\r\nURL={tt} \r\nIconFile={favicon}\r\nIconIndex=0\r\nHotKey=0\r\nIDList=\r\nAuthor=VideoLibrarian.exe");
                else
                    File.WriteAllText(filepath, $"[InternetShortcut]\r\nURL=https://www.imdb.com/title/{tt}/ \r\nIconFile={favicon}\r\nIconIndex=0\r\nHotKey=0\r\nIDList=\r\nAuthor=VideoLibrarian.exe");
            }
            Log.Write(Severity.Info, $"Created shortcut {Path.GetFileNameWithoutExtension(filepath)} ==> https://www.imdb.com/title/{tt}/");
        }

        /// <summary>
        /// Extract URL from IMDB shortcut
        /// </summary>
        /// <param name="path">Name of shortcut (*.url) to read</param>
        /// <returns>URL within shortcut</returns>
        public static string GetUrlFromShortcut(string path)
        {
            using (var sr = new StreamReader(path))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("URL="))
                    {
                        var url = line.Substring(4).Trim();
                        //Remove urlencoded properties (e.g. ?abc=def&ccc=ddd)
                        var i = url.IndexOf('?');
                        if (i != -1) url = url.Substring(0, i);
                        return url;
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Determine if file is a video file.
        /// </summary>
        /// <param name="file">UNC filename or URL to check.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Determine if file is an image file.
        /// </summary>
        /// <param name="file">UNC filename or URL to check.</param>
        /// <returns></returns>
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
        /// "fіle:///C:/Users/User/Videos" ==> "tt0000000" (aka MovieProperties.EmptyTitleID)
        /// NOTE: EmptyTitleID refers to video not found on IMDB and refers to a manually
        /// generated MovieProperties file (e.g. tt0000000.xml) The actual content
        /// (filė://c:/abc/def/) of the shortcut url has no meaning. It is only a placeholder.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>IMDB title id (e.g. tt0062622) or EmptyTitleID (e.g. tt0000000) or zero-length string if not found</returns>
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

        /// <summary>
        /// Check if folder or file should be ignored.
        /// An ignored folder contains a directory that is bracketed with matching bracket chars.
        /// Directories containing mismatching bracket chars are NOT ignored.
        /// </summary>
        /// <param name="folder">full folder name to check.</param>
        /// <returns>true if folder should be ignored</returns>
        public static bool IgnoreFolder(string folder) => reIgnoredFolder.IsMatch(folder + "\\");
        private const string bracketPattern = @"\\(
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
        private static readonly Regex reIgnoredFolder = new Regex(bracketPattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        #endregion

        /// <summary>
        /// For verbose debugging. Save names of properties that have been assigned a value.
        /// See DebugLog for alternate debug logging.
        /// </summary>
        // [Conditional("DEBUG")]
        private static class Parser
        {
            private static List<string> found = new List<string>();

            public static int Count => found.Count;

            [Conditional("DEBUG")]
            public static void Clear() => found.Clear();

            [Conditional("DEBUG")]
            public static void Found(List<string> value) { if (Log.SeverityFilter != Severity.Verbose) return; found.AddRange(value); }
            [Conditional("DEBUG")]
            public static void Found(string name) { if (Log.SeverityFilter != Severity.Verbose) return; found.Add(name); }

            [Conditional("DEBUG")]
            public static void Found(ICollection value, string name) { if (Log.SeverityFilter != Severity.Verbose) return;  if (!value.IsNullOrEmpty()) found.Add(name); }
            [Conditional("DEBUG")]
            public static void Found(DateTime value, string name) { if (Log.SeverityFilter != Severity.Verbose) return; if (value != DateTime.MinValue) found.Add(name); }
            [Conditional("DEBUG")]
            public static void Found(string value, string name) { if (Log.SeverityFilter != Severity.Verbose) return; if (!string.IsNullOrEmpty(value)) found.Add(name); }
            [Conditional("DEBUG")]
            public static void Found(int value, string name) { if (Log.SeverityFilter != Severity.Verbose) return; if (value != 0) found.Add(name); }
            [Conditional("DEBUG")]
            public static void Found(float value, string name) { if (Log.SeverityFilter != Severity.Verbose) return; if (value != 0) found.Add(name); }
            [Conditional("DEBUG")]
            public static void Found(double value, string name) { if (Log.SeverityFilter != Severity.Verbose) return; if (value != 0) found.Add(name); }
            [Conditional("DEBUG")]
            public static void Found(bool value, string name) { if (Log.SeverityFilter != Severity.Verbose) return; if (value) found.Add(name); }

            public static new string ToString()
            {
                if (found.Count == 0) return string.Empty;
                return string.Join(", ", found);
            }
        }
    }
}
