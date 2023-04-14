//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ManualVideoConfig.cs" company="Chuck Hill">
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VideoLibrarian;

namespace VideoOrganizer
{
    public partial class ManualVideoConfig : Form
    {
        private ToolTipHelp _tt; //Tooltip help manager
        private string RootFolder;
        private MovieProperties _mp; //Save loaded properties for change comparison during Save.
        private static readonly DateTime ControlMinDate = new DateTime(1900, 1, 1); //DateTimePicker does not support DateTime.MinValue!
        private long MovieFileLength = 0L;
        private Guid MovieHash = Guid.Empty;

        /// <summary>
        /// Opens the movie properties editor
        /// </summary>
        /// <param name="owner">Parent form</param>
        /// <param name="defaultFolder">The root folder used as the default folder for the open directory and open file dialogs. May be null.</param>
        /// <param name="destFile">The explicit folder or file (video or xml movie properties file) that contains (or will contain) the movie properties file. May be null.</param>
        public static void Show(IWin32Window owner, string defaultFolder, string destFile = null)
        {
            using (var dlg = new ManualVideoConfig(defaultFolder, destFile))
            {
                dlg.ShowDialog(owner);
            }
        }

        private ManualVideoConfig(string defaultFolder, string destFile)
        {
            InitializeComponent();
            _tt = new ToolTipHelp(this); //must be after InitializeComponent()

            m_pnlAllProperties.Enabled = false;
            m_grpEpisode.Visible = false;
            m_grpSeries.Visible = false;
            m_dtWatched.MaxDate = DateTime.Now.AddDays(1).Date;
            m_dtReleaseDate.MaxDate = DateTime.Now.AddYears(1).Date;
            m_dtWatched.Visible = false;

            m_lblDownloadDate.Text = DateTime.MinValue.ToString("g");
            m_lblAspectRatio.Text = "0:0";
            m_lblDimensions.Text = "0 x 0 pixels";
            m_lblRuntime.Text = "0 minutes (0:00)";

            if (destFile.IsNullOrEmpty())
            {
                m_txtMoviePath.Text = defaultFolder;
                RootFolder = defaultFolder;
            }
            else if (Directory.Exists(destFile))
            {
                m_txtMoviePath.Enabled = false;
                m_txtMoviePath.AllowDrop = false;
                m_btnSelectMovieFile.Enabled = false;
                m_btnSelectMovieFolder.Enabled = false;

                var f2 = Directory.EnumerateFiles(destFile).FirstOrDefault(f => MovieProperties.IsVideoFile(f) || MovieProperties.IsPropertiesFile(f));
                if (f2 == null)
                {
                    m_txtMoviePath.Text = destFile;
                    RootFolder = destFile;
                }
                else
                {
                    m_txtMoviePath.Text = f2;
                    RootFolder = Path.GetDirectoryName(f2);
                }

                LoadDialog(true);
                return;
            }
            else if (MovieProperties.IsVideoFile(destFile) || MovieProperties.IsPropertiesFile(destFile))
            {
                m_txtMoviePath.Text = destFile;
                m_txtMoviePath.Enabled = false;
                m_txtMoviePath.AllowDrop = false;
                m_btnSelectMovieFile.Enabled = false;
                m_btnSelectMovieFolder.Enabled = false;
                RootFolder = Path.GetDirectoryName(destFile);
                LoadDialog(true);
            }
        }

        private void m_btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void m_btnSave_Click(object sender, EventArgs e)
        {
            if (SaveDialog())
                this.Close();
        }

        private void m_btnSelectMovieFolder_Click(object sender, EventArgs e)
        {
            var initialDirectory = RootFolder.IsNullOrEmpty() || !Directory.Exists(RootFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) : RootFolder;
            var dir = FolderSelectDialog.Show(this, "Select TV Series Root Folder", initialDirectory);
            if (dir == null) return;
            HideError();
            m_txtMoviePath.Enabled = false;
            m_txtMoviePath.AllowDrop = false;
            m_btnSelectMovieFile.Enabled = false;
            m_btnSelectMovieFolder.Enabled = false;

            var f2 = Directory.EnumerateFiles(dir).FirstOrDefault(f => MovieProperties.IsVideoFile(f) || MovieProperties.IsPropertiesFile(f));
            if (f2 == null)
            {
                m_txtMoviePath.Text = dir;
                RootFolder = dir;
            }
            else
            {
                m_txtMoviePath.Text = f2;
                RootFolder = Path.GetDirectoryName(f2);
            }

            LoadDialog(true);
        }

