using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoLibrarian
{
    /// <summary>
    /// One-off text popup window. 
    /// Upon instantiation, the popup is displayed with the specified arguments.
    /// The popup text box text is formatted to fit into a 3x2 rectangle and centered on the parent tile.
    /// When the popup box is clicked, it is hidden and disposed.
    /// Any caller may also close the popup by calling SummaryPopup.Dispose().
    /// Not for use in forms designer.
    /// </summary>
    public static class SummaryPopup
    {
        private static Form Popup; //There can only be one popup at a time.

        public static event EventHandler Disposed
        {
            add { if (Popup != null) Popup.Disposed += value; }
            remove { if (Popup != null) Popup.Disposed -= value; }
        }

        public static bool Visible { get { return Popup != null; } }

        public static void Dispose() 
        { 
            if (Popup == null) return;
            Popup.Hide();
            Popup.Dispose();
            Popup = null;
        }

        public static void Create(Control parent, string text, double scalingFactor = 1.0)
        {
            Dispose(); //There can only be one popup at a time.

            var lblSummary = new Label();
            lblSummary.Dock = DockStyle.Fill;
            lblSummary.Font = new Font("Lucida Sans", 9f * (float)scalingFactor, FontStyle.Regular, GraphicsUnit.Point);
            lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            lblSummary.BackgroundImage = global::VideoLibrarian.ResourceCache.TileBackground;
            lblSummary.BackgroundImageLayout = ImageLayout.Stretch;
            lblSummary.BorderStyle = BorderStyle.FixedSingle;
            lblSummary.UseMnemonic = false;
            lblSummary.Name = "lblSummary"; //for debugging
            lblSummary.Tag = parent;        //set for FormMain.FindTile
            lblSummary.Text = text;

            Control Owner = parent;  //Find owning form
            while (Owner.Parent != null) Owner = Owner.Parent;

            Popup = new Form();
            Popup.Owner = Owner as Form;
            Popup.FormBorderStyle = FormBorderStyle.None;
            Popup.WindowState = FormWindowState.Normal;
            Popup.ShowInTaskbar = false;
            Popup.StartPosition = FormStartPosition.Manual;
            Popup.Controls.Add(lblSummary);
            Popup.Name = "SummaryPopup"; //for debugging
            Popup.Tag = parent;          //set for FormMain.FindTile
            
            Popup.Size = ComputeDimensions(lblSummary);

            var pt = new Point((parent.Width - Popup.Width) / 2, (parent.Height - Popup.Height) / 2);
            pt = parent.PointToScreen(pt);

            var screen = Screen.FromControl(parent).WorkingArea;
            if (pt.X < 4) pt.X = 4;
            else if (pt.X > (screen.Width - Popup.Width - 4)) pt.X = screen.Width - Popup.Width - 4;
            if (pt.Y < 4) pt.Y = 4;
            else if (pt.Y > (screen.Height - Popup.Height - 4)) pt.X = screen.Height - Popup.Height - 4;

            Popup.Location = pt;

            lblSummary.Click += (s, e) => Dispose();
            lblSummary.MouseLeave += (s, e) => Dispose();
            Popup.Click += (s, e) => Dispose();
            Popup.MouseLeave += (s, e) => Dispose();

            Popup.Show();
        }

        private static Size ComputeDimensions(Label lblSummary)
        {
            if (lblSummary.Text == "") return new Size(10,10);

            //var g = lblSummary.CreateGraphics();
            string s = lblSummary.Text;

            //var charSize = g.MeasureString("8", lblSummary.Font);

            int width = 50;
            //SizeF size = new SizeF(0, 0);
            //SizeF prevsize = new SizeF(0, 0);
            Size size = new Size(0, 0);
            Size prevsize = new Size(0, 0);
            double ratio = 0;

            const TextFormatFlags flags = TextFormatFlags.HidePrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

            while (ratio < 3)
            {
                //size = g.MeasureString(s, lblSummary.Font, width);
                size = TextRenderer.MeasureText(s, lblSummary.Font, new Size(width, 99999), flags);
                if (size == prevsize) break;
                ratio = size.Width / (double)size.Height;
                width += 25;
                prevsize = size;
            }

            size.Width += 6;
            size.Height += 6;
            //size.Height += charSize.Height + 6;
            //return size.ToSize();
            return size;
        }
    }
}
