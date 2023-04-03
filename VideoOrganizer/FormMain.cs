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
//#define OfflineDebugging  //enable saving/restoring TVSeries dictionary cache in debug mode.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using VideoLibrarian;

//-------------------------------------------------------------
// Use https://regex101.com to validate the regex patterns. 
//-------------------------------------------------------------

namespace VideoOrganizer
{
    public partial class FormMain : Form
    {
        private ToolTipHelp tt; //Tooltip help manager
        private string RootFolder; //Mainly used for stripping full filenames to make relative path for status msgs.

        //FindImdbUrl() Cached Movie and TVSeries TT codes so we don't have to query the IMDB web page for every episode.
        private readonly Dictionary<string, string> TVSeries;

        private readonly Font RegularFont;  //for SetStatus()
        private readonly Font BoldFont;

        public FormMain()
        {
            Log.MessageCapture += SetStatus;  //copy any log messages to to status window, particularly low-level logging errors from internet downloads.
            Log.SeverityFilter = Severity.Verbose;

            InitializeComponent();

            tt = new ToolTipHelp(this); //must be after InitializeComponent()

            //Optionally get root folder from command-line
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && Directory.Exists(args[1]))
            {
                var f = args[1];
                if (f[f.Length - 1] == '\\') f = f.Substring(0, f.Length - 1);
                m_txtRoot.Text = Path.GetFullPath(f);
            }

            m_txtRoot.Select(0, 0); //deselect root folder

            RegularFont = m_rtfStatus.Font; //for SetStatus() 
            BoldFont = new Font(m_rtfStatus.Font, FontStyle.Bold);

            TVSeries = DebugImportCache(); //If offline debugging enabled, will also overwrite m_txtRoot.Text
        }

        private void m_btnAbout_Click(object sender, EventArgs e)
        {
            AboutBox.Show(this);
        }

        private void m_btnManualConfig_Click(object sender, EventArgs e)
        {
            ManualVideoConfig.Show(this, m_txtRoot.Text);
        }

