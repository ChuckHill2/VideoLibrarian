using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MovieGuide;

namespace MovieFolderPrep
{
    public partial class FormMain : Form
    {
        //FindImdbUrl() cached TVSeries TT codes so we don't have to query the IMDB web page for every episode.
        private readonly Dictionary<string, string> TVSeries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly string Empty_txtRootText = null;  //for m_btnGo_Click()
        private readonly Font RegularFont;  //for SetStatus()
        private readonly Font BoldFont;

        public FormMain()
        {
            Log.MessageCapture += SetStatus;

            InitializeComponent();

            Empty_txtRootText = m_txtRoot.Text; //to detect if root folder has been set

            //Optionally get root folder from command-line
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && Directory.Exists(args[1]))
                m_txtRoot.Text = Path.GetFullPath(args[1]);
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
        }

        private void DoWork(string rootFolder)
        {
            SetStatus(Severity.Success, "Started\n");

            //Enumerate all videos. Ignoring all videos with matching shortcuts.
            foreach (var f in OrphanedMovieList(rootFolder))
            {
                var imdbParts = FindImdbUrl(f);
                if (imdbParts == null) continue;

                SetStatus(Severity.Info, $"Moving {f.Replace(rootFolder,"")} => \\{(imdbParts.Series == null ? imdbParts.FolderName : imdbParts.FolderName+"\\"+imdbParts.Series)}\\");

                var folder = Path.GetDirectoryName(f);
                string newFolder;
                string shortcut;

                if (imdbParts.Series == null) //feature movie.
                {
                    if (folder == rootFolder)
                    {
                        newFolder = Path.Combine(folder, imdbParts.FolderName);
                        Directory.CreateDirectory(newFolder);
                        File.Move(f, Path.Combine(newFolder, Path.GetFileName(f)));
                        File.WriteAllText(Path.Combine(newFolder, imdbParts.MovieName + ".url"), $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttMovie}/\n");
                        SetStatus($"Moved {imdbParts.FolderName} => https://www.imdb.com/title/{imdbParts.ttMovie}/");
                        continue;
                    }

                    File.WriteAllText(Path.Combine(folder, imdbParts.MovieName + ".url"), $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttMovie}/\n");
                    newFolder = Path.Combine(Path.GetDirectoryName(folder), imdbParts.FolderName);
                    if (!Directory.Exists(newFolder)) Directory.Move(folder, newFolder);
                    SetStatus($"Moved {imdbParts.FolderName} => https://www.imdb.com/title/{imdbParts.ttMovie}/");
                    continue;
                }
                else //TV Series Episode
                {
                    if (folder == rootFolder)
                    {
                        newFolder = Path.Combine(folder, imdbParts.FolderName, imdbParts.Series);
                        Directory.CreateDirectory(newFolder);
                        shortcut = Path.Combine(folder, imdbParts.FolderName, imdbParts.MovieName + ".url");
                        if (!File.Exists(shortcut))
                        {
                            File.WriteAllText(shortcut, $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttMovie}/\n");
                            SetStatus($"Set {imdbParts.FolderName} root folder => https://www.imdb.com/title/{imdbParts.ttMovie}/");
                        }

                        File.Move(f, Path.Combine(newFolder, Path.GetFileName(f)));
                        File.WriteAllText(Path.Combine(newFolder, imdbParts.MovieName + ".url"), $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttSeries}/\n");
                        SetStatus($"Moved {imdbParts.FolderName} {imdbParts.Series} => https://www.imdb.com/title/{imdbParts.ttSeries}/");
                        continue;
                    }

                    newFolder = Path.Combine(Path.GetDirectoryName(folder), imdbParts.FolderName);
                    if (!Directory.Exists(newFolder)) Directory.Move(folder, newFolder);
                    folder = newFolder;
                    shortcut = Path.Combine(folder, imdbParts.MovieName + ".url");
                    if (!File.Exists(shortcut))
                    {
                        File.WriteAllText(shortcut, $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttMovie}/\n");
                        SetStatus($"Set {imdbParts.FolderName}: root folder => https://www.imdb.com/title/{imdbParts.ttMovie}/");
                    }

                    newFolder = Path.Combine(folder, imdbParts.Series);
                    if (!Directory.Exists(newFolder)) Directory.CreateDirectory(newFolder);
                    File.Move(Path.Combine(folder, Path.GetFileName(f)), Path.Combine(newFolder, Path.GetFileName(f)));
                    File.WriteAllText(Path.Combine(newFolder, imdbParts.MovieName + ".url"), $"[InternetShortcut]\nURL=https://www.imdb.com/title/{imdbParts.ttSeries}/\n");
                    SetStatus($"Moved {imdbParts.FolderName} {imdbParts.Series} => https://www.imdb.com/title/{imdbParts.ttSeries}/");
                }
            }

            SetStatus(Severity.Success, "Completed.");
        }

