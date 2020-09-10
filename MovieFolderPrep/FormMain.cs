using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MovieGuide;

namespace MovieFolderPrep
{
    public partial class FormMain : Form
    {
        private ToolTipHelp tt;

        //FindImdbUrl() cached TVSeries TT codes so we don't have to query the IMDB web page for every episode.
        private readonly Dictionary<string, string> TVSeries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly string Empty_txtRootText = null;  //for m_btnGo_Click()
        private readonly Font RegularFont;  //for SetStatus()
        private readonly Font BoldFont;

        public FormMain()
        {
            Log.MessageCapture += SetStatus;  //copy to status window low level logging errors from internet downloads.

            InitializeComponent();

            tt = new ToolTipHelp(this); //must be after InitializeComponent()

            Empty_txtRootText = m_txtRoot.Text; //to detect if root folder has been set

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
        }

        private void m_btnAbout_Click(object sender, EventArgs e)
        {
            AboutBox.Show(this);
        }

        private void m_btnGo_Click(object sender, EventArgs e)
        {
            if (m_txtRoot.Text == Empty_txtRootText)
            {
                MiniMessageBox.Show(this, "Root folder has\nnot yet been set.", Empty_txtRootText, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(m_txtRoot.Text))
            {
                MiniMessageBox.Show(this, "Root folder\ndoes not exist", Empty_txtRootText, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            m_btnGo.Enabled = false; //don't allow user to click "Go" again.
            var task = Task.Run(() => DoWork(m_txtRoot.Text)); //Do work asynchronously.
            //task.Wait(); task.Dispose(); //don't wait for completion. A success message will be displayed in the status window.
            return;
        }

        private void m_btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void m_btnSelectRoot_Click(object sender, EventArgs e)
        {
            var dir = FolderSelectDialog.Show(this, "Select Movie/Media Folder");
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

        private void DoWork(string rootFolder)
        {
            SetStatus(Severity.Success, "Started\n");

            try
            {
                var previousFolder = rootFolder;
                var prevNewFolder = rootFolder;

                foreach (var f in OrphanedMovieList(rootFolder))
                {
                    var imdbParts = FindImdbUrl(f);
                    if (imdbParts == null) continue;

                    SetStatus(Severity.Info, $"Moving \"{f.Replace(rootFolder+"\\", "")}\" => \"{(imdbParts.Series == null ? imdbParts.FolderName : imdbParts.FolderName + "\\" + imdbParts.Series)}\"");

                    var folder = Path.GetDirectoryName(f);
                    string newFolder;
                    string shortcut;

                    if (imdbParts.Series == null) //feature movie.
                    {
                        if (folder == previousFolder || folder == rootFolder)
                        {
                            var ff = f;  //need to check if there are more than one video in a folder.
                            if (!File.Exists(ff)) ff = Path.Combine(prevNewFolder, Path.GetFileName(f));
                            if (!File.Exists(ff)) { SetStatus(Severity.Error, $"Video {ff} not found."); continue; }

                            newFolder = Path.Combine(rootFolder, imdbParts.FolderName);
                            if (Directory.Exists(newFolder)) { SetStatus(Severity.Error, $"Folder {imdbParts.FolderName} already exists."); continue; }
                            Directory.CreateDirectory(newFolder);
                            File.WriteAllText(Path.Combine(newFolder, imdbParts.MovieName + ".url"), $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttMovie}/\n");
                            File.Move(ff, Path.Combine(newFolder, Path.GetFileName(ff)));
                            previousFolder = folder;
                            prevNewFolder = newFolder;
                            SetStatus($"Moved {imdbParts.FolderName}, https://www.imdb.com/title/{imdbParts.ttMovie}/");
                            continue;
                        }

                        newFolder = Path.Combine(Path.GetDirectoryName(folder), imdbParts.FolderName);
                        if (folder != newFolder && Directory.Exists(newFolder)) { SetStatus(Severity.Error, $"Folder {imdbParts.FolderName} already exists."); continue; }
                        if (!Directory.Exists(newFolder)) Directory.Move(folder, newFolder);
                        File.WriteAllText(Path.Combine(newFolder, imdbParts.MovieName + ".url"), $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttMovie}/\n");
                        //File.Move(f, Path.Combine(newFolder, Path.GetFileName(f)));
                        previousFolder = folder;
                        prevNewFolder = newFolder;
                        SetStatus($"Moved {imdbParts.FolderName}, https://www.imdb.com/title/{imdbParts.ttMovie}/");
                        continue;
                    }
                    else //TV Series Episode
                    {
                        if (folder == previousFolder || folder == rootFolder)
                        {
                            var ff = f;  //need to check if there are more than one video in a folder.
                            if (!File.Exists(ff)) ff = Path.Combine(prevNewFolder, Path.GetFileName(f));
                            if (!File.Exists(ff)) { SetStatus(Severity.Error, $"Video {ff} not found."); continue; }

                            newFolder = Path.Combine(folder, imdbParts.FolderName);
                            if (!Directory.Exists(newFolder)) Directory.CreateDirectory(newFolder);
                            shortcut = Path.Combine(newFolder, imdbParts.MovieName + ".url");
                            if (!File.Exists(shortcut))
                            {
                                foreach (var f2 in Directory.EnumerateFiles(newFolder, "*.url", SearchOption.TopDirectoryOnly)) File.Delete(f2);
                                File.WriteAllText(shortcut, $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttMovie}/\n");
                                SetStatus($"Set {imdbParts.FolderName} episodes root folder, https://www.imdb.com/title/{imdbParts.ttMovie}/");
                            }

                            newFolder = Path.Combine(newFolder, imdbParts.Series);
                            if (Directory.Exists(newFolder)) { SetStatus(Severity.Error, $"Folder {imdbParts.FolderName}/{imdbParts.Series} already exists."); continue; }
                            Directory.CreateDirectory(newFolder);

                            File.Move(ff, Path.Combine(newFolder, Path.GetFileName(ff)));
                            File.WriteAllText(Path.Combine(newFolder, $"{imdbParts.MovieName}.{imdbParts.Series}.url"), $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttSeries}/\n");
                            previousFolder = folder;
                            prevNewFolder = newFolder;
                            SetStatus($"Moved {imdbParts.FolderName} {imdbParts.Series}, https://www.imdb.com/title/{imdbParts.ttSeries}/");
                            continue;
                        }

                        newFolder = Path.Combine(Path.GetDirectoryName(folder), imdbParts.FolderName);
                        if (!Directory.Exists(newFolder)) Directory.Move(folder, newFolder);
                        folder = newFolder;
                        shortcut = Path.Combine(newFolder, imdbParts.MovieName + ".url");
                        if (!File.Exists(shortcut))
                        {
                            foreach (var f2 in Directory.EnumerateFiles(newFolder, "*.url", SearchOption.TopDirectoryOnly)) File.Delete(f2);
                            File.WriteAllText(shortcut, $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttMovie}/\n");
                            SetStatus($"Set {imdbParts.FolderName} episodes root folder, https://www.imdb.com/title/{imdbParts.ttMovie}/");
                        }

                        newFolder = Path.Combine(newFolder, imdbParts.Series);
                        if (Directory.Exists(newFolder)) { SetStatus(Severity.Error, $"Folder {imdbParts.FolderName}/{imdbParts.Series} already exists."); continue; }
                        Directory.CreateDirectory(newFolder);
                        File.Move(Path.Combine(folder, Path.GetFileName(f)), Path.Combine(newFolder, Path.GetFileName(f)));
                        File.WriteAllText(Path.Combine(newFolder, $"{imdbParts.MovieName}.{imdbParts.Series}.url"), $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttSeries}/\n");
                        previousFolder = folder;
                        prevNewFolder = newFolder;
                        SetStatus($"Moved {imdbParts.FolderName} {imdbParts.Series}, https://www.imdb.com/title/{imdbParts.ttSeries}/");
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus(Severity.Error, "Fatal: {1}", ex);
                return;
            }

            SetStatus(Severity.Success, "Completed.");
        }

        private static List<string> OrphanedMovieList(string rootFolder)
        {
            //We must have a realized list because we may be moving folders and files around causing unrealized enumerations to break.
            var list = new List<string>();

            //Enumerate all videos. Ignoring all videos with matching shortcuts.
            foreach (var f in Directory.EnumerateFiles(rootFolder, "*.*", SearchOption.AllDirectories))
            {
                if (MovieProperties.MovieExtensions.IndexOf(Path.GetExtension(f)) == -1) continue;
                var folder = Path.GetDirectoryName(f);
                //A video in the root folder is always valid even if there are other shortcuts in
                //the root folder, because we will be moving the video into its own folder anyway. 
                if (folder != rootFolder) 
                {
                    var urlFound = false;
                    foreach (var f2 in Directory.EnumerateFiles(folder, "*.url", SearchOption.TopDirectoryOnly))
                    {
                        var link = MovieProperties.GetUrlFromLink(f2);
                        if (link == null) continue;
                        if (link.ContainsI("imdb.com/title/tt"))
                        {
                            //It's OK if there is a TVSeries shortcut in a folder full of episode videos.
                            var episodeCount = 0;
                            foreach (var f3 in Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly))
                            {
                                if (MovieProperties.MovieExtensions.IndexOf(Path.GetExtension(f3)) == -1) continue;
                                var mc = Regex.Matches(f3, @"[ \.]S[0-9]{2,2}E[0-9]{2,2}[ \.]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                if (mc.Count > 0) episodeCount++;
                            }
                            if (episodeCount <= 1) { urlFound = true; break; } 
                        }
                    }
                    if (urlFound) continue;
                }

                list.Add(f);
            }

            return list;
        }

        /// <summary>
        /// Find IMDB url from movie file name. Only works for well-formed file names
        /// where: 
        /// (1) Feature movie filenames consist of moviename followed by a 4-digit release year (plus a lot of other stuff).
        /// (2) Episodic TV series filenames consist of tv series name followed by season/episode in the form of S00E00. There is no release year.
        /// </summary>
        /// <param name="movieFileName">Full filename of video file.</param>
        /// <returns>IMDB url or null if not found</returns>
        private ImdbParts FindImdbUrl(string movieFileFullPath)
        {
            var imdbParts = new ImdbParts();

            MatchCollection mc;
            var movieFileName = Path.GetFileName(movieFileFullPath);
            var tempFileName = Path.Combine(Path.GetTempPath(), "FindUrl.htm");

            //-------------------------------------------------------------
            //Use https://regex101.com to validate the regex patterns. 
            //-------------------------------------------------------------

            //Find TV Series episode url. Contains TV series name + season and episode (No year!)
            mc = Regex.Matches(movieFileName, @"^(?<NAME>.+?)(?<SERIES>S(?<S>[0-9]{2,2})E(?<E>[0-9]{2,2}))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (mc.Count > 0)
            {
                var name = mc[0].Groups["NAME"].Value.Replace('.', ' ').Trim();
                var season = int.Parse(mc[0].Groups["S"].Value);
                var episode = int.Parse(mc[0].Groups["E"].Value);
                var series = mc[0].Groups["SERIES"].Value.ToUpper();
                string tt;

                if (TVSeries.TryGetValue(name, out tt) && tt == null) return null; //Series not found during a previous search 
                if (TVSeries.TryGetValue(string.Concat(name, ".", series), out tt))
                {
                    imdbParts.Series = series;
                    imdbParts.ttSeries = tt;
                    if (TVSeries.TryGetValue(name, out tt)) imdbParts.ttMovie = tt;
                    if (TVSeries.TryGetValue(name + ".FOLDERNAME", out tt)) imdbParts.FolderName = tt;
                    if (TVSeries.TryGetValue(name + ".MOVIENAME", out tt)) imdbParts.MovieName = tt;
                    return imdbParts;
                }

                var items = FindMovie(name, true);
                if (items == null)
                {
                    TVSeries[name] = null;
                    return null;
                }

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
                    imdbParts.Series = series;
                    imdbParts.ttSeries = tt;
                    if (TVSeries.TryGetValue(name, out tt)) imdbParts.ttMovie = tt;
                    if (TVSeries.TryGetValue(name + ".FOLDERNAME", out tt)) imdbParts.FolderName = tt;
                    if (TVSeries.TryGetValue(name + ".MOVIENAME", out tt)) imdbParts.MovieName = tt;
                    return imdbParts;
                }
            }

            //Find feature movie url. Contains movie name and release year.
            mc = Regex.Matches(movieFileName, @"^(?<NAME>.+?)(?<YEAR>[0-9]{4,4})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (mc.Count > 0)
            {
                var name = mc[0].Groups["NAME"].Value.Replace('.', ' ').Trim();
                if (int.TryParse(mc[0].Groups["YEAR"].Value, out int year) && year <= DateTime.Now.Year && year > 1900) name = $"{name} ({year})";

                var items = FindMovie(name, false);
                if (items == null) return null;

                imdbParts.MovieName = ToMovieName(items["NAME"], items["YEAR"]);
                imdbParts.FolderName = ToFolderName(items["NAME"], items["YEAR"]);
                imdbParts.ttMovie = items["TT"];

                return imdbParts;
            }

            SetStatus(Severity.Error, $"Unable to parse {movieFileName}");
            return null;
        }

        private static string ToMovieName(string name, string year)
        {
            return string.Concat(name, " ", year);
        }

        private static string ToFolderName(string name, string year)
        {
            if (name.StartsWith("The ", StringComparison.OrdinalIgnoreCase)) name = name.Substring(4) + ", The";
            else if (name.StartsWith("A ", StringComparison.OrdinalIgnoreCase)) name = name.Substring(2) + ", A";
            return string.Concat(name, " ", year);
        }

        /// <summary>
        /// Find true movie name, release year, and IMDB tt code based upon full or partial name.
        /// </summary>
        /// <param name="name">Suggested movie name. May or may not include release year.</param>
        /// <param name="series">True if we are searching for feature movie or a series parent.</param>
        /// <returns>Dictionary of (NAME, YEAR, and TT) or null if no match or other error.</returns>
        private Dictionary<string,string> FindMovie(string name, bool series)
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

        private class ImdbParts
        {
            /// <summary>
            /// Movie or TVSeries full name with illegal chars replaced with dashes.
            /// In addition, starting "The" and "A" articles moved to end for sorting purposes 
            /// e.g. "Time Machine, The (1992)" 
            /// Used for folder names.
            /// </summary>
            public string FolderName { get; set; }

            /// <summary>
            /// Movie or TVSeries full name with illegal chars replaced with dashes.
            /// Same as FolderName but without the article swap.
            /// e.g. "The Time Machine (1992)" 
            /// Used for shortcut names.
            /// </summary>
            public string MovieName { get; set; }

            public string ttMovie { get; set; }    //Movie or TVSeries TT code e.g. "tt1234567"
            public string Series { get; set; }     //Series episode e.g. "S01E01". Null if not an episode.
            public string ttSeries { get; set; }   //Episode TT code e.g. "tt1234567" Null if not an episode.
        }
    }
}
