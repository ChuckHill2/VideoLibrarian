//--------------------------------------------------------------------------
// <summary>
//  WinForms UI Editable ListBox Control
// </summary>
// <copyright file="EditListBox.cs" company="Chuck Hill">
// Copyright (c) 2022 Chuck Hill.
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
// <repository>https://github.com/ChuckHill2/ChuckHill2.Utilities</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    ///  @image html Example.png
    /// <summary>
    /// User-Editable ListBox control.
    /// </summary>
    [ToolboxBitmap(typeof(ListBox))]
    [Description("UI Editable ListBox")]
    public class EditListBox : ListBox
    {
        #region Hidden/Disabled Properties
        private const string NOTUSED = "Not used in " + nameof(EditListBox) + ".";
        //! @cond DOXYGENHIDE
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new object DataSource { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string DisplayMember { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string ValueMember { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new object SelectedValue { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DrawMode DrawMode { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string FormatString { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool FormattingEnabled { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new SelectedIndexCollection SelectedIndices { get; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new SelectedObjectCollection SelectedItems { get; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int TopIndex { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int ItemHeight { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContextMenuStrip ContextMenuStrip { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool IntegralHeight { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int ColumnWidth { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool MultiColumn { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new SelectionMode SelectionMode { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Sorted { get; set; }

#pragma warning disable CS0067 //The event is never used
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackgroundImageChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler PaddingChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event PaintEventHandler Paint;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event DrawItemEventHandler DrawItem;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event MeasureItemEventHandler MeasureItem;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackgroundImageLayoutChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler DataSourceChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler DisplayMemberChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event ListControlConvertEventHandler Format;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler FormatInfoChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler FormatStringChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler FormattingEnabledChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler ValueMemberChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BindingContextChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler ContextMenuStripChanged;
        #pragma warning restore CS0067 //The event is never used
        //! @endcond
        #endregion

        private ComboBox m_cbEditor = new ComboBox();
        private Brush _disabledTranslucentBackground = new SolidBrush(Color.FromArgb(32, SystemColors.InactiveCaption));
        private Point TextOffset;

        /// <summary>
        /// List of chars ignored when manually entering new data.
        /// </summary>
        [Category("Behavior"), Description("Any characters to be ignored when manually entering new data.")]
        [DefaultValue("")]
        public string InvalidChars
        {
            get => new string(InvalidCharsArray);
            set => InvalidCharsArray = (value ?? string.Empty).ToCharArray();
        }
        private char[] InvalidCharsArray { get; set; } = new char[0];

        /// <summary>
        ///  Gets or sets the items in the dropdown part of the edit box.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [Localizable(true), MergableProperty(false)]
        [Category("Data"), Description("Items in the combox dropdown of the current object.")]
        public ComboBox.ObjectCollection DropdownItems => m_cbEditor.Items;

        /// <summary>
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
        [Category("Appearance"), Description("The font used to display text in the control.")]
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                m_cbEditor.Font = value;
                UpdateDrawingBounds();
            }
        }

        private void UpdateDrawingBounds()
        {
            base.ItemHeight = m_cbEditor.Height;
            // Need to vertically center the *visible* text within the ItemHeight.
            TextOffset = new Point(0, (int)((base.ItemHeight - this.Font.Height) / 2.0f));
        }

        /// <summary>
        /// Initializes a new instance of the NamedColorListBox class.
        /// </summary>
        public EditListBox():base()
        {
            base.Name = "EditListBox";
            base.DrawMode = DrawMode.OwnerDrawFixed;
            base.IntegralHeight = false;
            // http://yacsharpblog.blogspot.com/2008/07/listbox-flicker.html
            base.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            InitializeContextMenu();  //Add add/remove context menu.

            m_cbEditor.Font = this.Font;
            m_cbEditor.BackColor = this.BackColor;
            m_cbEditor.ForeColor = this.ForeColor;
            m_cbEditor.RightToLeft = this.RightToLeft;
            m_cbEditor.Location = new System.Drawing.Point(0, 0);
            m_cbEditor.Name = "m_cbEditor";
            m_cbEditor.SelectionChangeCommitted += m_cbEditor_SelectionChangeCommitted; //push change to listbox
            m_cbEditor.Leave += m_cbEditor_Leave; //commit changes when focus lost.
            m_cbEditor.KeyPress += m_cbEditor_KeyPress; //exclude specified keyboard chars
            m_cbEditor.DropDownClosed += (s,e)=> this.Focus(); //put focus back on listbox.
            m_cbEditor.GotFocus += (s,e) => m_cbEditor.SelectionStart = 9999; //do not highlight text. set cursor to end of string.
            m_cbEditor.Hide();

            this.Controls.Add(m_cbEditor);
        }
        
        /// <summary>
        /// Manually flush/commit any outstanding changes.
        /// </summary>
        public void Flush()
        {
            m_cbEditor_Leave(m_cbEditor, EventArgs.Empty);
        }

        // Need to know when the parent form/dialog box is accessible. OnParentChanged is not 
        // guarenteed to have a form in the control chain (e.g. group boxes, etc), yet.. This is required in 
        // order to hook into the Form's FormClosing Event to be able to flush any combobox changes.
        private Form FormOwner = null;
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!base.Visible) return;
            Control c = base.Parent;
            while(!(c is Form))
            {
                c = c.Parent;
                if (c == null) break;
            }
            if (c != null)
            {
                if (FormOwner !=null) FormOwner.FormClosing -= FormOwner_FormClosing; //remove previous hook
                FormOwner = (Form)c;
                FormOwner.FormClosing += FormOwner_FormClosing; //add new hook
            }

            base.OnVisibleChanged(e);
        }
        private void FormOwner_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_cbEditor_Leave(m_cbEditor, EventArgs.Empty);
            Form f = sender as Form;
            if (f != null) f.FormClosing -= FormOwner_FormClosing;
        }

        private void InitializeContextMenu()
        {
            var addToolStripMenuItem = new ToolStripMenuItem();
            //addToolStripMenuItem.Image = global::VideoOrganizer.Properties.Resources.Add;
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Text = "Add";
            addToolStripMenuItem.Click += ctxMenu_Add;
            addToolStripMenuItem.Font = this.Font;
            addToolStripMenuItem.BackColor = this.BackColor;
            addToolStripMenuItem.ForeColor = this.ForeColor;
            addToolStripMenuItem.RightToLeft = this.RightToLeft;

            var removeToolStripMenuItem = new ToolStripMenuItem();
            //removeToolStripMenuItem.Image = global::VideoOrganizer.Properties.Resources.Remove;
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += ctxMenu_Remove;
            removeToolStripMenuItem.Font = this.Font;
            removeToolStripMenuItem.BackColor = this.BackColor;
            removeToolStripMenuItem.ForeColor = this.ForeColor;
            removeToolStripMenuItem.RightToLeft = this.RightToLeft;

            var ctxMenu = new ContextMenuStrip();
            base.ContextMenuStrip = ctxMenu;
            m_cbEditor.ContextMenuStrip = ctxMenu;
            ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { addToolStripMenuItem, removeToolStripMenuItem });
            ctxMenu.Name = "contextMenuStrip";
            ctxMenu.Font = this.Font;
            ctxMenu.BackColor = this.BackColor;
            ctxMenu.ForeColor = this.ForeColor;
            ctxMenu.RightToLeft = this.RightToLeft;
        }

        private void ctxMenu_Add(object sender, EventArgs e)
        {
            var index = base.SelectedIndex;
            base.Items.Add("");
            m_cbEditor.Text = "";
            m_cbEditor.Show();
            base.SelectedIndex = base.Items.Count - 1;
        }
        private void ctxMenu_Remove(object sender, EventArgs e)
        {
            var index = base.SelectedIndex;
            if (index == -1) return;
            base.Items.RemoveAt(index);

            if (base.Items.Count > 0)
            {
                if (index >= base.Items.Count) base.SelectedIndex = index - 1;
                else if (index < base.Items.Count) base.SelectedIndex = index;
                m_cbEditor.Text = base.Items[base.SelectedIndex].ToString();
            }
            else
            {
                m_cbEditor.Text = string.Empty;
                m_cbEditor.Hide();
            }

            CurrentIndex = -1;
        }

        private void m_cbEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                m_cbEditor_Leave(m_cbEditor, EventArgs.Empty);
                e.Handled = true;
                return;
            }
            if (InvalidCharsArray.Contains(e.KeyChar))
            {
                e.Handled = true;
                return;
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            m_cbEditor_Leave(m_cbEditor, EventArgs.Empty);
            base.OnLeave(e);
        }

        private void m_cbEditor_Leave(object sender, EventArgs e)
        {
            var text = m_cbEditor.Text.Trim();
            var index = base.SelectedIndex;

            if (text.Length==0)
            {
                ctxMenu_Remove(m_cbEditor, EventArgs.Empty);
                return;
            }

            if (!m_cbEditor.Items.OfType<string>().Contains(text, StringComparer.OrdinalIgnoreCase))
            {
                m_cbEditor.Items.Add(text);
            }

            base.Items[base.SelectedIndex] = text;
        }

        private void m_cbEditor_SelectionChangeCommitted(object sender, EventArgs e)
        {
            base.Items[base.SelectedIndex] = m_cbEditor.SelectedItem;
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            m_cbEditor.Width = this.ClientSize.Width;
            base.OnClientSizeChanged(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            //Event Sequence: Control.HandleCreated. Control.BindingContextChanged. Form.Load. Control.VisibleChanged. Form.Activated. Form.Shown
            base.OnHandleCreated(e);
            UpdateDrawingBounds();
        }

        private bool firstChanged = true;
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (firstChanged && this.Items.Count > 0)
            {
                //Append items in listbox, missing from combobox dropdown.
                var listItems = ((IEnumerable)base.Items).Cast<string>().Select(m => m.Trim()).Where(m => m.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase);
                var cbItems = ((IEnumerable)m_cbEditor.Items).Cast<string>().Select(m => m.Trim()).Where(m => m.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                foreach (var item in listItems)
                {
                    if (cbItems.Contains(item, StringComparer.OrdinalIgnoreCase)) continue;
                    m_cbEditor.Items.Add(item);
                }
                firstChanged = false;
            }

            m_cbEditor.SelectedItem = base.SelectedItem;
            base.OnSelectedIndexChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disabledTranslucentBackground != null)
            {
                _disabledTranslucentBackground.Dispose();
                _disabledTranslucentBackground = null;
            }

            base.Dispose(disposing);
        }

        private int CurrentIndex = -1;
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index == -1) return;
            var v = base.Items[e.Index].ToString();

            #region Set Selected Row Highlight
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e = new DrawItemEventArgs(e.Graphics,
                                          e.Font,
                                          e.Bounds,
                                          e.Index,
                                          e.State ^ DrawItemState.Selected,
                                          base.Focused ? SystemColors.HighlightText : SystemColors.ControlText,
                                          base.Focused ? SystemColors.Highlight : SystemColors.GradientInactiveCaption);//Choose the color

                if (CurrentIndex != e.Index)
                {
                    m_cbEditor.Show();
                    m_cbEditor.Location = new Point(0, e.Bounds.Y);
                    m_cbEditor.SelectedItem = base.SelectedItem;
                }
                CurrentIndex = e.Index;
            }
            #endregion

            #region Draw Text
            var textOffset = TextOffset;
            textOffset.X += e.Bounds.X;
            textOffset.Y += e.Bounds.Y;
            TextRenderer.DrawText(e.Graphics, v, this.Font, textOffset, e.ForeColor, e.BackColor);
            #endregion

            if (!base.Enabled) e.Graphics.FillRectangle(_disabledTranslucentBackground, e.Bounds);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Region iRegion = new Region(e.ClipRectangle);
            using (var br = new SolidBrush(base.BackColor)) e.Graphics.FillRegion(br, iRegion);
            if (!base.Enabled) e.Graphics.FillRectangle(_disabledTranslucentBackground, base.ClientRectangle);

            if (base.Items.Count > 0)
            {
                for (int i = 0; i < base.Items.Count; ++i)
                {
                    System.Drawing.Rectangle irect = base.GetItemRectangle(i);
                    if (e.ClipRectangle.IntersectsWith(irect))
                    {
                        if ((base.SelectionMode == SelectionMode.One && base.SelectedIndex == i)
                        || (base.SelectionMode == SelectionMode.MultiSimple && base.SelectedIndices.Contains(i))
                        || (base.SelectionMode == SelectionMode.MultiExtended && base.SelectedIndices.Contains(i)))
                        {
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                irect, i,
                                DrawItemState.Selected, base.Enabled ? base.ForeColor : SystemColors.GrayText,
                                base.BackColor));
                        }
                        else
                        {
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                irect, i,
                                DrawItemState.Default, base.Enabled ? base.ForeColor : SystemColors.GrayText,
                                base.BackColor));
                        }
                        iRegion.Complement(irect);
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}
