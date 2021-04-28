//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="SettingsDialog.cs" company="Chuck Hill">
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
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace VideoLibrarian
{
    public partial class SettingsDialog : Form
    {
        private ToolTipHelp tt;
        private SettingProperties OriginalSettings;
        private SettingProperties NewSettings = null;

        public static SettingProperties Show(IWin32Window owner, SettingProperties settings = null)
        {
            using (var dlg = new SettingsDialog(settings))
            {
                if (dlg.ShowDialog(owner) == DialogResult.OK)
                {
                    return dlg.NewSettings;
                }
                return null;
            }
        }

        private SettingsDialog(SettingProperties settings)
        {
            OriginalSettings = settings;
            InitializeComponent();
            tt = new ToolTipHelp(this);

            if (settings == null) return;

            if (settings.MediaFolders != null && settings.MediaFolders.Length > 0)
                m_lstFolders.Items.AddRange(settings.MediaFolders);

            if (!settings.Browser.IsNullOrEmpty())
                m_txtBrowser.Text = settings.Browser;

            if (!settings.VideoPlayer.IsNullOrEmpty())
                m_txtPlayer.Text = settings.VideoPlayer;

            // Example: Start Notepad++ at EOF and don't save session at exit.
            // C:\Program Files\Notepad++\notepad++.exe -nosession -n2147483647
            // 2147483647 == 0x7FFFFFFF == int.MaxValue

            if (!settings.LogViewer.IsNullOrEmpty())
                m_txtLogViewer.Text = settings.LogViewer;
        }

        private void m_btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None; //default to NOT close the dialog.
            NewSettings = new SettingProperties();
            NewSettings.MediaFolders = new string[m_lstFolders.Items.Count];
            int i = 0;
            foreach (string d in m_lstFolders.Items) NewSettings.MediaFolders[i++] = d;

            var exe = m_txtBrowser.Text.Trim();
            if (!ValidateExe(exe)) { m_txtBrowser.Focus(); return; }
            NewSettings.Browser = exe;

            exe = m_txtPlayer.Text.Trim();
            if (!ValidateExe(exe)) { m_txtPlayer.Focus(); return; }
            NewSettings.VideoPlayer = exe;

            exe = m_txtLogViewer.Text.Trim();
            if (!ValidateExe(exe)) { m_txtLogViewer.Focus(); return; }
            NewSettings.LogViewer = exe;

            this.DialogResult = DialogResult.OK;
            //this.Close(); close is implicit.
        }

        private bool ValidateExe(string exe)
        {
            if (exe.Length > 0)
            {
                var s = ProcessEx.SplitProcessCommandline(exe);
                if (s[0].Length < 5 || !File.Exists(s[0]))
                {
                    MessageBox.Show(this, "Executable not found: " + s[0], "Validate Executable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private void m_btnCancel_Click(object sender, EventArgs e)
        {
            this.NewSettings = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void m_btnAdd_Click(object sender, EventArgs e)
        {
            var dir = FolderSelectDialog.Show(this, "Select Movie/Media Folder", m_lstFolders.SelectedItem as string);
            if (dir == null) return;
            foreach (string item in m_lstFolders.Items)
            {
                if (item.Equals(dir, StringComparison.CurrentCultureIgnoreCase)) return;
            }
            m_lstFolders.Items.Add(dir);
        }

        private void m_btnRemove_Click(object sender, EventArgs e)
        {
            var index = m_lstFolders.SelectedIndex;
            if (index < 0) return;
            m_lstFolders.Items.RemoveAt(index);
        }

        private void m_btnBrowser_Click(object sender, EventArgs e)
        {
            var filename = m_txtBrowser.Text;
            filename = ExeSelector(filename, "Select Browser Executable");
            if (filename == null) return;
            m_txtBrowser.Text = filename;
        }

        private void m_btnPlayer_Click(object sender, EventArgs e)
        {
            var filename = m_txtPlayer.Text;
            filename = ExeSelector(filename, "Select Video Player Executable");
            if (filename == null) return;
            m_txtPlayer.Text = filename;
        }

        private void m_btnLogViewer_Click(object sender, EventArgs e)
        {
            var filename = m_txtLogViewer.Text;
            filename = ExeSelector(filename, "Select Text Log Viewer Executable");
            if (filename == null) return;
            m_txtLogViewer.Text = filename;
        }

        private string ExeSelector(string filename, string title)
        {
            int idx; //strip command-line arguments
            filename = !filename.IsNullOrEmpty() && (idx = filename.IndexOf(".exe", 0, StringComparison.CurrentCultureIgnoreCase)) > 0 ? filename.Substring(0, idx + 4) : string.Empty;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = $"Executables(*.exe)|*.exe|All Files(*.*)|*.*";
                ofd.FileName = filename.IsNullOrEmpty() || !File.Exists(filename) ? String.Empty : filename;
                ofd.AddExtension = false;
                ofd.CheckFileExists = true;
                ofd.DereferenceLinks = true;
                ofd.Multiselect = false;
                ofd.InitialDirectory = filename.IsNullOrEmpty() || !Directory.Exists(Path.GetDirectoryName(filename)) ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : Path.GetDirectoryName(filename);
                ofd.RestoreDirectory = true;
                ofd.Title = title;
                ofd.ValidateNames = true;
                ofd.AutoUpgradeEnabled = false;

                if (ofd.ShowDialog(this) != DialogResult.OK) return null;
                return ofd.FileName;
            }

        }

        private void m_txtExecutable_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (File.Exists(file) && file.EndsWith(".exe",StringComparison.CurrentCultureIgnoreCase))
                {
                    ((Control)sender).Text = file;
                }
            }
        }

        private void m_txtExecutable_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (File.Exists(file) && file.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }


        }

        private List<string> AvailableBrowsers()
        {
            string[] possibleBrowsers = new string[]
            {
                @"Mozilla Firefox\firefox.exe",
                @"Google\Chrome\Application\chrome.exe",
                @"Microsoft\Edge\Application\msedge.exe",
                @"Internet Explorer\iexplore.exe",
                @"Opera\Launcher.exe", //launches Opera\29.86113\Opera.exe
                @"yandex\yandexbrowser\application\browser.exe",
                @"BraveSoftware\brave.exe",
                @"Vivaldi\Application\vivaldi.exe",
                @"Maxthon5\Bin\MxService.exe"
            };

            var browsers = new List<string>();

            var pathPrefixes = new List<string>();
            pathPrefixes.Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            pathPrefixes.Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            pathPrefixes.Add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            if (pathPrefixes[0].EqualsI(pathPrefixes[1])) pathPrefixes.RemoveAt(1);

            foreach (var b in possibleBrowsers)
            {
                foreach (var prefix in pathPrefixes)
                {
                    string browser = Path.Combine(prefix, b);
                    if (File.Exists(browser)) { browsers.Add(browser); break; }
                }
            }

            return browsers;
        }

        private static List<string> KnownBrowsers = null; //populate upon demand.
        private void m_cmbBrowser_DropDown(object sender, EventArgs e)
        {
            if (KnownBrowsers == null) KnownBrowsers = AvailableBrowsers();
            if (m_cmbBrowser.DataSource == null) m_cmbBrowser.DataSource = KnownBrowsers;

            var filename = m_txtBrowser.Text;
            int idx; //strip command-line arguments
            filename = !filename.IsNullOrEmpty() && (idx = filename.IndexOf(".exe", 0, StringComparison.CurrentCultureIgnoreCase)) > 0 ? filename.Substring(0, idx + 4) : string.Empty;

            if ((filename = KnownBrowsers.FirstOrDefault(m => m.EqualsI(filename))) != null)
                m_cmbBrowser.SelectedItem = filename;
            else m_cmbBrowser.SelectedIndex = -1;
        }

        private void m_cmbBrowser_SelectionChangeCommitted(object sender, EventArgs e)
        {
            m_txtBrowser.Text = m_cmbBrowser.SelectedItem.ToString();
        }
    }
}
