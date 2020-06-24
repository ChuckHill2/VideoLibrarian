using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieGuide
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
                case MessageBoxIcon.Error: szIcon = "[Error]"; m_pbIcon.Image = global::MovieGuide.Properties.Resources.MB_Error; break;
                case MessageBoxIcon.Question: szIcon="[?]"; m_pbIcon.Image = global::MovieGuide.Properties.Resources.MB_Question; break;
                case MessageBoxIcon.Warning: szIcon = "[Warning]"; m_pbIcon.Image = global::MovieGuide.Properties.Resources.MB_Warning; break;
                case MessageBoxIcon.Information: szIcon = "[Info]"; m_pbIcon.Image = global::MovieGuide.Properties.Resources.MB_Info; break;
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
            var tsi = new ToolStripMenuItem("Copy", global::MovieGuide.Properties.Resources.CopyToClipboard, delegate(object sender, EventArgs e) { Clipboard.SetText(string.Format("{0}\r\n{1} {2}", this.Text, szIcon, m_lblText.Text)); }, Keys.Control | Keys.C);
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
