using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoLibrarian;
using ChuckHill2.Forms;

namespace VideoOrganizer
{
    public partial class CustomGroupsEditor : Form
    {
        private List<string> Groups;
        private List<string> AllKnownGroups;

        public static string Show(IWin32Window owner, string groups)
        {
            using (var dlg = new CustomGroupsEditor(groups))
            {
                dlg.ShowDialog(owner);
                return string.Join(";", dlg.Groups);
            }
        }

        private CustomGroupsEditor(string groups)
        {
            Groups = groups.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).ToList();
            AllKnownGroups = GetKnownGroups();
            InitializeComponent();

            if (Groups.Count > 0) m_lbGroups.Items.AddRange(Groups.ToArray());
            if (AllKnownGroups.Count > 0) m_lbGroups.DropdownItems.AddRange(AllKnownGroups.ToArray());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Groups = ((IEnumerable)m_lbGroups.Items).Cast<string>().Select(m => m.Trim()).Where(m => m.Length > 0).ToList();
            var newAllKnownGroups = ((IEnumerable)m_lbGroups.Items).Cast<string>().Select(m => m.Trim()).Where(m => m.Length > 0).ToList();
            SaveKnownGroups(newAllKnownGroups);
            base.OnClosing(e);
        }

        private List<string> GetKnownGroups()
        {
            var fn = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".CustomGroups.txt");
            if (!FileEx.Exists(fn)) return new List<string>();
            return File.ReadLines(fn).Select(ln =>
            {
                int i = ln.IndexOf('#');
                if (i == -1) return ln.Trim();
                return ln.Substring(0, i).Trim();
            }).Where(ln => ln.Length > 0).ToList();
        }

        private void SaveKnownGroups(List<string> groups)
        {
            if (groups.SequenceEqual(AllKnownGroups)) return; //no change

            var fn = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".CustomGroups.txt");
            using (var sw = new StreamWriter(fn, false))
            {
                sw.WriteLine("# List of all known Custom (aka user-defined) groups used for VideoLibrarian filtering.");
                sw.WriteLine("# This list is used by VideoOrganizer manual video properties editor.");
                sw.WriteLine("# This file is dynamically updated whenever the editor adds a new group.");
                sw.WriteLine("# You may also edit this file and add groups yourself.");
                sw.WriteLine();
                foreach (var g in groups)
                {
                    sw.WriteLine(g);
                }
            }
        }
    }
}
