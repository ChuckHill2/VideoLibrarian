using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VideoLibrarian;

namespace VideoOrganizer
{
    /// <summary>
    /// Extended TextBox control. Features:
    /// (1) A label painted within the control that disappears when text is entered.
    /// (2) When Enabled==false, the body of the control is grayed out.
    /// Limitations: 
    /// (1) BackColor colors are hardcoded to system colors: 'Window' and 'Control'.
    /// (2) Inline label is always left justified. TextAlign only refers to the entered text.
    /// (3) Inline label color is hardcoded as Colors.Gray, however it does use the TextBox Font.
    /// </summary>
    [Serializable]
    [Description("The TextBox control extended to include an optional virtual label within the control.")]
    public class LabeledTextBox : TextBox
    {
        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue("")]
        [Category("Appearance")]
        [Description("The inline label associated with this control.")]
        public string TextLabel { get; set; } = "";

        protected override void OnEnabledChanged(EventArgs e)
        {
            BackColor = Enabled ? SystemColors.Window : SystemColors.Control;
            base.OnEnabledChanged(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            //WM_PAINT is not triggered immediately so we paint now, so it looks seamless.

            //ToDo: To be complete, this.TextAlign (left,right,center) should also be supported.

            if (TextLabel.Length > 0 && this.TextLength == 0)
            {
                using (var gr = Graphics.FromHwnd(this.Handle))
                {
                    gr.DrawString(TextLabel, this.Font, Brushes.Gray, 0, 1);
                }
            }

            base.OnTextChanged(e);
        }

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    Diagnostics.WriteLine("OnPaint");
        //    //This is NEVER called by TextBox. Must use WM_PAINT.
        //}

        [StructLayout(LayoutKind.Sequential)]
        private struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
        }

        const int WM_PAINT = 0x000F;
        [DllImport("user32.dll")] static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);
        [DllImport("user32.dll")] static extern bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_PAINT:
                    if (TextLabel.Length > 0 && this.TextLength == 0)
                    {
                        PAINTSTRUCT ps = new PAINTSTRUCT();
                        Graphics gr = Graphics.FromHdc(BeginPaint(m.HWnd, out ps));
                        var br = new SolidBrush(this.BackColor);
                        gr.FillRectangle(br, gr.VisibleClipBounds);
                        br.Dispose();
                        gr.DrawString(TextLabel, this.Font, Brushes.Gray, 0, 1);
                        gr.Dispose();
                        EndPaint(m.HWnd, ref ps);
                    }
                    break;
            }

            base.WndProc(ref m);
        }
    }
}
