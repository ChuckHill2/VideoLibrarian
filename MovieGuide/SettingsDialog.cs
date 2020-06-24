using System;
using System.Windows.Forms;

namespace MovieGuide
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
