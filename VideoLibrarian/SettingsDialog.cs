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
using System.Windows.Forms;

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

            if (settings != null && settings.MediaFolders != null && settings.MediaFolders.Length > 0)
            {
                m_lstFolders.Items.AddRange(settings.MediaFolders);
            }
        }

        private void m_btnOK_Click(object sender, EventArgs e)
        {
            NewSettings = new SettingProperties();
            NewSettings.MediaFolders = new string[m_lstFolders.Items.Count];
            int i = 0;
            foreach (string d in m_lstFolders.Items) NewSettings.MediaFolders[i++] = d;

            this.DialogResult = DialogResult.OK;
            this.Close();
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
    }
}