        private void m_btnSelectMovieFile_Click(object sender, EventArgs e)
        {
            var c = (Control)sender;

            string filename;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = $"All Video Formats|{MovieProperties.MovieExtensions.Replace("|", ";*").Substring(1, MovieProperties.MovieExtensions.Length - 2)}" +
                    "|All Files(*.*)|*.*";
                ofd.AddExtension = false;
                ofd.CheckFileExists = true;
                ofd.DereferenceLinks = true;
                ofd.Multiselect = false;
                ofd.InitialDirectory = RootFolder.IsNullOrEmpty() || !Directory.Exists(RootFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) : RootFolder;
                ofd.RestoreDirectory = true;
                ofd.Multiselect = false;
                ofd.Title = "Select Video File to Configure";
                ofd.ValidateNames = true;
                ofd.AutoUpgradeEnabled = false;

                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                filename = ofd.FileName;
            }

            if (MovieProperties.IsVideoFile(filename) ||
                (Path.GetExtension(filename).EqualsI(".xml") && Path.GetFileNameWithoutExtension(filename).StartsWith("tt")))
            {
                HideError();
                m_txtMoviePath.Text = filename;
                m_txtMoviePath.Enabled = false;
                m_txtMoviePath.AllowDrop = false;
                m_btnSelectMovieFile.Enabled = false;
                m_btnSelectMovieFolder.Enabled = false;
                RootFolder = Path.GetDirectoryName(filename);
                LoadDialog(true);
                return;
            }

            ShowError(c, "Only video files or associated xml files are allowed.");
        }

        private void m_txtMoviePath_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            HideError();

            if (Directory.Exists(file))
            {
                m_txtMoviePath.Enabled = false;
                m_txtMoviePath.AllowDrop = false;
                m_btnSelectMovieFile.Enabled = false;
                m_btnSelectMovieFolder.Enabled = false;

                var f2 = Directory.EnumerateFiles(file).FirstOrDefault(f => MovieProperties.IsVideoFile(f) || MovieProperties.IsPropertiesFile(f));
                if (f2 == null)
                {
                    m_txtMoviePath.Text = file;
                    RootFolder = file;
                }
                else
                {
                    m_txtMoviePath.Text = f2;
                    RootFolder = Path.GetDirectoryName(f2);
                }

                LoadDialog(true);
                return;
            }