        private static List<string> OrphanedMovieList(string rootFolder)
        {
            var list = new List<string>();
            //Enumerate all videos. Ignoring all videos with matching shortcuts.
            foreach (var f in Directory.EnumerateFiles(rootFolder, "*.*", SearchOption.AllDirectories))
            {
                if (MovieProperties.MovieExtensions.IndexOf(Path.GetExtension(f)) == -1) continue;
                var folder = Path.GetDirectoryName(f);
                var urlFound = false;
                foreach (var f2 in Directory.EnumerateFiles(folder, "*.url", SearchOption.TopDirectoryOnly))
                {
                    var link = MovieProperties.GetUrlFromLink(f2);
                    if (link == null) continue;
                    if (link.ContainsI("imdb.com/title/tt")) { urlFound = true; break; }
                }
                if (urlFound) continue;

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
        /// <remarks>
        /// IMDB search failures:
        ///   Babylon.A.D.2008.1080p.BrRip.x264.YIFY.mp4 => "Babylon A D (2008)" != "Babylon A.D. (2008)"
        ///   Dont.Look.Deeper.S01E01.720p.QUIBI.WEBRip.x264-GalaxyTV.mkv => "Dont Look Deeper" != "Don't Look Deeper"
        /// </remarks>
        /// <param name="movieFileName">Full filename of video file.</param>
        /// <returns>IMDB url or null if not found</returns>
        private ImdbParts FindImdbUrl(string movieFileFullPath)
        {
            var imdbParts = new ImdbParts();

            MatchCollection mc;
            var tempFileName = Path.Combine(Path.GetTempPath(), "FindUrl.htm");
            var movieFileName = Path.GetFileName(movieFileFullPath);

            //-------------------------------------------------------------
            //Use https://regex101.com to validate the regex patterns. 
            //See pattern match testing: https://regex101.com/r/aK6sB5/1
            //-------------------------------------------------------------

            //Find TV Series episode url. Contains TV series name + season and episode (No year!)
            mc = Regex.Matches(movieFileName, @"^(?<NAME>.+?)(?<SERIES>S(?<S>[0-9]{2,2})E(?<E>[0-9]{2,2}))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (mc.Count > 0)
            {
                var name = mc[0].Groups["NAME"].Value.Replace('.', ' ').Trim();
                var season = int.Parse(mc[0].Groups["S"].Value);
                var episode = int.Parse(mc[0].Groups["E"].Value);
                var series = mc[0].Groups["SERIES"].Value;
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

                var job = new FileEx.Job(null, $"https://www.imdb.com/search/title/?title={name}&title_type=tv_series&view=simple", tempFileName);
                if (FileEx.Download(job))
                {
                    var html2 = FileEx.ReadHtml(job.Filename);
                    File.Delete(job.Filename); //no longer needed.

                    //<a href="/title/tt0796264/?ref_=adv_li_tt">Eureka</a><span class="lister-item-year text-muted unbold">(2006–2012)</span>
                    mc = Regex.Matches(html2, @"<span class='lister-item-index.+<a href='\/title\/(?<TT>tt[0-9]+)\/.+?>(?<NAME>[^<]+)<\/a><.+?lister-item-year .+?>(?<YEAR>[^<]+)<\/span>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if (mc.Count == 0)
                    {
                        TVSeries[name] = null;
                        SetStatus(Severity.Error, $"No match for series \"{name}\".");
                        return null;
                    }

                    var fname = FixFilename(mc[0].Groups["NAME"].Value);
                    var fname2 = fname;
                    if (fname2.StartsWith("The ", StringComparison.OrdinalIgnoreCase)) fname2 = fname2.Substring(4) + ", The";
                    else if (fname2.StartsWith("A ", StringComparison.OrdinalIgnoreCase)) fname2 = fname2.Substring(2) + ", A";

                    TVSeries[name + ".MOVIENAME"] = string.Concat(fname, " ", mc[0].Groups["YEAR"].Value);
                    TVSeries[name + ".FOLDERNAME"] = string.Concat(fname2, " ", mc[0].Groups["YEAR"].Value);
                    TVSeries[name] = mc[0].Groups["TT"].Value;

                    job = new FileEx.Job(null, $"https://www.imdb.com/title/{mc[0].Groups["TT"].Value}/episodes?season={season}", tempFileName);
                    if (FileEx.Download(job))
                    {
                        html2 = FileEx.ReadHtml(job.Filename);
                        File.Delete(job.Filename); //no longer needed. extension already used for this cache file.

                        //<a href='/title/tt1060050/?ref_=ttep_ep13' title='A Night in Global Dynamics' itemprop='name'>A Night in Global Dynamics</a>
                        var mc2 = Regex.Matches(html2, @"<a href='\/title\/(?<TT>tt[0-9]+)\/\?ref_=ttep_ep(?<EP>[0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                        if (mc2.Count == 0)
                        {
                            SetStatus(Severity.Error, $"Unable to find any {fname} season {season} episodes.");
                            return null;
                        }

                        foreach (Match m in mc2)
                        {
                            int.TryParse(m.Groups["EP"].Value, out int ep);
                            TVSeries[$"{name}.S{season:00}E{ep:00}"] = m.Groups["TT"].Value;
                        }
                    }
                    else
                    {
                        SetStatus(Severity.Error, $"Unable to reach IMDB episodes for {movieFileName}");
                        return null;
                    }
                }
                else
                {
                    SetStatus(Severity.Error, $"Unable to reach IMDB for {movieFileName}");
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

                SetStatus(Severity.Error, $"Missing {string.Concat(name, " ", series)}");
                return null;
            }

            //Find feature movie url. Contains movie name and release year.
            mc = Regex.Matches(movieFileName, @"^(?<NAME>.+?)(?<YEAR>[0-9]{4,4})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (mc.Count > 0)
            {
                var name = mc[0].Groups["NAME"].Value.Replace('.', ' ').Trim();
                if (int.TryParse(mc[0].Groups["YEAR"].Value, out int year) && year <= DateTime.Now.Year && year > 1900) name = $"{name} ({year})";

                var job = new FileEx.Job(null, $"https://www.imdb.com/find?q={name}&s=tt&exact=true", tempFileName);
                if (FileEx.Download(job))
                {
                    var html2 = FileEx.ReadHtml(job.Filename);
                    File.Delete(job.Filename); //no longer needed.
                    //<td class='result_text'><a href='/title/tt0796264/?ref_=fn_tt_tt_1'>Eureka</a>(2006)</td>
                    mc = Regex.Matches(html2, @"class='result_text'><a href='\/?title\/(?<TT>tt[0-9]+)\/[^>]+>(?<NAME>[^<]+)<\/a>.*?(?<YEAR>[0-9]{4,4})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if (mc.Count == 0)
                    {
                        SetStatus(Severity.Error, $"No match for \"{name}\".");
                        return null;
                    }

                    name = FixFilename(mc[0].Groups["NAME"].Value);
                    imdbParts.MovieName = string.Concat(name, " (", mc[0].Groups["YEAR"].Value,")");
                    if (name.StartsWith("The ", StringComparison.OrdinalIgnoreCase)) name = name.Substring(4) + ", The";
                    else if (name.StartsWith("A ", StringComparison.OrdinalIgnoreCase)) name = name.Substring(2) + ", A";
                    imdbParts.FolderName = string.Concat(name, " (", mc[0].Groups["YEAR"].Value,")");
                    imdbParts.ttMovie = mc[0].Groups["TT"].Value; //use index 0 as the first is likely correct.
                    return imdbParts;
                }
                else
                {
                    SetStatus(Severity.Error, $"Unable to reach IMDB for {movieFileName}");
                    return null;
                }
            }

            SetStatus(Severity.Error, $"Unable to parse {movieFileName}");
            return null;
        }

        //Remove illegal file chars from downloaded movie name.
        private static readonly Regex FixFilenameRE = new Regex($"[{Regex.Escape(new String(Path.GetInvalidFileNameChars()))}]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static string FixFilename(string fnameNotPath)
        {
            return FixFilenameRE.Replace(fnameNotPath, "_");
        }

        private class ImdbParts
        {
            //The.Time.Machine.1992.1080p.WebRip.h264.AAC-ripper.mkv
            //Glass 2019 1080p BluRay x264-DTS [MW].mkv
            //Eureka.S01E01.720p.BluRay.X265-REWARD.mkv
            public string FolderName { get; set; } //Movie or TVSeries full name w/"The" or "A" moved to end for sorting purposes (e.g. "Time Machine, The (1992)" or "Eureka (2006–2012)"
            public string MovieName { get; set; }  //Movie or TVSeries full name (e.g. "The Time Machine (1992)" or "Eureka (2006–2012)"
            public string ttMovie { get; set; }    //Movie or TVSeries TT code e.g. "tt1234567"
            public string Series { get; set; }     //Series episode e.g. "S01E01"
            public string ttSeries { get; set; }   //Episode TT code e.g. "tt1234567"
        }
    }
}
