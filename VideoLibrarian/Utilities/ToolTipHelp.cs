//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ToolTipHelp.cs" company="Chuck Hill">
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
using System.Windows.Forms;

namespace VideoLibrarian
{
    //Easy way to add tooltips to all UI elements!
    public class ToolTipHelp
    {
        private System.Windows.Forms.Form m_owner;
        private ToolTip m_ToolTips;
        private bool m_hasHelpText;
        private static ToolTip m_TempTip;

        /// <summary>
        /// Constructor to create tooltip-style help. It handles setting all
        /// necessary attributes in the dialog to enable tooltip-style help and
        /// registering the help strings contained in 'control.AccessibleDescription'.
        /// </summary>
        /// <param name="ownerdlg">'this' parameter for the given dialog</param>
        public ToolTipHelp(System.Windows.Forms.Form ownerdlg)
        {
            m_owner = ownerdlg;
            m_hasHelpText = false;
            m_ToolTips = new ToolTip();
            m_owner.Closing += new System.ComponentModel.CancelEventHandler(OnToolTipOwnerClosing);
            setToolTipEventHandlers(ownerdlg.Controls);
            if (!m_hasHelpText) { m_ToolTips.Dispose(); m_ToolTips = null; return; }  //nothing to do!
            m_ToolTips.AutoPopDelay = 5000; // Set up the delays for the ToolTip.
            m_ToolTips.InitialDelay = 1000;
            m_ToolTips.ReshowDelay = 500;
            m_ToolTips.IsBalloon = false;
            //m_ToolTips.OwnerDraw = true;
            //m_ToolTips.Popup += m_ToolTips_Popup;
            //m_ToolTips.Draw += m_ToolTips_Draw;

        }

        #region OwnerDrawn ToolTips
        //void m_ToolTips_Popup(object sender, PopupEventArgs e)
        //{
        //    string tip = e.AssociatedControl.AccessibleDescription.Replace("\\n", Environment.NewLine);

        //    Form AssociatedForm = null;
        //    for(var c = e.AssociatedControl; c != null; c = c.Parent) AssociatedForm = c as Form;

        //    Graphics g = Graphics.FromHwnd(AssociatedForm.Handle);
        //    var font = new Font("Microsoft Sans Serif", 11.0f, FontStyle.Regular, GraphicsUnit.World);
        //    var sizeF = g.MeasureString(tip, font, AssociatedForm.ClientSize.Width - 20);
        //    e.ToolTipSize = Size.Add(sizeF.ToSize(),new Size(10,10));
        //    font.Dispose();
        //    g.Dispose();
        //}

        //void m_ToolTips_Draw(object sender, DrawToolTipEventArgs e)
        //{
        //    Graphics g = e.Graphics;
        //    g.SmoothingMode = SmoothingMode.AntiAlias;
        //    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        //    var b = new LinearGradientBrush(e.Bounds, Color.Lavender, Color.SkyBlue, 45f);
        //    g.FillRectangle(b, e.Bounds);
        //    //FillRoundedRectangle(g, b, e.Bounds, 20);
        //    b.Dispose();

        //    g.DrawRectangle(Pens.LightSlateGray, Rectangle.Inflate(e.Bounds, -2, -2));
        //    //DrawRoundedRectangle(g, Pens.LightSlateGray, Rectangle.Inflate(e.Bounds, 0, 0), 20);

        //    var font = new Font("Microsoft Sans Serif", 11.0f, FontStyle.Regular, GraphicsUnit.World);
        //    g.DrawString(e.ToolTipText, font, Brushes.Black, e.Bounds.X + 5, e.Bounds.Y + 5);
        //    font.Dispose();
        //}
        #endregion

        private void setToolTipEventHandlers(System.Windows.Forms.Control.ControlCollection collection)
        {
            foreach (System.Windows.Forms.Control control in collection)
            {
                if (control.HasChildren && control.Controls != null) setToolTipEventHandlers(control.Controls);  //recurse
                if (control.AccessibleDescription == null) continue;  //no help tip
                m_hasHelpText = true;
                m_ToolTips.SetToolTip(control, control.AccessibleDescription.Replace(@"\n", Environment.NewLine));
            }
        }

        public void ChangeToolTip(Control c, string msg)
        {
            m_ToolTips.SetToolTip(c, msg);
        }

        public static void ShowTempToolTip(Control c, string msg, ToolTipIcon icon)
        {
            //Info in auto-dispose:
            //https://stackoverflow.com/questions/13387982/c-sharp-winforms-how-to-know-detect-if-tooltip-is-being-displayed-shown?rq=1

            if (m_TempTip == null)
            {
                m_TempTip = new ToolTip();
                m_TempTip.AutoPopDelay = 2;
                m_TempTip.InitialDelay = 0;
                m_TempTip.ReshowDelay = Int32.MaxValue;
                m_TempTip.IsBalloon = false;
                m_TempTip.UseAnimation = true;
                m_TempTip.UseFading = true;
                c.Disposed += c_Disposed;
            }
            m_TempTip.ToolTipIcon = icon;
            if (icon == ToolTipIcon.None) m_TempTip.ToolTipTitle = null;
            else m_TempTip.ToolTipTitle = icon.ToString();
            m_TempTip.Show(msg, c, 10, c.Height / 2, 2000);
        }
        static void c_Disposed(object sender, EventArgs e)
        {
            if (m_TempTip != null) { m_TempTip.Dispose(); m_TempTip = null; }
        }

        private void OnToolTipOwnerClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_ToolTips != null) m_ToolTips.Dispose();
        }
    }
}
