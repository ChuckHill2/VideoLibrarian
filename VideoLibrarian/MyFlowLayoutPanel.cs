//#undef DEBUG

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VideoLibrarian
{
    /// <summary>
    /// My Custom FlowLayoutPanel.
    /// Assumes all child controls are exactly the same size.
    /// Only flows left-to-right, then top-to-bottom
    /// </summary>
    public class MyFlowLayoutPanel : ScrollPanel
    {
        private enum Scrolling { Up, Down }
        private int PrevMaxCols = -1;

        public MyFlowLayoutPanel() : base()
        {
            Panel.Layout += Panel_Layout;
        }

        #region Designer Compatibility with System.Windows.Forms.Panel, otherwise ignored
        [Browsable(false), DefaultValue(AutoSizeMode.GrowOnly), Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual AutoSizeMode AutoSizeMode { get; set; }
        #endregion

        void Panel_Layout(object sender, LayoutEventArgs e)
        {
            if (e.AffectedProperty == "Parent" && e.AffectedControl.Parent == null)
                PrevMaxCols = -1;

            if (this.Children.Count == 0) return;

            var cc = this.Children[0];
            int maxcols = Panel.ClientSize.Width / (cc.Width + cc.Margin.Horizontal);
            if (maxcols == 0) maxcols = 1;
            if (e.AffectedControl == null) PrevMaxCols = -1;
            if (maxcols == PrevMaxCols) return;
            PrevMaxCols = maxcols;

            Diagnostics.WriteLine("BEGIN FlowLayoutPanel.Panel_Layout: Computing NEW tile locations. {0}", ScrollPanel.FormatLayoutEventArgs(e));

            var hDWP = BeginDeferWindowPos(this.Controls.Count);

            for (int i=0,j=0; i < this.Children.Count; i++,j++)
            {
                Control c = this.Children[i];

                int row = j / maxcols;
                int col = j % maxcols;
                var x = col * (c.Width + c.Margin.Horizontal) + c.Margin.Left;
                var y = row * (c.Height + c.Margin.Vertical) + c.Margin.Top;
                var ex = ScrollPanel.GetExtraInfo(c);
                ex.Location = new Point(x, y);

                if (x > Panel.ClientSize.Width) x = short.MaxValue;
                if (y > Panel.ClientSize.Height) y = short.MaxValue;
                SWP uflags = SWP.NOSIZE | SWP.NOZORDER | SWP.NOOWNERZORDER | SWP.NOACTIVATE;
                uflags |= (y < Panel.ClientSize.Height && ex.Visible ? SWP.SHOWWINDOW : SWP.HIDEWINDOW);
                DeferWindowPos(hDWP, c.Handle, HWND.Top, x, y, 0, 0, uflags);

                Diagnostics.WriteLine(@"        Control={0}, Visible={1}, New Location={2}", c, ex.Visible, new Point(x, y));

                if (!ex.Visible) { j--; continue; }
            }

            EndDeferWindowPos(hDWP);

            //Resets ScrollBar.Value=0; without actually scrolling the panel contents.
            //Important because all the children's positions are reset back to the beginning.
            //It is up to the caller to restore the original scroll position.
            VerticalScroll.Reset();

            Diagnostics.WriteLine("END FlowLayoutPanel.Panel_Layout: Computing NEW tile locations. {0}", ScrollPanel.FormatLayoutEventArgs(e));
        }

        #region === Win32 ===
        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr BeginDeferWindowPos(int nNumWindows);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, [MarshalAs(UnmanagedType.I4)] SWP uFlags);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);

        /// <summary>
        /// Window handles (HWND) used for DeferWindowPos hWndInsertAfter
        /// </summary>
        private static class HWND
        {
            public static IntPtr
            NoTopMost = new IntPtr(-2),
            TopMost = new IntPtr(-1),
            Top = new IntPtr(0),
            Bottom = new IntPtr(1);
        }

        /// <summary>
        /// SetWindowPos/DeferWindowPos Flags
        /// </summary>
        [Flags]
        private enum SWP : int
        {
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = NOOWNERZORDER,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000
        }
        #endregion
    }
}
