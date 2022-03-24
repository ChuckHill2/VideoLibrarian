//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="SortDialog.cs" company="Chuck Hill">
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public partial class SortDialog : Form
    {
        private ToolTipHelp tt;
        private SortProperties OriginalOrder;
        private SortProperties NewOrder = null;

        public static SortProperties Show(IWin32Window owner, SortProperties order = null)
        {
            using (var dlg = new SortDialog(order))
            {
                if (dlg.ShowDialog(owner) == DialogResult.OK)
                {
                    return dlg.NewOrder;
                }
                return null;
            }
        }

        private SortDialog(SortProperties order)
        {
            OriginalOrder = order;

            NewOrder = order==null ? new SortProperties() : order.Clone();
            InitializeComponent();
            tt = new ToolTipHelp(this);
        }

        private void FormSort_Load(object sender, EventArgs e)
        {
            var col1 = new DataGridViewCheckBoxColumn();
            col1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            col1.DataPropertyName = "Enabled";
            col1.TrueValue = true;
            col1.FalseValue = false;
            col1.HeaderText = "";
            col1.MinimumWidth = 21;
            col1.Name = "Enabled";
            col1.ReadOnly = false;
            col1.Width = 21;
            col1.ToolTipText = "Enable/disable property\r\nto use as a sort key.";

            var col2 = new DataGridViewTextBoxColumn();
            col2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            col2.DataPropertyName = "FriendlyName";
            col2.HeaderText = "Name";
            col2.MinimumWidth = 93;
            col2.Name = "Name";
            col2.ReadOnly = true;
            col2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            col2.Width = 93;
            col2.ToolTipText = "Name of property to use as a\r\nsort key. Click and drag row to\r\nchange sort priority.";

            var col3 = new DataGridViewComboBoxColumn();
            col3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            col3.DataPropertyName = "Direction";
            col3.HeaderText = "Direction";
            col3.MinimumWidth = 94;
            col3.Name = "Direction";
            col3.ReadOnly = false;
            col3.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            col3.Width = 94;
            col3.DataSource = Enum.GetValues(typeof(SortDirection));
            col3.ToolTipText = "Sort direction.";
            col3.FlatStyle = FlatStyle.Flat;

            m_grid.Columns.Clear();
            m_grid.Columns.AddRange(new DataGridViewColumn[] { col1, col2, col3 });

            var bindingList = new BindingList<SortProperties.SortKey>(NewOrder.Keys);
            m_grid.AutoGenerateColumns = false;
            m_grid.DataSource = bindingList;

            var dlgHGridSpacing = this.Width - m_grid.Width;
            var gridHBorders = m_grid.Width - (m_grid.Columns[0].Width + m_grid.Columns[1].Width + m_grid.Columns[2].Width);
            var dlgVGridSpacing = this.Height - m_grid.Height;
            var gridVBorders = m_grid.Height - ((m_grid.Rows.Count+1) * m_grid.Rows[1].Height);


            m_grid.MultiSelect = false;
            m_grid.ReadOnly = false;
            //resize all the columns to fit the data.
            var savedValue = m_grid.Rows[0].Cells[2].Value;
            m_grid.Rows[0].Cells[2].Value = SortDirection.Descending;
            m_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            for (int i = 0; i < m_grid.Columns.Count; i++)
            {
                int width = m_grid.Columns[i].GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
                if (width > 300) width = 300;
                m_grid.Columns[i].Width = width;
            }
            m_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            m_grid.Rows[0].Cells[2].Value = savedValue;

            //this.Width = this.Width - (m_grid.Width - m_grid.Columns[0].Width - m_grid.Columns[1].Width - m_grid.Columns[2].Width) + SystemInformation.VerticalScrollBarWidth + 3;

            m_grid.Rows[0].Selected = true;


            this.Width = dlgHGridSpacing + (m_grid.Columns[0].Width + m_grid.Columns[1].Width + m_grid.Columns[2].Width) + 2;
            this.Height = dlgVGridSpacing + ((m_grid.Rows.Count+1) * m_grid.Rows[1].Height) + 2;
        }

        private void m_btnOK_Click(object sender, EventArgs e)
        {
            var ds = m_grid.DataSource as System.Collections.IList;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void m_btnCancel_Click(object sender, EventArgs e)
        {
            this.NewOrder = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void m_grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            //Do nothing. Just mask any format errors.
            //Ex: source value is not in the list of known enum values.
            //When this occurs the value is displayed as its native value, an int.
        }

        #region DragDrop Grid Rows
        private Rectangle dragBoxFromMouseDown;
        private void m_grid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var grid = sender as DataGridView;
            var ht = grid.HitTest(e.X, e.Y);
            if (ht.RowIndex < 0) return;
            grid.Rows[ht.RowIndex].Selected = true;
            //if (ht.ColumnIndex == 0)
            //{
            //    var ds = grid.DataSource as System.Collections.IList;
            //    var row = ds[ht.RowIndex] as SortProperties.SortKey;
            //    bool enabled = !row.Enabled;
            //    row.Enabled = enabled;

            //    var cell = grid.Rows[ht.RowIndex].Cells[0] as DataGridViewCheckBoxCell;
            //    grid.CurrentCell = cell;
            //    grid.BeginEdit(true);
            //    grid.CurrentCell.Value = enabled;
            //    grid.EndEdit();
            //    return;
            //}
            if (ht.ColumnIndex == 1) //only click n drag on 'Name' column
            {
                Size dragSize = SystemInformation.DragSize;
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
                grid.MouseMove += Drag_MouseMove;
                return;
            }
            //if (ht.ColumnIndex == 2)
            //{
            //    grid.CurrentCell = grid[ht.RowIndex, 2];
            //    grid.BeginEdit(true);
            //    var comboBox = grid.EditingControl as DataGridViewComboBoxEditingControl;
            //    comboBox.DroppedDown = true;
            //    return;
            //}

        }
        private void Drag_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (dragBoxFromMouseDown == Rectangle.Empty || dragBoxFromMouseDown.Contains(e.X, e.Y)) return;
            var grid = sender as DataGridView;
            dragBoxFromMouseDown = Rectangle.Empty;
            grid.DoDragDrop(grid.SelectedRows[0], DragDropEffects.Move);
            grid.MouseMove -= Drag_MouseMove; //once DoDragDrop is called, MouseMove is never called again. see this.Capture;
        }
        private void m_grid_DragOver(object sender, DragEventArgs e) //mousemove
        {
            e.Effect = DragDropEffects.Move;
        }
        private void m_grid_DragDrop(object sender, DragEventArgs e)
        {
            var grid = sender as DataGridView;
            Point clientPoint = grid.PointToClient(new Point(e.X, e.Y));
            var dstIndex = grid.HitTest(clientPoint.X, clientPoint.Y).RowIndex;
            if (dstIndex == -1) return;

            if (e.Effect == DragDropEffects.Move)
            {
                var rw = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
                if (rw == null) return;
                if (rw.Index == dstIndex) return;
                var ds = grid.DataSource as System.Collections.IList;
                var entry = ds[rw.Index];
                ds.RemoveAt(rw.Index);
                ds.Insert(dstIndex, entry);
                grid.Rows[dstIndex].Selected = true;
                grid.Refresh();
            }
        }
        private void m_grid_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        #endregion

        private void m_grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = sender as DataGridView;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (e.ColumnIndex == 0)
            {
                var ds = grid.DataSource as System.Collections.IList;
                var row = ds[e.RowIndex] as SortProperties.SortKey;
                bool enabled = !row.Enabled;
                row.Enabled = enabled;

                //var cell = grid.Rows[e.RowIndex].Cells[0] as DataGridViewCheckBoxCell;
                //grid.CurrentCell = cell;
                //grid.BeginEdit(true);
                //grid.CurrentCell.Value = enabled;
                //grid.EndEdit();
                return;
            }

            if (e.ColumnIndex == 2)
            {
                //grid.CurrentCell = grid[e.RowIndex, 2];
                grid.BeginEdit(true);
                var comboBox = grid.EditingControl as DataGridViewComboBoxEditingControl;
                if (comboBox == null) return;
                comboBox.DroppedDown = true;
                return;
            }
        }

        private void m_grid_Leave(object sender, EventArgs e)
        {
            var grid = sender as DataGridView;
            grid.EndEdit();
        }

        private void m_btnUp_Click(object sender, EventArgs e)
        {
            if (m_grid.SelectedRows.Count == 0)
            {
                MiniMessageBox.ShowDialog(m_grid, "Select a row before moving it.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var rw = m_grid.SelectedRows[0];
            int index = rw.Index;
            if (index == 0) return;
            var ds = m_grid.DataSource as System.Collections.IList;
            var entry = ds[index];
            ds.RemoveAt(index);
            ds.Insert(index - 1, entry);
            m_grid.Rows[index - 1].Selected = true;
            m_grid.Refresh();
        }

        private void m_btnDown_Click(object sender, EventArgs e)
        {
            if (m_grid.SelectedRows.Count == 0)
            {
                MiniMessageBox.ShowDialog(m_grid, "Select a row before moving it.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var rw = m_grid.SelectedRows[0];
            int index = rw.Index;
            if (index == m_grid.RowCount-1) return;
            var ds = m_grid.DataSource as System.Collections.IList;
            var entry = ds[index];
            ds.RemoveAt(index);
            ds.Insert(index + 1, entry);
            m_grid.Rows[index + 1].Selected = true;
            m_grid.Refresh();
        }
    }
}
