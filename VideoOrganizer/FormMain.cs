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
using System.Net;
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
            Log.MessageCapture += SetStatus;  //copy to status window low level logging errors from internet downloads.

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

        private void m_btnGo_Click(object sender, EventArgs e)
        {
            if (m_txtRoot.Text == string.Empty)
            {
                MiniMessageBox.Show(this, "Root folder has\nnot yet been set.", m_txtRoot.TextLabel, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(m_txtRoot.Text))
            {
                MiniMessageBox.Show(this, "Root folder\ndoes not exist", m_txtRoot.TextLabel, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void SetStatus(string msg)
        {
            SetStatus(Severity.None, msg);
        }

        private void SetStatus(Severity severity, string fmt, params object[] args)
        {
            //Cleanup string and indent succeeding lines
            #if DEBUG
                if (args != null && args.Length > 0)  fmt = string.Format(fmt, args);
            #else
                //Cleanup string and indent succeeding lines. But as this is release
                //mode, exceptions show only the message not the entire call stack.
                //Users wouldn't know what to do with the call stack, anyway.
                if (args != null && args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] is Exception) args[i] = ((Exception)args[i]).Message;
                    }
                    fmt = string.Format(fmt, args);
                }
            #endif

            fmt = fmt.Beautify(false, "    ").TrimStart();
            SetStatus(severity, fmt);
        }

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
                case Severity.Success: clr = Color.Green;  break;
                case Severity.Error:   clr = Color.Red;    break;
                case Severity.Warning: clr = Color.Gold;   break;
                case Severity.Info:    clr = Color.Blue;   break;
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

            m_rtfStatus.AppendText(msg);
            //if (msg[msg.Length-1] != '\n') 
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
                    if (!File.Exists(oldPath)) oldPath = Path.Combine(prevNewFolder, Path.GetFileName(oldPath));
                    if (!File.Exists(oldPath)) { SetStatus(Severity.Error, $"Video {oldPath.Substring(RootFolder.Length)} not found."); continue; }

                    var folder = Path.GetDirectoryName(oldPath);

                    var i = folder.IndexOf("\\" + imdbParts.FolderName);
                    var newFolder = Path.Combine(i == -1 ? (folder==RootFolder ? folder : Path.GetDirectoryName(folder)) : folder.Substring(0, i), imdbParts.FolderName);
                    var newPath = Path.Combine(newFolder, Path.GetFileName(oldPath));

                    if (folder == previousFolder || folder == prevNewFolder || folder == RootFolder) { CreateFolder(newFolder); MoveFile(oldPath, newPath); }
                    else { MoveFolder(folder, newFolder);  }
                    if (!File.Exists(oldPath)) oldPath = Path.Combine(newFolder, Path.GetFileName(oldPath));
                    CreateTTShortcut(Path.Combine(newFolder, imdbParts.MovieName + ".url"), imdbParts.ttMovie);

                    if (imdbParts.Series != null) //TV Series Episode
                    {
                        var newSeriesFolder = Path.Combine(newFolder, imdbParts.Series);
                        newPath = Path.Combine(newSeriesFolder, Path.GetFileName(oldPath));
                        CreateFolder(newSeriesFolder);
                        MoveFile(oldPath, newPath);
                        CreateTTShortcut(Path.Combine(newSeriesFolder, $"{imdbParts.MovieName}.{imdbParts.Series}.url"), imdbParts.ttSeries);
                    }

                    previousFolder = folder;
                    prevNewFolder = newFolder;
                }
            }
            catch (Exception ex)
            {
                SetStatus(Severity.Error, "Fatal: {1}", ex);
                return;
            }

            DebugExportCache();
            SetStatus(string.Empty);
            SetStatus(Severity.Success, "Completed.");
        }

        public void CreateTTShortcut(string filepath, string tt)
        {
            if (File.Exists(filepath)) return;
            //http://www.lyberty.com/encyc/articles/tech/dot_url_format_-_an_unofficial_guide.html

            //One cannot use the website favicon for the shortcut icon directly from the website url. It must be downloaded to a local file before it can be used!
            //https://docs.microsoft.com/en-us/answers/questions/120626/internet-shortcut-url-file-no-longer-supports-remo.html
            var favicon = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures), "favicon_imdb.ico");
            if (!File.Exists(favicon))
            {
                var job = new FileEx.Job(null, "https://www.imdb.com/favicon.ico", favicon);
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

            if (tt==MovieProperties.EmptyTitleID)
            {
                File.WriteAllText(filepath, $"[InternetShortcut]\r\nURL={new Uri(Path.GetDirectoryName(filepath))}\r\nIconIndex=129\r\nIconFile=C:\\Windows\\System32\\SHELL32.dll\r\nAuthor=VideoLibrarian.exe");
            }
            else
            {
                File.WriteAllText(filepath, $"[InternetShortcut]\r\nURL=https://www.imdb.com/title/{tt}/ \r\nIconFile={favicon}\r\nIconIndex=0\r\nHotKey=0\r\nIDList=\r\nAuthor=VideoLibrarian.exe");
            }
            SetStatus($"Created shortcut {Path.GetFileNameWithoutExtension(filepath)} ==> https://www.imdb.com/title/{tt}/");
        }

        //Bug in Regex.Escape(@"~`'!@#$%^&*(){}[].,;+_=-"). It doesn't escape ']'
        private static readonly Regex ReBracketed = new Regex(@"\\[~`'!@\#\$%\^&\*\(\)\{}\[\]\.,;\+_=-][^~`'!@\#\$%\^&\*\(\)\{}\[\]\.,;\+_=-]+[~`'!@\#\$%\^&\*\(\)\{}\[\]\.,;\+_=-]\\", RegexOptions.Compiled);
        private static IEnumerable<string> UnprocessedMovieList(string rootFolder)
        {
            //We must have a realized list because we may be moving folders and files around causing unrealized enumerations to break.
            var list = new List<string>();

            //Enumerate all videos. Ignoring all videos with matching shortcuts.

            foreach (var f in Directory.EnumerateFiles(rootFolder, "*.*", SearchOption.AllDirectories))
            {
                if (!MovieProperties.IsVideoFile(f)) continue;
                var folder = Path.GetDirectoryName(f);

                //Special: if video is in a bracketed folder (or any of its child folders) the video is ignored. 
                if (ReBracketed.IsMatch(folder + "\\")) continue;

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

                var isTVSeriesRootFolder = Directory.EnumerateDirectories(folder).Any((f)=>Regex.IsMatch(f, @"\\S[0-9]{2,2}E[0-9]{2,2}\\?$", RegexOptions.Compiled | RegexOptions.IgnoreCase));
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
            if (!File.Exists(src)) { SetStatus(Severity.Error, $"Source File {src.Substring(RootFolder.Length)} does not exist."); return; }
            if (!File.Exists(dst)) File.Move(src, dst);
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
            var pattern = @"
                ^(?:[0-9]{1,2}[ -]+)?
                (?<NAME>.+?)
                (?:[ \.\(]*(?<YEAR1>[0-9]{4,4})[ \.\)-]*)?
                (?:
                (?:S(?<S1>[0-9]{2,2})E(?<E1>[0-9]{2,2}))|
                (?:(?<S2>[0-9]{1,2})x(?<E2>[0-9]{1,2}))|
                (?:[ \.-]+Episode[ \.](?<S3>[0-9]{1,2})\.(?<E3>[0-9]{1,2}))
                )
                [^0-9]+(?<YEAR2>[0-9]{4,4})?
                ";

            mc = Regex.Matches(movieFileName, pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
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

                var job = new FileEx.Job(null, $"https://www.imdb.com/title/{items["TT"]}/episodes?season={season}", tempFileName);
                if (FileEx.Download(job))
                {
                    var html = FileEx.ReadHtml(job.Filename, true);
                    File.Delete(job.Filename);

                    //<a href='/title/tt1060050/?ref_=ttep_ep13' title='A Night in Global Dynamics' itemprop='name'>A Night in Global Dynamics</a>
                    mc = Regex.Matches(html, @"<a href='\/title\/(?<TT>tt[0-9]+)\/\?ref_=ttep_ep(?<EP>[0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if (mc.Count == 0)
                    {
                        SetStatus(Severity.Error, $"Unable to find any {TVSeries[name + ".MOVIENAME"]} season {season} episodes.");
                        return null;
                    }

                    foreach (Match m in mc)
                    {
                        int.TryParse(m.Groups["EP"].Value, out int ep);
                        TVSeries[$"{name}.S{season:00}E{ep:00}"] = m.Groups["TT"].Value;
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
            var pattern2 = @"
                ^(?<NAME>.+?)\(?(?<YEAR>[0-9]{4,4})
                (?:(?<NAME2>.+?)\(?(?<YEAR2>[0-9]{4,4}))?
                ";

            mc = Regex.Matches(movieFileName, pattern2, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
            if (mc.Count > 0)
            {
                var name = mc[0].Groups["NAME"].Value.Replace('.', ' ').Trim();
                var name2 = mc[0].Groups["NAME"].Value.Replace('.', ' ').Trim();
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
            //'year' is already enclosed in parentheses (e.g. "(2020)") 
            return string.Concat(name, " ", year);
        }
        private static string ToFolderName(string name, string year)
        {
            //Put articles to the back so sorting is more sensible.
            if (name.StartsWith("The ", StringComparison.OrdinalIgnoreCase)) name = name.Substring(4) + ", The";
            else if (name.StartsWith("A ", StringComparison.OrdinalIgnoreCase)) name = name.Substring(2) + ", A";
            //'year' is already enclosed in parentheses (e.g. "(2020)") 
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

            var job = new FileEx.Job(null, $"https://www.imdb.com/find?q={WebUtility.UrlEncode(name)}&s=tt&exact=true", tempFileName); //strict search
            if (FileEx.Download(job))
            {
                html = FileEx.ReadHtml(job.Filename, true);
                File.Delete(job.Filename); //no longer needed.
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

            job = new FileEx.Job(null, $"https://www.imdb.com/find?q={WebUtility.UrlEncode(fuzzyName)}&s=tt", tempFileName);  //try again, not so strict.
            if (FileEx.Download(job))
            {
                html = FileEx.ReadHtml(job.Filename, true);
                File.Delete(job.Filename); //no longer needed.
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

        private static readonly Regex reFindResult = new Regex(@"class='result_text'><a href='\/?title\/(?<TT>tt[0-9]+)\/[^>]+>(?<NAME>[^<]+)<\/a>.*?(?<YEAR>\([0-9]{4,4}[^\)]*\))(?:[^<]*(?<SERIES>Series))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex reInvalidFileNameChars = new Regex($@"\s*[{Regex.Escape(new String(Path.GetInvalidFileNameChars()))}]\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Dictionary<string, string> ParseHtml(string html, bool series)
        {
            //Find first found of series or non-series type:
            //Examples:
            //<td class='result_text'><a href='/title/tt8787802/?ref_=fn_tt_tt_1'>Away</a>(2020) (TV Series)</td>
            //<td class='result_text'><a href='/title/tt8288450/?ref_=fn_tt_tt_2'>Away</a>(I) (2019)</td>
            //<td class='result_text'><a href='/title/tt8288450/?ref_=fn_tt_tt_3'>Away</a>(2019)</td>
            //<td class='result_text'><a href='/title/tt8288450/?ref_=fn_tt_tt_4'>Away</a>(2019- )</td>
            //<td class='result_text'><a href='/title/tt12905120/?ref_=fn_tt_tt_5'>Away</a>(IV) (2019) (Short)</td>
            //<td class='result_text'><a href='/title/tt3696720/?ref_=fn_tt_tt_1'>Ascension</a>(2014)(TV Mini-Series)</td>

            var mc = reFindResult.Matches(html);
            foreach(Match m in mc)
            {
                if (series && m.Groups["SERIES"].Value == string.Empty) continue;
                if (!series && m.Groups["SERIES"].Value != string.Empty) continue;
                return new Dictionary<string, string>()
                {
                    { "TT", m.Groups["TT"].Value },
                    { "NAME", reInvalidFileNameChars.Replace(m.Groups["NAME"].Value, "-") },
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
                if (File.Exists(fn))
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