        private void m_btnManualConfig_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (Directory.Exists(file) || MovieProperties.IsVideoFile(file) || MovieProperties.IsPropertiesFile(file))
            {
                e.Effect = DragDropEffects.Link;
            }
        }

        private void m_btnManualConfig_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

            if (Directory.Exists(file) || MovieProperties.IsVideoFile(file) || MovieProperties.IsPropertiesFile(file))
            {
                ManualVideoConfig.Show(this, m_txtRoot.Text, file);
                return;
            }
        }

        private void m_btnGo_Click(object sender, EventArgs e)
        {
            if (m_txtRoot.Text == string.Empty)
            {
                MiniMessageBox.ShowDialog(this, "Root folder has not yet been set.", m_txtRoot.TextLabel, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(m_txtRoot.Text))
            {
                MiniMessageBox.ShowDialog(this, "Root folder does not exist", m_txtRoot.TextLabel, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            m_btnGo.Enabled = false;             //don't allow user to click "Go" again.
            RootFolder = m_txtRoot.Text;         //save for DoWork() and all its child methods.
            var task = Task.Run(() => DoWork()); //Do work asynchronously.
            //task.Wait(); task.Dispose(); //don't wait for completion. A success message will be displayed in the status window.
            return;
        }

        private void m_btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void m_btnSelectRoot_Click(object sender, EventArgs e)
        {
            var initialDirectory = m_txtRoot.Text=="" || !Directory.Exists(m_txtRoot.Text) ? Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) : m_txtRoot.Text;
            var dir = FolderSelectDialog.Show(this, "Select Movie/Media Folder", initialDirectory);
            if (dir == null) return;
            m_txtRoot.Text = dir;
        }

        private void m_txtRoot_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var folder = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (Directory.Exists(folder))
                {
                    m_txtRoot.Text = folder;
                }
            }
        }

        private void m_txtRoot_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && Directory.Exists(((string[])e.Data.GetData(DataFormats.FileDrop))[0]))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void m_rtfStatus_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        // We don't actually perform any file logging as everything goes to the status window.
        private void SetStatus(Severity severity, string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action<Severity, string>)((s,m) => SetStatus(s, m)), severity, msg);
                return;
            }

            Color clr;
            switch(severity)
            {
                case Severity.Success: clr = Color.ForestGreen;  break;
                case Severity.Error:   clr = Color.Red;    break;
                case Severity.Warning: clr = Color.Gold;   break;
                case Severity.Info:    clr = Color.MediumBlue;   break;
                case Severity.Verbose: clr = Color.LightBlue; break;
                default:               clr = Color.Black;  break;
            }

            if (severity != Severity.None)
            {
                int start = m_rtfStatus.Text.Length;
                m_rtfStatus.AppendText(severity.ToString() + ": ");
                int end = m_rtfStatus.Text.Length;

                m_rtfStatus.Select(start, end - start);
                m_rtfStatus.SelectionColor = clr;
                m_rtfStatus.SelectionFont = BoldFont;

                m_rtfStatus.Select(end, 0);
                m_rtfStatus.SelectionColor = Color.Black;
                m_rtfStatus.SelectionFont = RegularFont;
            }

            msg = msg.Indent();

            m_rtfStatus.AppendText(msg);
            m_rtfStatus.AppendText("\n");
            m_rtfStatus.ScrollToCaret();
        }

        private void DoWork()
        {
            SetStatus(Severity.Success, "Started\n");

            try
            {
                //folders may have been renamed, so we have to adjust for that. The UnprocessedMovieList is in sorted order so saving the previous folder is good enough.
                var previousFolder = RootFolder;
                var prevNewFolder = RootFolder;

                foreach (var f in UnprocessedMovieList(RootFolder))
                {
                    var imdbParts = FindImdbUrl(f);
                    if (imdbParts == null) continue;

                    SetStatus(Severity.Info, $"Moving \"{f.Substring(RootFolder.Length)}\" => \"\\{imdbParts.FolderName}{(imdbParts.Series == null ? "" : "\\" + imdbParts.Series)}\\\"");

                    var oldPath = f;  //Need to determine if underlying folder was renamed on a previous loop.
                    if (!FileEx.Exists(oldPath)) oldPath = Path.Combine(prevNewFolder, Path.GetFileName(oldPath));
                    if (!FileEx.Exists(oldPath)) { SetStatus(Severity.Error, $"Video {oldPath.Substring(RootFolder.Length)} not found."); continue; }

                    var folder = Path.GetDirectoryName(oldPath);

                    var i = folder.IndexOf("\\" + imdbParts.FolderName);
                    var newFolder = Path.Combine(i == -1 ? (folder==RootFolder ? folder : Path.GetDirectoryName(folder)) : folder.Substring(0, i), imdbParts.FolderName);
                    var newPath = Path.Combine(newFolder, Path.GetFileName(oldPath));

                    if (folder == previousFolder || folder == prevNewFolder || folder == RootFolder) { CreateFolder(newFolder); MoveFile(oldPath, newPath); }
                    else { MoveFolder(folder, newFolder);  }
                    if (!FileEx.Exists(oldPath)) oldPath = Path.Combine(newFolder, Path.GetFileName(oldPath));
                    MovieProperties.CreateTTShortcut(Path.Combine(newFolder, imdbParts.MovieName + ".url"), imdbParts.ttMovie);

                    if (imdbParts.Series != null) //TV Series Episode
                    {
                        var newSeriesFolder = Path.Combine(newFolder, imdbParts.Series);
                        newPath = Path.Combine(newSeriesFolder, Path.GetFileName(oldPath));
                        CreateFolder(newSeriesFolder);
                        MoveFile(oldPath, newPath);
                        MovieProperties.CreateTTShortcut(Path.Combine(newSeriesFolder, $"{imdbParts.MovieName}.{imdbParts.Series}.url"), imdbParts.ttSeries);
                    }

                    previousFolder = folder;
                    prevNewFolder = newFolder;
                }
            }
            catch (Exception ex)
            {
                SetStatus(Severity.Error, $"Fatal: {ex}");
                return;
            }

            DebugExportCache();
            SetStatus(Severity.None, string.Empty);
            SetStatus(Severity.Success, "Completed.");
        }

        private static IEnumerable<string> UnprocessedMovieList(string rootFolder)
        {
            //We must have a realized list because we may be moving folders and files around causing unrealized enumerations to break.
            var list = new List<string>();

            //Enumerate all videos. Ignoring all videos with matching shortcuts.

            foreach (var f in DirectoryEx.EnumerateAllFiles(rootFolder, SearchOption.AllDirectories))
            {
                if (!MovieProperties.IsVideoFile(f)) continue;
                var folder = Path.GetDirectoryName(f);

                //Special: if video is in a bracketed folder (or any of its child folders) the video is ignored. 
                if (MovieProperties.IgnoreFolder(folder)) continue;

                //A video in the root folder is always unprocessed even if there are other shortcuts in
                //the root folder, because we will be moving the video into its own folder anyway. 
                if (folder == rootFolder) { list.Add(f); continue; }

                if (HasMatchingShortcut(folder)) continue;

                list.Add(f);
            }

            return list.OrderBy(m=>m);
        }

        private static bool HasMatchingShortcut(string folder)
        {
            var hasMatchingShortcut = false;
            foreach (var f2 in Directory.EnumerateFiles(folder, "*.url", SearchOption.TopDirectoryOnly))
            {
                var link = MovieProperties.GetUrlFromShortcut(f2);
                if (link == null) continue;
                if (!link.ContainsI("imdb.com/title/tt")) continue;
                hasMatchingShortcut = true;

                //Ignore matching shortcut rule under these conditions...

                //It's OK if there is a video in a TVSeries root. folder It's a TVSeries root folder if it contains S01E01 folders

                var isTVSeriesRootFolder = Directory.EnumerateDirectories(folder).Any((f)=> RegexCache.RegEx(@"\\S[0-9]{2,2}E[0-9]{2,2}\\?$", RegexOptions.IgnoreCase).IsMatch(f));
                if (isTVSeriesRootFolder) { hasMatchingShortcut = false; break; }

                //It's OK if there is a shortcut in a folder full of videos.
                var kount = Directory.EnumerateFiles(folder).Count((f) => MovieProperties.IsVideoFile(f));
                if (kount > 1) { hasMatchingShortcut = false; break; } //must be 2 or more

                break; //if we get this far, we're done searching for shortcuts.
            }

            return hasMatchingShortcut;
        }

        private void CreateFolder(string dst)
        {
            if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);
        }
        private void MoveFolder(string src, string dst)
        {
            if (Directory.Exists(src) && !Directory.Exists(dst)) Directory.Move(src, dst);
        }
        private void MoveFile(string src, string dst)
        {
            if (src == dst) return;
            if (!FileEx.Exists(src)) { SetStatus(Severity.Error, $"Source File {src.Substring(RootFolder.Length)} does not exist."); return; }
            if (!FileEx.Exists(dst)) FileEx.Move(src, dst);
            else { SetStatus(Severity.Error, $"Source file {src.Substring(RootFolder.Length)} already exists in {Path.GetDirectoryName(dst).Substring(RootFolder.Length)}."); return; }

            //Verify that destination folder contains only one movie
            if (Directory.EnumerateFiles(Path.GetDirectoryName(dst)).Count((f)=> MovieProperties.IsVideoFile(f)) > 1)
                SetStatus(Severity.Warning, $"Multiple movies in destination folder {Path.GetDirectoryName(dst).Substring(RootFolder.Length)}. There can only be one movie in a folder.");

            //Delete empty folders
            var folder = Path.GetDirectoryName(src);
            if (!Directory.Exists(folder) ||
                Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Any((s) => !s.EndsWith("desktop.ini")))
                return;
            Directory.Delete(folder, true);
        }

        /// <summary>
        /// Find IMDB url from movie file name. Only works for well-formed file names.
        /// </summary>
        /// <param name="movieFileName">Full filename of video file.</param>
        /// <returns>IMDB url or null if not found</returns>
        public ImdbParts FindImdbUrl(string movieFileFullPath)
        {
            var imdbParts = new ImdbParts();

            MatchCollection mc;
            var movieFileName = Path.GetFileName(movieFileFullPath);
            var tempFileName = Path.Combine(Path.GetTempPath(), "FindUrl.htm");

            // Find TV Series episode url. Contains TV series name + (maybe) year + season and episode
            // Possible Filename Permutations:
            //  Ascension S01E01 Chapter 1 (2014) 720p.mp4
            //  BrainDead.S01E01.1080p.AMZN.WEB-DL.DD5.1.H.264-SiGMA.mkv
            //  Eureka (2006) - S05E02 - The Real Thing (1080p BluRay x265 Panda).mkv
            //  The.Expanse.2017.S02E03.1080p.H.265.mkv
            //  The Expanse (2018) S03E07 1008p.mp4
            //  The.Flash.2014.S01E01.HDTV.x264-LOL.mp4
            //  For All Mankind (2019) S01E01 720p.ATVP.WEB-DL.DDP5.1.H.265-TOMMY.mkv
            //  Fringe (2008) - S01E04 - The Arrival (1080p BluRay x265 Silence).mkv
            //  Lexx S03E07.mkv
            //  **PATTERNRESET0 - needed when testing on regex101.com
            //  The Man In The High Castle S01E01.mp4
            //  Threshold.1x01.720p.HDTV.H.265-aljasPOD.mkv
            //  01 Utopia - Episode 1.1 Mystery 2013 Eng Subs 720p [H264-mp4].mp4
            //  50.States.Of.Fright.S01E01.720p.QUIBI.WEBRip.x264-GalaxyTV.mkv
            const string patternEpisodes = @"
                ^(?:[0-9]{1,2}[ -]+)?
                (?<NAME>.+?)
                (?:[ \.\(]*(?<YEAR1>[0-9]{4,4})[ \.\)-]*)?
                (?:
                (?:S(?<S1>[0-9]{1,2})E(?<E1>[0-9]{1,2}))|
                (?:(?<S2>[0-9]{1,2})x(?<E2>[0-9]{1,2}))|
                (?:[ \.-]+Episode[ \.](?<S3>[0-9]{1,2})\.(?<E3>[0-9]{1,2}))
                )
                [^0-9]+(?<YEAR2>[0-9]{4,4})?
                ";

            mc = RegexCache.RegEx(patternEpisodes, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace).Matches(movieFileName);
            if (mc.Count > 0)
            {
                var name = mc[0].Groups["NAME"].Value.Replace('.', ' ').Trim();

                if (int.TryParse(mc[0].Groups["YEAR1"].Value, out int year) && year <= DateTime.Now.Year && year > 1900) name = $"{name} ({year})";
                else if (int.TryParse(mc[0].Groups["YEAR2"].Value, out year) && year <= DateTime.Now.Year && year > 1900) name = $"{name} ({year})";

                if (!int.TryParse(mc[0].Groups["S1"].Value, out int season))
                    if (!int.TryParse(mc[0].Groups["S2"].Value, out season))
                        int.TryParse(mc[0].Groups["S3"].Value, out season);

                if (!int.TryParse(mc[0].Groups["E1"].Value, out int episode))
                    if (!int.TryParse(mc[0].Groups["E2"].Value, out episode))
                        int.TryParse(mc[0].Groups["E3"].Value, out episode);

                var series = $"S{season:00}E{episode:00}";
                string tt;

                if (TVSeries.TryGetValue(string.Concat(name, ".", series), out tt))
                {
                    imdbParts.ttSeries = tt;
                    if (TVSeries.TryGetValue(name + ".FOLDERNAME", out tt)) imdbParts.FolderName = tt;
                    if (TVSeries.TryGetValue(name + ".MOVIENAME", out tt)) imdbParts.MovieName = tt;
                    if (TVSeries.TryGetValue(name, out tt)) imdbParts.ttMovie = tt;
                    if (tt == null) return null; //Series not found during a previous search 
                    imdbParts.Series = series;
                    return imdbParts;
                }

                var items = GetMovieProperties(name, true);
                if (items == null)  { TVSeries[name] = null; return null; }

                TVSeries[name + ".MOVIENAME"] = ToMovieName(items["NAME"], items["YEAR"]);
                TVSeries[name + ".FOLDERNAME"] = ToFolderName(items["NAME"], items["YEAR"]);
                TVSeries[name] = items["TT"];

                var job = new Downloader.Job($"https://www.imdb.com/title/{items["TT"]}/episodes?season={season}", tempFileName);
                if (Downloader.Download(job))
                {
                    var html = FileEx.ReadHtml(job.Filename, true);
                    FileEx.Delete(job.Filename);

                    //<a href='/title/tt1060050/?ref_=ttep_ep13' title='A Night in Global Dynamics' itemprop='name'>A Night in Global Dynamics</a>
                    //Warning: =ttep_ep([0-9]+) does not 100% always refer to episode with same number.

                    //Retrieve tt id and episode# properties from episode image and "S1, Ep1" image overlay.
                    mc = RegexCache.RegEx(@"<div data-const='(?<TT>tt[0-9]+)[^>]+><img[^>]+><div>S(?<S>[0-9]+), Ep(?<E>[0-9]+)", RegexOptions.IgnoreCase).Matches(html);
                    int episodesFound = 0;
                    foreach (Match m in mc)
                    {
                        var tt2 = m.Groups["TT"].Value;
                        int.TryParse(m.Groups["E"].Value, out int ep);
                        TVSeries[$"{name}.S{season:00}E{ep:00}"] = tt2;
                        episodesFound++;
                    }

                    //fallback if there is no episode image.
                    mc = RegexCache.RegEx(@"<a href='\/title\/(?<TT>tt[0-9]+)\/\?ref_=ttep_ep(?<E>[0-9]+)", RegexOptions.IgnoreCase).Matches(html);
                    foreach (Match m in mc)
                    {
                        var tt2 = m.Groups["TT"].Value;
                        int.TryParse(m.Groups["E"].Value, out int ep);

                        if (TVSeries.FirstOrDefault(kv => kv.Value == tt2).Value == null)
                        {
                            TVSeries[$"{name}.S{season:00}E{ep:00}"] = tt2;
                            episodesFound++;
                        }
                    }

                    if (episodesFound==0)
                    {
                        SetStatus(Severity.Error, $"Unable to find any {TVSeries[name + ".MOVIENAME"]} season {season} episodes.");
                        return null;
                    }
                }
                else
                {
                    SetStatus(Severity.Error, $"Unable to reach IMDB episodes for {movieFileName}");
                    TVSeries[name] = null;
                    return null;
                }

                if (TVSeries.TryGetValue(string.Concat(name, ".", series), out tt))
                {
                    imdbParts.ttSeries = tt;
                    if (TVSeries.TryGetValue(name + ".FOLDERNAME", out tt)) imdbParts.FolderName = tt;
                    if (TVSeries.TryGetValue(name + ".MOVIENAME", out tt)) imdbParts.MovieName = tt;
                    if (TVSeries.TryGetValue(name, out tt)) imdbParts.ttMovie = tt;
                    if (tt == null) return null; //Series not found during a previous search 
                    imdbParts.Series = series;
                    return imdbParts;
                }

                return null;
            }

            // Find feature movie url. Contains movie name and release year only. No season/episode
            // Possible Filename Permutations:
            //  Ad.Astra.2019.1080p.WEBRip.x264-[YTS.LT].mkv
            //  6.Underground.2019.1080p.WEBRip.x264-[YTS.LT].mp4
            //  The Chair to Everywhere (2019) 720p HDRip x264 - [SHADOW].mp4
            //  John.Wick.Chapter.3.-.Parabellum.2019.1080p.WEBRip.x264-[YTS.LT].mp4
            //  Guardians.Of.The.Galaxy.Vol..2.2017.1080p.BluRay.x264-[YTS.AG].mp4
            //  Lara Croft Tomb Raider 2001 1080p DTS.H.265-PHD.mkv
            //  2001 A Space Odyssey (1968) 1080p.mp4
            //  10,000 BC (2008) 1080p.mp4
            //  1492 - Conquest of Paradise (1992) 1080p.mp4
            //  2036 - Origin Unknown (2018) 1080p.mp4
            //  Space.1999.The.End.1981.1080p.WEBRip.x264-[YTS.LT].mkv
            //  Space.1999.1981.1080p.WEBRip.x264-[YTS.LT].mkv
            //  Dead End  (Crime Drama 1937)  Humphrey Bogart, Sylvia Sidney & Joel McCrea.mp4
            const string patternMovies = @"
                ^(?<NAME>.+?)\(?(?<YEAR>[0-9]{4,4})
                (?:(?<NAME2>.+?)\(?(?<YEAR2>[0-9]{4,4}))?
                ";

            mc = RegexCache.RegEx(patternMovies, RegexOptions.IgnorePatternWhitespace).Matches(movieFileName);
            if (mc.Count > 0)
            {
                var name =  mc[0].Groups["NAME"].Value.Replace('.', ' ').Trim();
                var i = name.IndexOf('(');  //"Dead End  (Crime Drama 1937)  Humphrey Bogart, Sylvia Sidney & Joel McCrea.mp4" => name2 == "Dead End  (Crime Drama"
                if (i != -1) name = name.Substring(0, i).Trim();
                var name2 = name;
                name2 = name2 == string.Empty ? name2 : " " + name2;

                if (int.TryParse(mc[0].Groups["YEAR2"].Value, out int year) && year <= DateTime.Now.Year && year > 1900) name = $"{name} {mc[0].Groups["YEAR2"].Value}{name2} ({year})";
                else if (int.TryParse(mc[0].Groups["YEAR"].Value, out year) && year <= DateTime.Now.Year && year > 1900) name = $"{name} ({year})";

                if (TVSeries.TryGetValue(name, out var tt)) //Offline debugging
                {
                    if (tt == null) return null;
                    imdbParts.ttMovie = tt;
                    if (TVSeries.TryGetValue(name + ".FOLDERNAME", out tt)) imdbParts.FolderName = tt;
                    if (TVSeries.TryGetValue(name + ".MOVIENAME", out tt)) imdbParts.MovieName = tt;
                    return imdbParts;
                }

                var items = GetMovieProperties(name, false);
                if (items == null)
                {
                    TVSeries[name] = null; //Offline debugging
                    return null;
                }

                imdbParts.MovieName = ToMovieName(items["NAME"], items["YEAR"]);
                imdbParts.FolderName = ToFolderName(items["NAME"], items["YEAR"]);
                imdbParts.ttMovie = items["TT"];

                TVSeries[name + ".MOVIENAME"] = imdbParts.MovieName; //Offline debugging
                TVSeries[name + ".FOLDERNAME"] = imdbParts.FolderName;
                TVSeries[name] = imdbParts.ttMovie;

                return imdbParts;
            }

            SetStatus(Severity.Error, $"Unable to parse {movieFileName}");
            return null;
        }

        private static string ToMovieName(string name, string year)
        {
            //'year' may or may not be enclosed in parentheses (e.g. "(2020)") 
            if (year.Length < 4) return name;
            if (year.Length == 4) return string.Concat(name, " (", year, ")");
            return string.Concat(name, " ", year);
        }
        private static string ToFolderName(string name, string year)
        {
            //Put articles to the back so sorting is more sensible.
            if (name.StartsWith("The ", StringComparison.OrdinalIgnoreCase)) name = name.Substring(4) + ", The";
            else if (name.StartsWith("A ", StringComparison.OrdinalIgnoreCase)) name = name.Substring(2) + ", A";

            //'year' may or may not be enclosed in parentheses (e.g. "(2020)") 
            if (year.Length < 4) return name;
            if (year.Length == 4) return string.Concat(name, " (", year, ")");
            return string.Concat(name, " ", year);
        }

        /// <summary>
        /// Find true movie name, release year, and IMDB tt code based upon full or partial name.
        /// </summary>
        /// <param name="name">Suggested movie name. May or may not include release year.</param>
        /// <param name="series">True if we are searching for feature movie or a series parent.</param>
        /// <returns>Dictionary of (NAME, YEAR, and TT) or null if no match or other error.</returns>
        private Dictionary<string, string> GetMovieProperties(string name, bool series)
        {
            string html;
            var tempFileName = Path.Combine(Path.GetTempPath(), "FindUrl.htm");

            var job = new Downloader.Job($"https://www.imdb.com/find/?q={Uri.EscapeUriString(name)}&s=tt&exact=true", tempFileName); //strict search
            if (Downloader.Download(job))
            {
                html = FileEx.ReadHtml(job.Filename, true);
                FileEx.Delete(job.Filename); //no longer needed.
                var items = ParseHtml(html, series);
                if (items != null) return items;
            }
            else
            {
                SetStatus(Severity.Error, $"Unable to reach IMDB for {name}");
                return null;
            }

            SetStatus(Severity.Warning, $"Exact search for \"{name}\" failed. Trying fuzzier search.");
            //Try a fuzzier search w/o year part e.g. "(2020)". May find the wrong movie but it's better than nothing.
            var i = name.IndexOf('(');
            var fuzzyName = i == -1 ? name : name.Substring(0, i).TrimEnd();

            job = new Downloader.Job($"https://www.imdb.com/find/?q={Uri.EscapeUriString(fuzzyName)}&s=tt", tempFileName);  //try again, not so strict.
            if (Downloader.Download(job))
            {
                html = FileEx.ReadHtml(job.Filename, true);
                FileEx.Delete(job.Filename); //no longer needed.
                var items = ParseHtml(html, series);
                if (items != null) return items;
            }
            else
            {
                SetStatus(Severity.Error, $"Unable to reach IMDB for {name}");
                return null;
            }

            SetStatus(Severity.Error, $"No match for \"{name}\".");
            return null;
        }

        //Parse the Search/Find results
        private static readonly Regex reFindResult1 = RegexCache.RegEx(@"href='/title/(?<TT>tt[0-9]+)/[^>]+>(?<NAME>[^<]+).+?<label[^>]+>(?<YEAR>[0-9]{4,4})", RegexOptions.IgnoreCase);
        //private static readonly Regex reFindResult2 = RegexCache.RegEx(@"href='/title/(?<TT>tt[0-9]+)/[^>]+>(?<NAME>[^<]+).+?<span [^>]+>(?<YEAR>[0-9]{4,4})", RegexOptions.IgnoreCase); 
        private static readonly Regex reFindResult2 = RegexCache.RegEx(@"href='/title/(?<TT>tt[0-9]+)/[^>]+>(?<NAME>[^<]+).+?<span [^>]+>(?<YEAR>[0-9]{4,4}).+?<span [^>]+>(?<CLASS>[^<]+)", RegexOptions.IgnoreCase);
        private static readonly Regex reInvalidFileNameChars = RegexCache.RegEx($@"\s*[{Regex.Escape(new String(Path.GetInvalidFileNameChars()))}]\s*", RegexOptions.IgnoreCase);
        private static Dictionary<string, string> ParseHtml(string html, bool series)
        {
            var mc = reFindResult1.Matches(html);
            if (mc.Count==0) mc = reFindResult2.Matches(html);
            foreach (Match m in mc)
            {
                if (series && !m.Groups["CLASS"].Value.ContainsI("Series")) return null; //We expect the video to be a TV Series or TV Mini-Series
                return new Dictionary<string, string>()
                {
                    { "TT", m.Groups["TT"].Value },
                    { "NAME", reInvalidFileNameChars.Replace(System.Net.WebUtility.HtmlDecode(m.Groups["NAME"].Value), "-") },
                    { "YEAR", m.Groups["YEAR"].Value }
                };
            }

            return null; //no matches of the specified type
        }

        public class ImdbParts
        {
            /// <summary>
            /// Movie or TVSeries full name with illegal chars replaced with dashes.
            /// In addition, starting "The" and "A" articles moved to end for sorting purposes 
            /// e.g. "Time Machine, The (1992)" 
            /// This is used for folder names.
            /// </summary>
            public string FolderName { get; set; }

            /// <summary>
            /// Movie or TVSeries full name with illegal chars replaced with dashes.
            /// Same as FolderName but without the article swap.
            /// e.g. "The Time Machine (1992)" 
            /// This is used for shortcut names.
            /// </summary>
            public string MovieName { get; set; }

            public string ttMovie { get; set; }    //Movie or TVSeries IMDB TT code e.g. "tt1234567"

            public string Series { get; set; }     //Series episode e.g. "S01E01". Null if not an episode.

            public string ttSeries { get; set; }   //IMDB Episode TT code e.g. "tt1234567" Null if not an episode.
        }

        #region Offline Debugging
        //In Debug mode and 'OfflineDebugging' defined at top of file, these 
        //methods save the temporary 'TVSeries' cache upon close and restores it
        //upon startup so repeated debugging may occur without internet access.
        //Useful for debugging all the different ways movies and their folders 
        //are arranged.

        public class TVSeriesCache //exclusively for serialization only.
        {
            public string SelectedFolder { get; set; }

            public List<DictEntry> Items { get; set; }

            public class DictEntry
            {
                [XmlAttribute] public string Key { get; set; }
                [XmlAttribute] public string Value { get; set; }
            }
        }

        private void DebugExportCache()
        {
#if (DEBUG && OfflineDebugging)
            try
            {
                if (TVSeries.Count == 0 || RootFolder == null) return;
                var fn = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".Cache.xml");
                SetStatus(Severity.Verbose, $"Debugging: Saving TVSeries dictionary to {fn}");
                XmlIO.Serialize(new TVSeriesCache { SelectedFolder = RootFolder, Items = TVSeries.Select((kv) => new TVSeriesCache.DictEntry { Key = kv.Key, Value = kv.Value }).OrderBy((kv)=>kv.Key).ToList() }, fn);
            }
            catch(Exception ex)
            {
                SetStatus(Severity.Error, "Debugging: Saving TVSeries dictionary to {0}", ex);
            }
#endif
        }

        private Dictionary<string, string> DebugImportCache()
        {
#if (DEBUG && OfflineDebugging)
            try
            {
                var fn = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".Cache.xml");
                if (FileEx.Exists(fn))
                {
                    SetStatus(Severity.Verbose, $"Debugging: Restoring TVSeries dictionary from {fn}");
                    var cache = XmlIO.Deserialize<TVSeriesCache>(fn);
                    m_txtRoot.Text = cache.SelectedFolder;
                    return cache.Items.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                SetStatus(Severity.Error, "Debugging: Restoring TVSeries dictionary from {0}", ex);
            }
#endif
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion  //Offline Debugging
    }
}
