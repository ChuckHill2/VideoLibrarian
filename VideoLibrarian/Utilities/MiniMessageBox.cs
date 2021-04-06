//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="MiniMessageBox.cs" company="Chuck Hill">
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
using System.Drawing;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public partial class MiniMessageBox : Form
    {

        public static DialogResult Show(IWin32Window owner, string text, string caption="", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            if (owner == null)
            {
                owner = System.Windows.Forms.Form.ActiveForm;
            }
            if (owner == null)
            {
                FormCollection fc = System.Windows.Forms.Application.OpenForms;
                if (fc != null && fc.Count > 0) owner = fc[0];
            }

            using (var dlg = new MiniMessageBox(owner, text, caption, buttons, icon))
            {
                return dlg.ShowDialog(owner);
            }
        }

        private MiniMessageBox(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            InitializeComponent();

            string szIcon = "[]"; //for clipboard
            switch(icon)
            {
                case MessageBoxIcon.Error: szIcon = "[Error]"; m_pbIcon.Image = global::VideoLibrarian.Properties.Resources.MB_Error; break;
                case MessageBoxIcon.Question: szIcon="[?]"; m_pbIcon.Image = global::VideoLibrarian.Properties.Resources.MB_Question; break;
                case MessageBoxIcon.Warning: szIcon = "[Warning]"; m_pbIcon.Image = global::VideoLibrarian.Properties.Resources.MB_Warning; break;
                case MessageBoxIcon.Information: szIcon = "[Info]"; m_pbIcon.Image = global::VideoLibrarian.Properties.Resources.MB_Info; break;
                case MessageBoxIcon.None:
                default: break;
            }

            switch(buttons)
            {
                case MessageBoxButtons.OK:
                    button1.Visible = false;
                    button2.Text = "OK";
                    button2.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.OK; this.Close(); };
                    button3.Visible = false;
                    this.AcceptButton = button2;
                    this.CancelButton = button2;
                    break;
                case MessageBoxButtons.OKCancel:
                    button1.Text = "OK";
                    button1.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.OK; this.Close(); };
                    button2.Visible = false;
                    button3.Text = "Cancel";
                    button3.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.Cancel; this.Close(); };
                    this.AcceptButton = button1;
                    this.CancelButton = button3;
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    button1.Text = "Abort";
                    button1.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.Abort; this.Close(); };
                    button2.Text = "Retry";
                    button2.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.Retry; this.Close(); };
                    button3.Text = "Ignore";
                    button3.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.Ignore; this.Close(); };
                    this.AcceptButton = button1;
                    this.CancelButton = button3;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    button1.Text = "Yes";
                    button1.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.Yes; this.Close(); };
                    button2.Text = "No";
                    button2.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.No; this.Close(); };
                    button3.Text = "Cancel";
                    button3.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.Cancel; this.Close(); };
                    this.AcceptButton = button1;
                    this.CancelButton = button3;
                    break;
                case MessageBoxButtons.YesNo:
                    button1.Text = "Yes";
                    button1.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.Yes; this.Close(); };
                    button2.Visible = false;
                    button3.Text = "No";
                    button3.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.No; this.Close(); };
                    this.AcceptButton = button1;
                    this.CancelButton = button3;
                    break;
                case MessageBoxButtons.RetryCancel:
                    button1.Text = "Retry";
                    button1.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.Retry; this.Close(); };
                    button2.Visible = false;
                    button3.Text = "Cancel";
                    button3.Click += delegate(object sender, EventArgs e) { DialogResult = DialogResult.Cancel; this.Close(); };
                    this.AcceptButton = button1;
                    this.CancelButton = button3;
                    break;
            }
            this.Text = caption;

            ContextMenuStrip ctx = new ContextMenuStrip();
            m_lblText.ContextMenuStrip = ctx;
            var tsi = new ToolStripMenuItem("Copy", global::VideoLibrarian.Properties.Resources.CopyToClipboard, delegate(object sender, EventArgs e) { Clipboard.SetText(string.Format("{0}\r\n{1} {2}", this.Text, szIcon, m_lblText.Text)); }, Keys.Control | Keys.C);
            ctx.Items.Add(tsi);

            Control ctl = (Control)owner;
            int Xpadding = this.Width - m_lblText.Width;
            int Ypadding = this.Height - m_lblText.Height;
            int Xmax = ctl.Width - Xpadding;

            Graphics g = m_lblText.CreateGraphics();
            var sz = g.MeasureString(text, m_lblText.Font, Xmax);
            this.Width = (int)sz.Width + Xpadding +1;
            this.Height = (int)sz.Height + Ypadding+3;
            g.Dispose();
            m_lblText.Text = text;
        }
    }
}
