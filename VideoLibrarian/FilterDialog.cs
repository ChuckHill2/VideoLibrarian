using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public partial class FilterDialog : Form
    {
        private FilterProperties OriginalFilter;
        private FilterProperties NewFilter = null;

        public static FilterProperties Show(IWin32Window owner, FilterProperties filter = null)
        {
            using (var dlg = new FilterDialog(filter))
            {
                if (dlg.ShowDialog(owner) == DialogResult.OK)
                {
                    return dlg.NewFilter;
                }
                return null;
            }
        }

        private FilterDialog(FilterProperties filter)
        {
            OriginalFilter = filter;
 
            InitializeComponent();

            //this.Font = new Font("Microsoft Sans Serif", 9f,FontStyle.Regular,GraphicsUnit.Pixel,0);
            //this.Scale(new System.Drawing.SizeF(2, 2)); //Scales sizes control dimensions, but not font

            this.SuspendLayout();

            m_cbIn.DataSource = Enum.GetValues(typeof(ContainsLocation));

            m_clbGenre.Items.Clear();
            m_clbGenre.Items.AddRange(FilterProperties.AvailableGenres.Select(m=>(object)m).ToArray());
            m_clbGenre.DisplayMember = "FriendlyName";

            m_clbVideoType.Items.Clear();
            m_clbVideoType.Items.AddRange(FilterProperties.AvailableClasses.Select(m => (object)m).ToArray());
            m_clbVideoType.DisplayMember = "FriendlyName";

            if (OriginalFilter == null) //Use default filters
            {
                m_cbIn.SelectedItem = ContainsLocation.Anywhere;

                for (int i = 0; i < m_clbGenre.Items.Count; i++) m_clbGenre.SetItemChecked(i, true);
                for (int i = 0; i < m_clbVideoType.Items.Count; i++) m_clbVideoType.SetItemChecked(i, true);

                m_cbRating.Items.Clear();
                for (int i = FilterProperties.MaxRating-1; i >= FilterProperties.MinRating; i--)
                {
                    if (i==0) continue;
                    m_cbRating.Items.Add(string.Concat(i,"+"));
                }
                m_cbRating.SelectedIndex = m_cbRating.Items.Count-1;
                m_chkUnrated.Checked = (FilterProperties.MinRating==0);

                m_numReleaseFrom.Value = m_numReleaseTo.Minimum = m_numReleaseFrom.Minimum = FilterProperties.MinYear;
                m_numReleaseTo.Value = m_numReleaseFrom.Maximum = m_numReleaseTo.Maximum = FilterProperties.MaxYear; 

                m_radBothWatched.Select();

                m_chkDisabled.Checked = true;

                this.ResumeLayout();
                return;
            }

            m_txtContains.Text = filter.ContainsSubstring ?? string.Empty;
            m_cbIn.SelectedItem = filter.ContainsLocation;

            var comparer = new EqualityComparer<FilterProperties.FilterValue>((x, y) => x.Name.Equals(y.Name, StringComparison.Ordinal));
            for (int i = 0; i < m_clbGenre.Items.Count; i++)
            {
                m_clbGenre.SetItemChecked(i, filter.Genres.Contains((FilterProperties.FilterValue)m_clbGenre.Items[i], comparer));
            }
            bool isChecked = false;
            for (int i = 0; i < m_clbGenre.Items.Count; i++)
            {
                var ch = m_clbGenre.GetItemChecked(i);
                if (ch) isChecked = true;
            }
            if (!isChecked) for (int i = 0; i < m_clbGenre.Items.Count; i++) m_clbGenre.SetItemChecked(i, true);

            for (int i = 0; i < m_clbVideoType.Items.Count; i++)
            {
                m_clbVideoType.SetItemChecked(i, filter.Classes.Contains((FilterProperties.FilterValue)m_clbVideoType.Items[i], comparer));
            }

            isChecked = false;
            for (int i = 0; i < m_clbVideoType.Items.Count; i++)
            {
                var ch = m_clbVideoType.GetItemChecked(i);
                if (ch) isChecked = true;
            }
            if (!isChecked) for (int i = 0; i < m_clbVideoType.Items.Count; i++) m_clbVideoType.SetItemChecked(i, true);

            m_cbRating.Items.Clear();
            for (int i = FilterProperties.MaxRating - 1; i >= FilterProperties.MinRating; i--)
            {
                if (i == 0) continue;
                m_cbRating.Items.Add(string.Concat(i, "+"));
            }
            m_cbRating.SelectedItem = (filter.Rating >= FilterProperties.MinRating && filter.Rating <= FilterProperties.MaxRating ? filter.Rating : FilterProperties.MinRating) + "+";
            m_chkUnrated.Checked = filter.IncludeUnrated;

            m_numReleaseTo.Minimum = m_numReleaseFrom.Minimum = FilterProperties.MinYear;
            m_numReleaseFrom.Maximum = m_numReleaseTo.Maximum = FilterProperties.MaxYear;
            if (filter.StartYear < FilterProperties.MinYear) filter.StartYear = FilterProperties.MinYear;
            if (filter.EndYear > FilterProperties.MaxYear) filter.EndYear = FilterProperties.MaxYear;
            m_numReleaseFrom.Value = filter.StartYear;
            m_numReleaseTo.Value = filter.EndYear;

            if (!filter.Watched.HasValue) m_radBothWatched.Select();
            else if (filter.Watched.Value) m_radWatched.Select();
            else m_radUnwatched.Select();

            m_chkDisabled.Checked = filter.FilteringDisabled;

            this.ResumeLayout();
        }

        private void m_btnOK_Click(object sender, EventArgs e)
        {
            this.NewFilter = ValidateAndReturn();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void m_btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void m_btnSelectAll_Click(object sender, EventArgs e)
        {
            CheckedListBox clb = (sender == m_btnGenreSelectAll ? m_clbGenre : m_clbVideoType);
            for (int i = 0; i < clb.Items.Count; i++) clb.SetItemChecked(i, true);
        }

        private void m_btnSelectNone_Click(object sender, EventArgs e)
        {
            CheckedListBox clb = (sender == m_btnGenreSelectNone ? m_clbGenre : m_clbVideoType);
            for (int i = 0; i < clb.Items.Count; i++) clb.SetItemChecked(i, false);
        }

        private void m_numReleaseFrom_ValueChanged(object sender, EventArgs e)
        {
            var v1 = m_numReleaseFrom.Value;
            var v2 = m_numReleaseTo.Value;
            if (v1 > v2) m_numReleaseTo.Value = v1;
        }

        private void m_numReleaseTo_ValueChanged(object sender, EventArgs e)
        {
            var v1 = m_numReleaseFrom.Value;
            var v2 = m_numReleaseTo.Value;
            if (v2 < v1) m_numReleaseFrom.Value = v2;
        }

        private FilterProperties ValidateAndReturn()
        {
            var fp = new FilterProperties();

            fp.ContainsSubstring  = m_txtContains.Text;
            fp.ContainsLocation = (ContainsLocation)m_cbIn.SelectedItem;

            var list = new List<FilterProperties.FilterValue>(m_clbGenre.Items.Count);
            for (int i = 0; i < m_clbGenre.Items.Count; i++)
            {
                if (!m_clbGenre.GetItemChecked(i)) continue;
                list.Add((FilterProperties.FilterValue)m_clbGenre.Items[i]);
            }
            fp.Genres = list.ToArray();

            list.Clear();
            for (int i = 0; i < m_clbVideoType.Items.Count; i++)
            {
                if (!m_clbVideoType.GetItemChecked(i)) continue;
                list.Add((FilterProperties.FilterValue)m_clbVideoType.Items[i]);
            }
            fp.Classes = list.ToArray();

            var szRating = (string)m_cbRating.SelectedItem ?? (FilterProperties.MinRating.ToString() + "+");
            fp.Rating = int.Parse(szRating.Replace("+",""));
            fp.IncludeUnrated = m_chkUnrated.Checked;

            fp.StartYear = (int)m_numReleaseFrom.Value;
            fp.EndYear = (int)m_numReleaseTo.Value;

            fp.Watched = m_radBothWatched.Checked ? (bool?)null : m_radWatched.Checked ? true : false;

            fp.FilteringDisabled = m_chkDisabled.Checked;
            return fp;
        }

        private void m_chkDisabled_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = !m_chkDisabled.Checked;

            m_grpContains.Enabled = enabled;
            m_grpGenre.Enabled = enabled;
            m_grpRating.Enabled = enabled;
            m_grpVideoType.Enabled = enabled;
            m_grpReleaseYear.Enabled = enabled;
            m_grpWatch.Enabled = enabled;
        }

        private void m_btnContainsClear_Click(object sender, EventArgs e)
        {
            m_txtContains.Clear();
        }
    }
}
