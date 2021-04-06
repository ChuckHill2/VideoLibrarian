//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Watched.cs" company="Chuck Hill">
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
    //This is used in 4 of the 6 tiles. So for brevity we set all the properties here.
    public class Watched : CheckBox
    {
        private static Font dtFont;
        private string szDate = null;
        private DateTime dtDate;

        public Watched() : base()
        {
            base.Anchor = System.Windows.Forms.AnchorStyles.None;
            base.Appearance = System.Windows.Forms.Appearance.Button;
            base.BackColor = System.Drawing.Color.Transparent;
            base.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            base.Cursor = System.Windows.Forms.Cursors.Hand;
            base.FlatAppearance.BorderSize = 0;
            base.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            base.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            base.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            base.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            base.Font = global::VideoLibrarian.ResourceCache.FontBold;
            base.Image = global::VideoLibrarian.ResourceCache.CheckboxUnchecked;
            base.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            base.Name = "m_chkWatched";
            base.Text = "Watched";
            base.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            base.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            base.UseVisualStyleBackColor = false;

            if (dtFont == null)
                dtFont = new Font(base.Font.Name, base.Font.Size * 0.75f, FontStyle.Regular);
        }

        #region Hide unused/restricted properties
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        new public Appearance Appearance { get { return base.Appearance; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override System.Drawing.Color BackColor { get { return base.BackColor; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ImageLayout BackgroundImageLayout { get { return base.BackgroundImageLayout; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Cursor Cursor { get { return base.Cursor; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        new public FlatButtonAppearance FlatAppearance { get { return null; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        new public FlatStyle FlatStyle { get { return base.FlatStyle; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override System.Drawing.Font Font { get { return base.Font; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        new public System.Drawing.Image Image { get { return base.Image; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        new public System.Drawing.ContentAlignment ImageAlign { get { return base.ImageAlign; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text { get { return base.Text; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override System.Drawing.ContentAlignment TextAlign { get { return base.TextAlign; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        new public TextImageRelation TextImageRelation { get { return base.TextImageRelation; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        new public bool UseVisualStyleBackColor { get { return base.UseVisualStyleBackColor; } set { } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        new public bool Checked { get { return base.Checked; } set { } }
        #endregion

        protected override void OnCheckedChanged(EventArgs e)
        {
            CheckDate = base.Checked ? (dtDate == DateTime.MinValue ? DateTime.Now : dtDate) : DateTime.MinValue;

            if (this.Enabled)
            {
                var tile = this.FindParent<ITile>();
                tile.MovieProps.Watched = CheckDate;
            }

            base.Image = base.Checked ? global::VideoLibrarian.ResourceCache.CheckboxChecked : global::VideoLibrarian.ResourceCache.CheckboxUnchecked;
            base.OnCheckedChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.Focus(); //if we don't do this the user has to click twice!
            TileBase.Highlight_MouseEnter(this, e);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            TileBase.Highlight_MouseLeave(this, e);
            base.OnMouseLeave(e);
        }

        public DateTime CheckDate
        {
            get { return dtDate; }
            set
            {
                if (dtDate == value.Date) return;
                dtDate = value.Date;
                if (dtDate == DateTime.MinValue)
                {
                    szDate = null;
                    base.Checked = false;
                }
                else
                {
                    szDate = dtDate.ToString("d");
                    base.Checked = true;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (szDate!=null) e.Graphics.DrawString(szDate, dtFont, Brushes.Black, 37, 24);
        }
    }
}