            if (MovieProperties.IsVideoFile(file) || MovieProperties.IsPropertiesFile(file))
            {
                m_txtMoviePath.Text = file;
                m_txtMoviePath.Enabled = false;
                m_txtMoviePath.AllowDrop = false;
                m_btnSelectMovieFile.Enabled = false;
                m_btnSelectMovieFolder.Enabled = false;
                RootFolder = Path.GetDirectoryName(file);
                LoadDialog(true);
                return;
            }
        }

        private void m_txtMoviePath_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (Directory.Exists(file) || MovieProperties.IsVideoFile(file) || MovieProperties.IsPropertiesFile(file))
            {
                e.Effect = DragDropEffects.Link;
            }
        }

        private void m_txtMoviePath_KeyPress(object sender, KeyPressEventArgs e)
        {
            var c = (TextBox)sender;
            if (e.KeyChar != '\r' && e.KeyChar != '\n') return;
            e.Handled = true;
            var file = c.Text;

            HideError();

            if (Directory.Exists(file))
            {
                m_txtMoviePath.Text = file;
                m_txtMoviePath.Enabled = false;
                m_txtMoviePath.AllowDrop = false;
                m_btnSelectMovieFile.Enabled = false;
                m_btnSelectMovieFolder.Enabled = false;
                RootFolder = file;
                LoadDialog(true);
                return;
            }

            if (MovieProperties.IsVideoFile(file))
            {
                m_txtMoviePath.Text = file;
                m_txtMoviePath.Enabled = false;
                m_txtMoviePath.AllowDrop = false;
                m_btnSelectMovieFile.Enabled = false;
                m_btnSelectMovieFolder.Enabled = false;
                RootFolder = Path.GetDirectoryName(file);
                LoadDialog(true);
                return;
            }

            ShowError(c, "Not a folder or video file path.");
        }

        private void m_txtImdbUrl_DragDrop(object sender, DragEventArgs e)
        {
            var c = (TextBox)sender;

            if (e.Data.GetDataPresent("UniformResourceLocator"))
            {
                var file = ((string)e.Data.GetData(typeof(string)));
                if (!file.StartsWith("https://www.imdb.com/title/tt")) return;
                var i = file.IndexOf('?');
                if (i != -1) file = file.Substring(0, i);
                c.Text = file;
                return;
            }
        }

        private void m_txtImdbUrl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("UniformResourceLocator"))
            {
                //Warning Valid UniformResourceLocator prefix: file:///
                var file = ((string)e.Data.GetData(typeof(string)));
                if (!file.StartsWith("https://www.imdb.com/title/tt")) return;
                e.Effect = DragDropEffects.Link;
                return;
            }
        }

        private void m_txtImdbUrl_Leave(object sender, EventArgs e)
        {
            var c = (TextBox)sender;
            if (c.Text.IsNullOrEmpty()) { HideError(); return; }
            var file = c.Text;

            HideError();

            if (file.StartsWith("http"))
            {
                try { var uri = new Uri(file); }
                catch { ShowError(c, "Must contain a valid HTTPS url."); return; }
                if (!file.StartsWith("https://www.imdb.com/title/tt"))
                {
                    ShowError(c, "Must contain a valid IMDB movie title url.");
                    return;
                }
                var i = file.IndexOf('?');
                if (i != -1)
                {
                    file = file.Substring(0, i);
                    c.Text = file;
                }
                return;
            }
            else { ShowError(c, "Must contain a valid HTTPS url."); return; }
        }

        private void m_txtPosterUrl_DragDrop(object sender, DragEventArgs e)
        {
            var c = (TextBox)sender;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (!MovieProperties.IsImageFile(file)) return;
                if (file.StartsWith(Path.GetDirectoryName(file) + "\\tt")) return;
                c.Text = new Uri(file).ToString();
                return;
            }
            if (e.Data.GetDataPresent("UniformResourceLocator"))
            {
                var file = ((string)e.Data.GetData(typeof(string)));
                if (!MovieProperties.IsImageFile(file)) return;
                c.Text = file;
                return;
            }
        }

        private void m_txtPosterUrl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (!MovieProperties.IsImageFile(file)) return;
                if (file.StartsWith(Path.GetDirectoryName(file) + "\\tt")) return;
                e.Effect = DragDropEffects.Link;
                return;
            }
            if (e.Data.GetDataPresent("UniformResourceLocator"))
            {
                var file = ((string)e.Data.GetData(typeof(string)));
                if (!MovieProperties.IsImageFile(file)) return;
                e.Effect = DragDropEffects.Link;
                return;
            }
        }

        private void m_txtPosterUrl_Leave(object sender, EventArgs e)
        {
            var c = (TextBox)sender;
            if (c.Text.IsNullOrEmpty()) { HideError(); return; }
            var file = c.Text;

            if (file.StartsWith("http"))
            {
                try { var uri = new Uri(file); }
                catch { ShowError(c, "Must contain a valid url."); return; }
                if (!MovieProperties.IsImageFile(file))
                {
                    ShowError(c, "Must contain a valid image url.");
                    return;
                }
                return;
            }

            if (file.StartsWith("file:///"))
            {
                try { var uri = new Uri(file); file = uri.LocalPath; }
                catch { ShowError(c, "Must contain a valid image\nurl or image filename."); return; }
            }

            try
            {
                if (!MovieProperties.IsImageFile(file))
                {
                    ShowError(c, "Image file not found.");
                    return;
                }
                if (file.StartsWith(Path.GetDirectoryName(file) + "\\tt"))
                {
                    ShowError(c, "Image file must not originate in\nthe same directory as the video.");
                    return;
                }

                c.Text = new Uri(file).ToString();
            }
            catch
            {
                ShowError(c, "Invalid file format.");
                return;
            }

            c.Text = new Uri(file).ToString();
            HideError();
        }

        private void m_btnDownloadWebpage_Click(object sender, EventArgs e)
        {
            if (!m_chkImdbUrl.Checked) return;
            var url = m_txtImdbUrl.Text.Trim();
            if (url.IsNullOrEmpty()) { ShowError(m_txtImdbUrl, "IMDB Url is empty."); return; }

            var tt = MovieProperties.GetTitleId(url);
            if (tt.IsNullOrEmpty()) { ShowError(m_txtImdbUrl, "IMDB Url is invalid."); return; }

            var tempExt = $".X{Environment.TickCount}X"; //create unique temporary backup extension
            Directory.GetFiles(RootFolder, "*.url").ForEach(f => FileEx.Move(f, f + tempExt)); //backup other shortcuts to avoid conflicts
            var shortcutPath = Path.Combine(RootFolder, "TEMP.url");  //Create temp shortcut. Will delete upon success.
            MovieProperties.CreateTTShortcut(shortcutPath, tt);

            var propertiesPath = Path.Combine(RootFolder, tt + ".xml"); //backup cache xml file to force MovieProperties to recreate it.
            if (FileEx.Exists(propertiesPath)) FileEx.Move(propertiesPath, propertiesPath + tempExt);

            MovieProperties mpx;
            try 
            { 
                mpx = new MovieProperties(RootFolder);
            }
            catch (Exception ex) 
            {
                //restore backed up properties and show exception
                DeleteFile(propertiesPath);
                if (FileEx.Exists(propertiesPath + tempExt)) FileEx.Move(propertiesPath + tempExt, propertiesPath);
                ShowError(m_txtImdbUrl, ex.Message); 
                return; 
            }

            //Success: Delete backup cache xml file.
            DeleteFile(propertiesPath + tempExt);

            LoadDialog(false);
            _mp = mpx;
            DeleteFile(shortcutPath);  //We create the real IMDB shortcuts upon save.
            Directory.GetFiles(RootFolder, "*" + tempExt).ForEach(f =>   //Restore other shortcuts
            { 
                var f2 = f.Substring(0, f.Length - tempExt.Length);
                if (FileEx.Exists(f2)) DeleteFile(f); else FileEx.Move(f, f2); 
            });
        }

        private void m_btnDownloadPoster_Click(object sender, EventArgs e)
        {
            var posterUrl = m_txtPosterUrl.Text;
            if (posterUrl.IsNullOrEmpty()) { ShowError(m_txtPosterUrl, "Must have poster\nUrl to download."); return; }
            if (posterUrl == _mp.MoviePosterUrl && FileEx.Exists(_mp.MoviePosterPath)) { ToolTipHelp.ShowTempToolTip((Control)sender, "Poster already exists.", ToolTipIcon.Info); return; }

            var pageUrl = m_chkImdbUrl.Checked ? m_txtImdbUrl.Text : new Uri(RootFolder).ToString();
            var tt = MovieProperties.GetTitleId(pageUrl);
            if (tt.IsNullOrEmpty()) { ShowError(m_txtPosterUrl, "Must have IMDB\nUrl to download."); return; }

            var mp2 = new MovieProperties();
            mp2.MoviePosterUrl = posterUrl;
            mp2.PathPrefix = Path.Combine(RootFolder, tt);
            using (var img = mp2.MoviePosterImg) { } //force download of movie poster

            if (!FileEx.Exists(mp2.MoviePosterPath))
            {
                ToolTipHelp.ShowTempToolTip((Control)sender, "Poster image\ndownload FAILED.", ToolTipIcon.Error);
                return;
            }

            if (!_mp.MoviePosterPath.Equals(mp2.MoviePosterPath)) DeleteFile(_mp.MoviePosterPath);
            ToolTipHelp.ShowTempToolTip((Control)sender, "Poster image\nsuccessfully downloaded.", ToolTipIcon.Info);
        }

        private void m_lnkRecompute_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (m_txtMoviePath.Text=="" || Directory.Exists(m_txtMoviePath.Text)) return;

            MiniMessageBox.Show(m_lnkRecompute, "Computing properties...", "Please Wait...", MiniMessageBox.Buttons.None, MiniMessageBox.Symbol.Wait);

            string movieFile = null;
            if (!MovieProperties.IsVideoFile(m_txtMoviePath.Text))
            {
                movieFile = DirectoryEx.EnumerateAllFiles(Path.GetDirectoryName(m_txtMoviePath.Text)).FirstOrDefault(m => MovieProperties.IsVideoFile(m));
                if (movieFile == null)
                {
                    MiniMessageBox.Hide();
                    return;
                }
            }
            else movieFile = m_txtMoviePath.Text;

            var mpx = new MovieProperties();
            mpx.MoviePath = movieFile;
            mpx.GetVideoFileProperties();

            m_lblDownloadDate.Text = mpx.DownloadDate.ToString("g");
            m_lblAspectRatio.Text = mpx.DisplayRatio;
            m_lblDimensions.Text = $"{mpx.DisplayWidth} x {mpx.DisplayHeight} pixels";
            m_lblRuntime.Text = $"{mpx.Runtime} minutes ({mpx.Runtime/60}:{mpx.Runtime % 60})";

            MovieFileLength = mpx.MovieFileLength;
            MovieHash = mpx.MovieHash;
            MiniMessageBox.Hide();
        }

        private void m_clbVideoType_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var c = (CheckedListBox)sender;
            var label = (string)c.Items[e.Index];

            if (e.NewValue == CheckState.Checked)
            {
                //Note: m_grpEpisode & m_grpSeries are children of m_grpSeriesEpisode
                if (label.EndsWith("Series"))
                {
                    m_grpEpisode.Visible = false;
                    m_grpSeries.Visible = true;
                    m_txtEpisodeName.Enabled = false;
                    m_txtCustomGroups.Visible = true;
                    m_btnEditCustomGroups.Visible = true;

                    //The following are children of m_grpExtractedVidProps
                    m_lblDownloadDate.Visible = m_lblDownloadDateLabel.Visible = true;
                    m_lblRuntime.Visible = m_lblRuntimeLabel.Visible = false;
                    m_grpVerticalDivider.Visible = false;
                    m_lblAspectRatio.Visible = m_lblAspectRatioLabel.Visible = false;
                    m_lblDimensions.Visible = m_lblDimensionsLabel.Visible = false;
                }
                else if (label.EndsWith("Episode"))
                {
                    m_grpEpisode.Visible = true;
                    m_grpSeries.Visible = false;
                    m_txtEpisodeName.Enabled = true;
                    m_txtCustomGroups.Visible = false;
                    m_btnEditCustomGroups.Visible = false;

                    m_lblDownloadDate.Visible = m_lblDownloadDateLabel.Visible = true;
                    m_lblRuntime.Visible = m_lblRuntimeLabel.Visible = true;
                    m_grpVerticalDivider.Visible = true;
                    m_lblAspectRatio.Visible = m_lblAspectRatioLabel.Visible = true;
                    m_lblDimensions.Visible = m_lblDimensionsLabel.Visible = true;
                }
                else
                {
                    m_grpEpisode.Visible = false;
                    m_grpSeries.Visible = false;
                    m_txtEpisodeName.Enabled = false;
                    m_txtCustomGroups.Visible = true;
                    m_btnEditCustomGroups.Visible = true;

                    m_lblDownloadDate.Visible = m_lblDownloadDateLabel.Visible = true;
                    m_lblRuntime.Visible = m_lblRuntimeLabel.Visible = true;
                    m_grpVerticalDivider.Visible = true;
                    m_lblAspectRatio.Visible = m_lblAspectRatioLabel.Visible = true;
                    m_lblDimensions.Visible = m_lblDimensionsLabel.Visible = true;
                }
            }

            //Only one may be checked.
            c.ItemCheck -= m_clbVideoType_ItemCheck; //avoid recursion
            foreach (int i in c.CheckedIndices)
            {
                c.SetItemChecked(i, false);
            }
            c.ItemCheck += m_clbVideoType_ItemCheck;
        }

        private void m_chkWatched_CheckedChanged(object sender, EventArgs e)
        {
            var c = (CheckBox)sender;
            m_dtWatched.Visible = c.Checked;
            if (c.Checked && m_dtWatched.Value.Year <= ControlMinDate.Year) m_dtWatched.Value = DateTime.Now.Date;
        }

        private void JustHideError_Leave(object sender, EventArgs e)
        {
            HideError();
        }

        private void m_chkImdbUrl_CheckedChanged(object sender, EventArgs e)
        {
            m_txtImdbUrl.Enabled = m_chkImdbUrl.Checked;
            m_txtImdbUrl.AllowDrop = m_chkImdbUrl.Checked;
            m_btnDownloadWebpage.Enabled = m_chkImdbUrl.Checked;
        }

        private void TextBoxRun_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //for m_txtPosterUrl, m_txtImdbUrl
            var c = (TextBox)sender;
            c.Select(0, 0);
            if (c.Text.IsNullOrEmpty()) { ToolTipHelp.ShowTempToolTip((Control)sender, "Value empty. Nothing to view.", ToolTipIcon.Info); return; };
            Process.Start(c.Text);
        }

        private void m_btnEditCustomGroups_Click(object sender, EventArgs e)
        {
            m_txtCustomGroups.Text = CustomGroupsEditor.Show(this, m_txtCustomGroups.Text);
        }

        /// <summary>
        /// Populate all dialog controls. 
        /// </summary>
        /// <param name="justLoadAvailable">True to NOT go to internet for missing properties</param>
        private void LoadDialog(bool justLoadAvailable)
        {
            m_pnlAllProperties.Enabled = true;
            var allVideoTypes = m_clbVideoType.Items.OfType<string>().ToArray();

            if (Directory.Exists(m_txtMoviePath.Text))
            {
                var videoFile = DirectoryEx.EnumerateAllFiles(RootFolder).FirstOrDefault(f => MovieProperties.IsVideoFile(f));
                var propFile = DirectoryEx.EnumerateAllFiles(RootFolder).FirstOrDefault(f => MovieProperties.IsPropertiesFile(f));
                //If the movie path is a directory without a video, then it must be a Series folder.
                if (videoFile == null && propFile==null)
                {

                    m_clbVideoType.Items.Clear();
                    m_clbVideoType.Items.AddRange(new[] { "TV Mini Series", "TV Series" });

                    MiniMessageBox.ShowDialog(this, "Selecting a folder without a video assumes that this is a TV Series root folder and must not contain a video file.",
                        "Selected Series Folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            //Load any existing properties.
            try { _mp = new MovieProperties(RootFolder, false, justLoadAvailable); }
            catch(InvalidDataException)
            {
                //No properties to load, so we're done loading.
                _mp = new MovieProperties();
                return;
            }
            catch (Exception ex) 
            {
                //Data load error.
                MiniMessageBox.ShowDialog(this, ex.Message, "Existing Movie Properties", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _mp = new MovieProperties();
                return; 
            }

            m_clbGenre.CheckedIndices.OfType<int>().ForEach(ii => m_clbGenre.SetItemChecked(ii, false)); //clear in case of reload

            int i;
            foreach (var g in _mp.Genre)
            {
                i = m_clbGenre.Items.IndexOf(g);
                if (i == -1) continue;
                m_clbGenre.SetItemChecked(i, true);
            }

            if (_mp.MovieClass == "TV Mini-Series") _mp.MovieClass = "TV Mini Series"; //Standardize name with en-US IMDB name.

            //Not a series after all
            if (_mp.MovieClass != "TV Mini Series" && _mp.MovieClass != "TV Series" && m_clbVideoType.Items.Count==2)
            {
                m_clbVideoType.Items.Clear();
                m_clbVideoType.Items.AddRange(allVideoTypes);
            }

            i = m_clbVideoType.Items.IndexOf(_mp.MovieClass);
            if (i != -1) m_clbVideoType.SetItemChecked(i, true);

            if (!_mp.MovieName.IsNullOrEmpty())
             {
                var names = _mp.MovieName.Split(new[] { " \xAD " }, StringSplitOptions.RemoveEmptyEntries);
                m_txtMovieName.Text = names.Length > 0 ? names[0] : "";
                m_txtEpisodeName.Text = names.Length > 1 ? names[1] : "";
            }

            if (_mp.MovieClass.EndsWith("Series")) m_numEpisodeCount.Value = _mp.EpisodeCount;
            if (_mp.MovieClass.EndsWith("Episode"))
            {
                m_numSeason.Value = _mp.Season;
                m_numEpisode.Value = _mp.Episode;
            }

            m_dtReleaseDate.Value = _mp.ReleaseDate < ControlMinDate ? DateTime.Now.Date : _mp.ReleaseDate;
            m_numRating.Value = (decimal)_mp.MovieRating;
            m_chkImdbUrl.Checked = _mp.UrlLink.StartsWith("http");
            m_txtImdbUrl.Text = _mp.UrlLink.StartsWith("http") ? new Uri(_mp.UrlLink).ToString() : "";
            m_txtPosterUrl.Text = _mp.MoviePosterUrl.IsNullOrEmpty() ? "" : new Uri(_mp.MoviePosterUrl).ToString();
            m_txtPlot.Text = _mp.Plot;
            m_txtSummary.Text = _mp.Summary;
            m_txtCreators.Text = _mp.Creators;
            m_txtDirectors.Text = _mp.Directors;
            m_txtWriters.Text = _mp.Writers;
            m_txtCast.Text = _mp.Cast;

            m_chkWatched.Checked = _mp.Watched > ControlMinDate;
            m_dtWatched.Value = _mp.Watched > ControlMinDate ? _mp.Watched : ControlMinDate;

            m_lblDownloadDate.Text = _mp.DownloadDate.ToString("g");
            m_lblAspectRatio.Text = _mp.DisplayRatio;
            m_lblDimensions.Text = $"{_mp.DisplayWidth} x {_mp.DisplayHeight} pixels";
            m_lblRuntime.Text = $"{_mp.Runtime} minutes ({_mp.Runtime / 60}:{_mp.Runtime % 60})";

            MovieFileLength = _mp.MovieFileLength;
            MovieHash = _mp.MovieHash;

            m_txtCustomGroups.Text = _mp.CustomGroups ?? "".Trim();
            if (m_txtCustomGroups.Text.Length > 0)
                m_txtCustomGroups.Text = string.Join(";",
                    m_txtCustomGroups.Text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(m => !m.EqualsI(FilterProperties.CustomGroup_Any)));
        }

        /// <summary>
        /// Copy all control values to new MovieProperties object and serialize.
        /// </summary>
        /// <returns>True if successfully saved. false if no change or validation failed.</returns>
        private bool SaveDialog() //returns true if saved
        {
            var mp2 = new MovieProperties();
            bool isSeries = false, isEpisode = false;

            if (m_clbVideoType.CheckedItems.Count != 1) { ShowError(m_clbVideoType, "Must be exactly 1 video type."); return false; }
            if (m_clbGenre.CheckedItems.Count < 1) { ShowError(m_clbGenre, "Must belong to least 1 genre."); return false; }
            if (m_txtMovieName.Text.Trim().IsNullOrEmpty()) { ShowError(m_txtMovieName, "Movie name must not be empty."); return false; }
            if (m_dtReleaseDate.Value.Date <= ControlMinDate) { ShowError(m_dtReleaseDate, "Release Date must be set."); return false; }

            if (m_clbVideoType.CheckedItems.OfType<string>().Any(m => m.EndsWith("Series"))) isSeries = true;
            else if (m_clbVideoType.CheckedItems.OfType<string>().Any(m => m.EndsWith("Episode"))) isEpisode = true;

            mp2.MovieName = isEpisode && !m_txtEpisodeName.Text.IsNullOrEmpty() ? string.Concat(m_txtMovieName.Text.Squeeze(), " \xAD ", m_txtEpisodeName.Text.Squeeze()) : m_txtMovieName.Text.Squeeze();
            mp2.Genre = m_clbGenre.CheckedItems.OfType<string>().ToArray();
            mp2.MovieClass = m_clbVideoType.CheckedItems.OfType<string>().FirstOrDefault();
            mp2.ReleaseDate = m_dtReleaseDate.Value.Date;
            mp2.Year = mp2.ReleaseDate.Year;
            mp2.MovieRating = decimal.ToSingle(m_numRating.Value);
            mp2.Plot = m_txtPlot.Text.Squeeze();
            mp2.Summary = m_txtSummary.Text.Squeeze();
            if (mp2.Plot.IsNullOrEmpty()) mp2.Plot = mp2.Summary;
            if (mp2.Summary.IsNullOrEmpty()) mp2.Summary = mp2.Plot;
            mp2.Creators = m_txtCreators.Text.Squeeze();
            mp2.Directors = m_txtDirectors.Text.Squeeze();
            mp2.Writers = m_txtWriters.Text.Squeeze();
            mp2.Cast = m_txtCast.Text.Squeeze();
            mp2.YearEnd = _mp.YearEnd;
            mp2.Season = isEpisode ? decimal.ToInt32(m_numSeason.Value) : 0;
            mp2.Episode = isEpisode ? decimal.ToInt32(m_numEpisode.Value) : 0;
            mp2.EpisodeCount = isSeries ? decimal.ToInt32(m_numEpisodeCount.Value) : 0;
            mp2.DownloadDate = DateTime.Parse(m_lblDownloadDate.Text);
            if (!isSeries)
            {
                mp2.Runtime = int.Parse(m_lblRuntime.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                mp2.DisplayRatio = m_lblAspectRatio.Text;
                mp2.DisplayWidth = int.Parse(m_lblDimensions.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                mp2.DisplayHeight = int.Parse(m_lblDimensions.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2]);
            }
            mp2.Watched = m_chkWatched.Checked ? m_dtWatched.Value.Date : DateTime.MinValue;

            mp2.MovieFileLength = this.MovieFileLength;
            mp2.MovieHash = this.MovieHash;

            if (m_chkImdbUrl.Checked)
            {
                var url = m_txtImdbUrl.Text;
                if (url.IsNullOrEmpty()) { ShowError(m_txtImdbUrl, "IMDB Url is missing."); return false; }
                var tt = MovieProperties.GetTitleId(url);
                if (tt.IsNullOrEmpty()) { ShowError(m_txtImdbUrl, "IMDB Url is invalid."); return false; }

                var shortcutPath = Path.Combine(RootFolder, mp2.FullMovieName + ".url");

                if (!_mp.UrlLink.EqualsI(url))
                {
                    MovieProperties.CreateTTShortcut(shortcutPath, tt);
                }

                mp2.UrlLink = url;
                mp2.ShortcutPath = shortcutPath;
                mp2.PathPrefix = Path.Combine(RootFolder, tt);
                mp2.MoviePath = m_txtMoviePath.Text;
            }
            else
            {
                var url = new Uri(RootFolder).ToString();
                var shortcutPath = Path.Combine(RootFolder, mp2.FullMovieName + ".url");
                var tt = MovieProperties.EmptyTitleID;

                if (_mp.UrlLink.IsNullOrEmpty() || url != new Uri(_mp.UrlLink).ToString() || !FileEx.Exists(shortcutPath))
                {
                    MovieProperties.CreateTTShortcut(shortcutPath, MovieProperties.EmptyTitleID);
                }

                mp2.UrlLink = url;
                mp2.ShortcutPath = shortcutPath;
                mp2.PathPrefix = Path.Combine(RootFolder, tt);
                //mp2.MoviePath is delibrately undefined because TV Series have no videos.
            }

            mp2.MoviePosterUrl = m_txtPosterUrl.Text.IsNullOrEmpty() ? "" : new Uri(m_txtPosterUrl.Text).ToString();
            mp2.PropertiesPath = mp2.PathPrefix + ".xml";
            mp2.HtmlPath = mp2.PathPrefix + ".htm";
            mp2.MoviePosterPath = mp2.PathPrefix + ".jpg";
            mp2.FolderName = Path.GetFileName(RootFolder);

            if (!mp2.MovieClass.EndsWith("Episode"))
                mp2.CustomGroups = m_txtCustomGroups.Text;

            if (!_mp.Equals(mp2))
            {
                TileBase.PurgeTileImages(RootFolder);
                if (mp2.PropertiesPath != _mp.PropertiesPath) DeleteFile(_mp.PropertiesPath);
                mp2.Serialize();
                _mp = mp2;
            }
            return true;
        }

        private static void DeleteFile(string file)
        {
            //Safely delete a file. Open/locked/permissioned files throw errors. We just log the error...
            if (file.IsNullOrEmpty()) return;
            try
            {
                if (file.StartsWith("file:///")) file = new Uri(file).LocalPath; 
                if (!FileEx.Exists(file)) return;
                FileEx.Delete(file);
            }
            catch(Exception ex)
            {
                Log.Write(Severity.Error, $"Deleting {file}: {ex.Message}");
            }
        }
        private Control LastErrorControl = null;
        private void HideError()
        {
            if (LastErrorControl == null) return;
            DrawErrorBorder(LastErrorControl, SystemPens.Control);
            LastErrorControl = null;
        }
        private void ShowError(Control c, string msg)
        {
            HideError();
            DrawErrorBorder(c, Pens.Red);
            ToolTipHelp.ShowTempToolTip(c, msg, ToolTipIcon.Error);
            c.Focus();
            LastErrorControl = c;
        }
        private void DrawErrorBorder(Control c, Pen pen)
        {
            using(var g = Graphics.FromHwnd(c.Parent.Handle))
            {
                var rc = c.Bounds;
                g.DrawRectangle(pen, rc.X-1, rc.Y-1, rc.Width+1, rc.Height+1);
            }
        }
    }
}
