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
        private static readonly string AllKnownGroupsFilename = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".CustomGroups.txt");
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e); //This must be first so EditListBox may flush any outstanding changes before retrieving it's Items.  Alternatively may use m_lbGroups.Flush().
            Groups = ((IEnumerable)m_lbGroups.Items).Cast<string>().Select(m => m.Trim()).Where(m => m.Length > 0).ToList();
            var newAllKnownGroups = ((IEnumerable)m_lbGroups.DropdownItems).Cast<string>().Select(m => m.Trim()).Where(m => m.Length > 0).ToList();
            SaveKnownGroups(newAllKnownGroups);
        }

        private List<string> GetKnownGroups()
        {
            var groups = new List<string>();
            if (!FileEx.Exists(AllKnownGroupsFilename)) return groups;

            using(var sr = new StreamReader(AllKnownGroupsFilename))
            {
                string line;
                while((line=sr.ReadLine())!=null)
                {
                    int i = line.IndexOf('#');
                    if (i != -1) line = line.Substring(0, i);
                    line = line.Trim();
                    if (line.Length == 0) continue;
                    groups.Add(line);
                }
            }

            return groups;
        }

        private void SaveKnownGroups(List<string> groups)
        {
            if (groups.SequenceEqual(AllKnownGroups)) return; //no change

            using (var sw = new StreamWriter(AllKnownGroupsFilename, false))
            {
                sw.WriteLine("# List of all known Custom (aka user-defined) groups used for VideoLibrarian filtering.");
                sw.WriteLine("# This list is used by VideoOrganizer manual video properties editor.");
                sw.WriteLine("# This file is dynamically updated whenever the editor adds a new group.");
                sw.WriteLine("# You may also edit this file and add/remove groups yourself.");
                sw.WriteLine();
                foreach (var g in groups)
                {
                    sw.WriteLine(g);
                }
            }
        }
    }
}
