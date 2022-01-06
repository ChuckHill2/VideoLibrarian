//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ScrollPanel.cs" company="Chuck Hill">
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
//#undef DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VideoLibrarian
{
    /// <summary>
    /// Identical to System.Windows.Forms.ScrollableControl EXCEPT scroll range is NOT limited to native win32 16-bit integers.
    /// </summary>
    public class ScrollPanel : Control
    {
        #region Designer Compatibility with System.Windows.Forms.ScrollControl, otherwise ignored
        [Browsable(false), DefaultValue(false), Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool AutoScroll { get; set; }

        [Browsable(false), Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size AutoScrollMargin { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point AutoScrollPosition { get; set; }

        [Browsable(false), Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size AutoScrollMinSize { get; set; }
        #endregion
        
        #region Dispose(bool disposing)
        private System.ComponentModel.IContainer components = null; // Required designer variable.
        /// <summary>Clean up any resources being used.</summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        #endregion

        #region ==== Reflect ====
        #region Raw Control Bounds
        private struct RawControl
        {
            private static readonly FieldInfo fiX = typeof(Control).GetField("x", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly FieldInfo fiY = typeof(Control).GetField("y", BindingFlags.Instance | BindingFlags.NonPublic);
            //private static readonly FieldInfo fiWidth = typeof(Control).GetField("width", BindingFlags.Instance | BindingFlags.NonPublic);
            //private static readonly FieldInfo fiHeight = typeof(Control).GetField("height", BindingFlags.Instance | BindingFlags.NonPublic);
            //private static readonly FieldInfo fiClientWidth = typeof(Control).GetField("clientWidth", BindingFlags.Instance | BindingFlags.NonPublic);
            //private static readonly FieldInfo fiClientHeight = typeof(Control).GetField("clientHeight", BindingFlags.Instance | BindingFlags.NonPublic);
            //private static readonly MethodInfo miXClearPreferredSizeCache = Type.GetType("System.Windows.Forms.Layout.CommonProperties, " + typeof(System.Windows.Forms.Layout.LayoutEngine).Assembly.FullName, false, false)
            //    .GetMethod("xClearPreferredSizeCache", BindingFlags.Static | BindingFlags.NonPublic);
            //private static readonly MethodInfo miDoLayout = Type.GetType("System.Windows.Forms.Layout.LayoutTransaction, " + typeof(System.Windows.Forms.Layout.LayoutEngine).Assembly.FullName, false, false)
            //    .GetMethod("DoLayout", BindingFlags.Static | BindingFlags.Public);
            private static readonly MethodInfo miSetStyle = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);

            private readonly Control ctrl;
            public RawControl(Control ctrl) { this.ctrl = ctrl; }

            public int X { get { return (int)fiX.GetValue(ctrl); } set { fiX.SetValue(ctrl, value); } }
            public int Y { get { return (int)fiY.GetValue(ctrl); } set { fiY.SetValue(ctrl, value); } }
            //public int Width { get { return (int)fiWidth.GetValue(ctrl); } set { fiWidth.SetValue(ctrl, value); } }
            //public int Height { get { return (int)fiHeight.GetValue(ctrl); } set { fiHeight.SetValue(ctrl, value); } }
            //public int ClientWidth { get { return (int)fiClientWidth.GetValue(ctrl); } set { fiClientWidth.SetValue(ctrl, value); } }
            //public int ClientHeight { get { return (int)fiClientHeight.GetValue(ctrl); } set { fiClientHeight.SetValue(ctrl, value); } }

            //public static void xClearPreferredSizeCache(Control element) { miXClearPreferredSizeCache.Invoke(null, new object[] { element }); }
            //public static void DoLayout(Control elementToLayout, Control elementCausingLayout, string property) { miDoLayout.Invoke(null, new object[] { elementToLayout, elementCausingLayout, property }); }

            public void SetStyle(ControlStyles styleFlags, bool doSet) { miSetStyle.Invoke(ctrl, new object[] { styleFlags, doSet }); }
        }
        #endregion

        #region Custom Control Properties
        private static readonly MethodInfo miCreateKey = Type.GetType("System.Windows.Forms.PropertyStore, " + typeof(System.Windows.Forms.Control).Assembly.FullName, false, false)
            .GetMethod("CreateKey", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo miGetObject = Type.GetType("System.Windows.Forms.PropertyStore, " + typeof(System.Windows.Forms.Control).Assembly.FullName, false, false)
            .GetMethod("GetObject", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(int) }, null);
        private static readonly MethodInfo miSetObject = Type.GetType("System.Windows.Forms.PropertyStore, " + typeof(System.Windows.Forms.Control).Assembly.FullName, false, false)
            .GetMethod("SetObject", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(int), typeof(Object) }, null);
        private static readonly FieldInfo fiPropertyStore = typeof(Control).GetField("propertyStore", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///    Generates a new unique property key used to reference associated property value of a control.
        /// </summary>
        /// <returns>Unique integer property key</returns>
        private static int CreatePropertyKey() { return (int)miCreateKey.Invoke(null, new object[] { }); }
        /// <summary>
        ///    Returns specified property from control. Returns NULL if not found.
        /// </summary>
        /// <param name="c">Control containing property.</param>
        /// <param name="key">Unique integer key (generated by CreatePropertyKey) to specified property</param>
        /// <returns>Property value or null if not found.</returns>
        private static object GetProperty(Control c, int key) { return miGetObject.Invoke(fiPropertyStore.GetValue(c), new object[] { key }); }
        /// <summary>
        ///    Create or reassign property value to specified control and key.
        /// </summary>
        /// <param name="c">Control to assign property.</param>
        /// <param name="key">Unique integer key (generated by CreatePropertyKey) to specified property</param>
        /// <param name="value">
        ///    Value to associate with control and key. NOTE: For performance reasons, 
        ///    value should not be a value type (e.g. Structs, and primitive types like 
        ///    integers,floats, etc.). It is best to wrap them into a class (e.g. 
        ///    reference type).
        /// </param>
        private static void SetProperty(Control c, int key, object value) { miSetObject.Invoke(fiPropertyStore.GetValue(c), new object[] { key, value }); }
        #endregion
        #endregion

        #region Get/Set Custom ExtraInfo
        private static readonly int ExtraInfoKey = CreatePropertyKey();
        public class ExtraInfo
        {
            public Point Location;
            public readonly RectangleRef DisplayRect;
            public int Index;
            public bool Visible;

            public ExtraInfo(RectangleRef displayRect, Point location, int index, bool visible)
            {
                this.Location = location;
                this.DisplayRect = displayRect;
                this.Index = index;
                this.Visible = visible;
            }
        }
        public static ExtraInfo GetExtraInfo(Control c) { return (ExtraInfo)GetProperty(c, ExtraInfoKey); }
        public static void SetExtraInfo(Control c, ExtraInfo info) { SetProperty(c, ExtraInfoKey, info); }
        #endregion

        private readonly ProXoft.WinForms.ScrollBarEnhanced m_vScroll = new ProXoft.WinForms.ScrollBarEnhanced();
        private readonly ProXoft.WinForms.ScrollBarEnhanced m_hScroll = new ProXoft.WinForms.ScrollBarEnhanced();
        private readonly MyPanel m_panel = new MyPanel();
        protected Panel Panel { get { return m_panel; } }
        protected readonly ChildControls Children; //Contains live low-level (e.g.efficient) read-only array of child controls in m_panel.

        #region public ControlCollection Controls
        new public ControlCollection Controls { get { return m_panel.Controls; } }
        new public event ControlEventHandler ControlAdded
        {
            add { m_panel.ControlAdded += value; }
            remove { m_panel.ControlAdded -= value; }
        }
        new public event ControlEventHandler ControlRemoved
        {
            add { m_panel.ControlRemoved += value; }
            remove { m_panel.ControlRemoved -= value; }
        }

        protected class ChildControls
        {
            //This class is a shortcut to improve readonly efficiency to m_panel child controls.
            private static readonly FieldInfo fiInnerList = Type.GetType("System.Windows.Forms.Layout.ArrangedElementCollection, " + typeof(System.Windows.Forms.Layout.LayoutEngine).Assembly.FullName, false, false)
                .GetField("_innerList", BindingFlags.Instance | BindingFlags.NonPublic);
            private readonly ArrayList _innerList;
            public ChildControls(Control parent) { _innerList = (ArrayList)fiInnerList.GetValue(parent.Controls); }
            public int Count { get { return _innerList.Count; } }
            public Control this[int index] { get { return (Control)_innerList[index]; } }
        }
        #endregion

        public ScrollPanel()
        {
            DisplayRect = new RectangleRef(); //must be initialized BEFORE InitializeComponent();

            #region InitializeComponent()
            base.SuspendLayout();

            m_vScroll.Margin = new Padding(0);
            m_vScroll.Name = "m_vScroll";
            m_vScroll.Text = "m_vScroll";
            m_vScroll.Orientation = Orientation.Vertical;
            m_vScroll.ShowContextMenu = false;
            m_vScroll.ShowToolTips = false;
            m_vScroll.Scroll += m_vScroll_Scroll;
            m_vScroll.VisibleChanged += scrollBar_VisibleChanged;

            m_hScroll.Margin = new Padding(0);
            m_hScroll.Name = "m_hScroll";
            m_hScroll.Text = "m_hScroll";
            m_hScroll.Orientation = Orientation.Horizontal;
            m_hScroll.ShowContextMenu = false;
            m_hScroll.ShowToolTips = false;
            m_hScroll.Scroll += m_hScroll_Scroll;
            m_hScroll.VisibleChanged += scrollBar_VisibleChanged;

            m_panel.Margin = new Padding(0);
            m_panel.Name = "m_panel";
            m_panel.Text = "m_panel";
            m_panel.ControlAdded += m_panel_ControlAdded;
            m_panel.ControlRemoved += m_panel_ControlRemoved;
            m_panel.PostLayout += m_panel_PostLayout;
            #if DEBUG
                m_panel.Layout += m_panel_Layout;
                m_panel.Paint += m_panel_Paint;
            #endif
            base.Controls.Add(m_panel);
            base.Controls.Add(m_hScroll);
            base.Controls.Add(m_vScroll);
            base.Margin = new Padding(0);
            base.ResumeLayout(false);
            #endregion

            Children = new ChildControls(m_panel);

            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.controlstyles?view=netframework-4.7.2
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.StandardClick, false);

            var rc = new RawControl(m_panel);
            rc.SetStyle(ControlStyles.Selectable, false);
            rc.SetStyle(ControlStyles.StandardClick, false);

            VerticalScroll.HideLegacyScrollBars();
            HorizontalScroll.HideLegacyScrollBars();
        }

        //Recompute Panel size when scrollbar is hidden or shown.
        private void scrollBar_VisibleChanged(object sender, EventArgs e)
        {
            Diagnostics.WriteLine("ScrollPanel.scrollBar_VisibleChanged: Calling ResizePanelChildren({0})", ((Control)sender).Name);
            ResizePanelChildren((Control)sender);
        }

        //Scroll panel contents when scrollbar position changes.
        //Also post to this.OnScroll/Scroll event.
        private void m_vScroll_Scroll(object sender, ProXoft.WinForms.EnhancedScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.EndScroll) return;
            int oldValue = ROUND(e.OldValue);
            int newValue = ROUND(e.NewValue);
            if (oldValue != newValue)
            {
                DisplayRect.Y = -newValue;
                ScrollWindow(0, ROUND(e.OldValue - e.NewValue));
            }
            OnScroll(new ScrollEventArgs(e.Type, oldValue, newValue, e.ScrollOrientation));
        }
        private void m_hScroll_Scroll(object sender, ProXoft.WinForms.EnhancedScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.EndScroll) return;
            int oldValue = ROUND(e.OldValue);
            int newValue = ROUND(e.NewValue);
            if (oldValue != newValue)
            {
                DisplayRect.X = -newValue;
                ScrollWindow(ROUND(e.OldValue - e.NewValue), 0);
            }
            OnScroll(new ScrollEventArgs(e.Type, oldValue, newValue, e.ScrollOrientation));
        }

        ///<summary>Raises the ScrollablePanel.Scroll event. Nothing else.</summary>
        protected virtual void OnScroll(ScrollEventArgs e) { if (Scroll != null) { Scroll(this, e); } }
        ///<summary>Occurs when the user or code scrolls through the client area.</summary>
        public event ScrollEventHandler Scroll = null;

        private int ROUND(decimal v) { return (int)decimal.Round(v); }

        private void ScrollWindow(int dX, int dY)
        {
            if (dX == 0 && dY == 0) return;
            Control container = m_panel;

            Diagnostics.WriteLine("BEGIN ScrollPanel.ScrollWindow: dX={0}, dY={1}, Value={2}, Client={3}, DisplayRect={4}", dX, dY, m_vScroll.Value.ToString("0.#################################"), container.ClientSize, DisplayRect);

            #region SetChildrenLocations
            // For very large virtual display rectangles (e.g. >32767) Win32 ScrollWindow() fails strangely because it 
            // only knows about 16-bit ranges. Therefore we must skip ScrollWindow alltogether and manually fixup the 
            // locations of the controls moved in and out of the visible client area.
            // Note: Debugging the clip and scroll portions of the client area from within the Paint events, 
            // Win32 BitBlt() appears to provide no advantage at all.

            const SWP swp = SWP.NOSIZE | SWP.NOZORDER | SWP.NOOWNERZORDER | SWP.NOACTIVATE;
            Rectangle newDisplayRect = DisplayRect;
            Rectangle oldDisplayRect = DisplayRect;
            oldDisplayRect.Offset(-dX, -dY);
            var visibleControls = new List<Control>();

            var hDWP = BeginDeferWindowPos(this.Children.Count);
            for (int i = 0; i < this.Children.Count; i++)
            {
                Control c = this.Children[i];
                var ei = GetExtraInfo(c);

                var oldx = ei.Location.X + oldDisplayRect.X;
                var oldy = ei.Location.Y + oldDisplayRect.Y;
                var oldIntersected = container.ClientRectangle.IntersectsWith(new Rectangle(oldx, oldy, c.Width, c.Height));

                var newx = ei.Location.X + newDisplayRect.X;
                var newy = ei.Location.Y + newDisplayRect.Y;
                var newIntersected = container.ClientRectangle.IntersectsWith(new Rectangle(newx, newy, c.Width, c.Height));

                if (oldIntersected && newIntersected)
                {
                    var rc = new RawControl(c);
                    rc.X = newx;
                    rc.Y = newy;
                    DeferWindowPos(hDWP, c.Handle, HWND.Top, newx, newy, 0, 0, swp | (ei.Visible ? SWP.SHOWWINDOW : SWP.HIDEWINDOW));
                    Diagnostics.WriteLine("        Control={0}, child still visible in client, new location={1}, Visible={2}", c, new Point(newx, newy), ei.Visible);
                    if (ei.Visible) visibleControls.Add(c);
                }
                else if (newIntersected)
                {
                    var rc = new RawControl(c);
                    rc.X = newx;
                    rc.Y = newy;
                    DeferWindowPos(hDWP, c.Handle, HWND.Top, newx, newy, 0, 0, swp | (ei.Visible ? SWP.SHOWWINDOW : SWP.HIDEWINDOW));
                    Diagnostics.WriteLine("        Control={0}, new child shown in client, new location={1}, Visible={2}", c, new Point(newx, newy), ei.Visible);
                    if (ei.Visible) visibleControls.Add(c);
                }
                else if (oldIntersected)
                {
                    newx = (int)short.MaxValue;
                    newy = (int)short.MaxValue;

                    var rc = new RawControl(c);
                    rc.X = newx;
                    rc.Y = newy;
                    DeferWindowPos(hDWP, c.Handle, HWND.Top, newx, newy, 0, 0, swp | SWP.HIDEWINDOW);
                    Diagnostics.WriteLine("        Control={0}, previously shown child now hidden, new location={1}", c, new Point(newx, newy));
                }
                else
                {
                    //Diagnostics.WriteLine("        Control={0}, no change", c);
                }
            }
            EndDeferWindowPos(hDWP);

            //Set control in middle of visible window as the CurrentVisibleControl.
            if (visibleControls.Count > 0)
                CurrentVisibleControl = visibleControls[visibleControls.Count/2];
            #endregion

            Diagnostics.WriteLine("END ScrollPanel.ScrollWindow: dX={0}, dY={1}, Value={2}, Client={3}, DisplayRect={4}", dX, dY, m_vScroll.Value.ToString("0.#################################"), container.ClientSize, DisplayRect);
        }

        /// <summary>Get the visible child control in the center of the window.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control CurrentVisibleControl { get; private set; }

        private ScrollProperties _verticalScroll = null;
        ///<summary>Gets the characteristics associated with the vertical scroll bar.</summary>
        public ScrollProperties VerticalScroll
        {
            get
            {
                if (_verticalScroll == null)
                    _verticalScroll = new ScrollProperties(this, Orientation.Vertical);
                return _verticalScroll;
            }
        }

        private ScrollProperties _horizontalScroll = null;
        /// <summary>Gets the characteristics associated with the horizontal scroll bar.</summary>
        public ScrollProperties HorizontalScroll
        {
            get
            {
                if (_horizontalScroll == null)
                    _horizontalScroll = new ScrollProperties(this, Orientation.Horizontal);
                return _horizontalScroll;
            }
        }

        private RectangleRef DisplayRect; //initialized in constructor before InitializeComponent();
        ///<summary>Returns the virtual client area containing all the child controls. May be much larger or smaller than the actual client area.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Rectangle DisplayRectangle { get { return DisplayRect; } }

        void m_panel_ControlAdded(object sender, ControlEventArgs e)
        {
            var c = e.Control;
            SetExtraInfo(c, new ExtraInfo(DisplayRect, c.Location, this.Children.Count - 1, c.Visible));
            //#if DEBUG
            //    c.LocationChanged += m_panel_ChildControlLocationChanged;
            //    c.Paint += m_panel_ChildControlPaint;
            //#endif
            //Diagnostics.WriteLine("ScrollPanel.m_panel_ControlAdded: [Set ExtraInfo] Control={0}", e.Control);
        }

        void m_panel_ControlRemoved(object sender, ControlEventArgs e)
        {
            var c = e.Control;
            SetExtraInfo(c, null);
            //#if DEBUG
            //    c.LocationChanged -= m_panel_ChildControlLocationChanged;
            //    c.Paint -= m_panel_ChildControlPaint;
            //#endif
            //Diagnostics.WriteLine("ScrollPanel.m_panel_ControlRemoved: [Clear ExtraInfo] Control={0}", e.Control);
        }

        #region DEBUG
        #if DEBUG
        void m_panel_ChildControlLocationChanged(object sender, EventArgs e) //debugging only
        {
            var c = (Control)sender;
            Diagnostics.WriteLine("ScrollPanel.m_panel_ChildControlLocationChanged: [NA] {0}, Location={1}", c, c.Location);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            Diagnostics.WriteLine("ScrollPanel.OnLayout: [NA] {0}, Panel={1}, DisplayRect={2}", FormatLayoutEventArgs(e), m_panel.ClientSize, DisplayRect.Size);
            base.OnLayout(e);
        }

        void m_panel_ChildControlPaint(object sender, PaintEventArgs e) //debugging only
        {
            var c = (Control)sender;
            Diagnostics.WriteLine("ScrollPanel.m_panel_ChildControlPaint: [NA] ClientSize={0}, Clip={1}, Control={2}", m_panel.ClientSize, e.ClipRectangle, c);
        }

        void m_panel_Layout(object sender, LayoutEventArgs e) //debugging only
        {
            Diagnostics.WriteLine("ScrollPanel.m_panel_Layout: [NA] {0}", FormatLayoutEventArgs(e));
        }

        void m_panel_Paint(object sender, PaintEventArgs e) //debugging only
        {
            var c = (Control)sender;
            Diagnostics.WriteLine("ScrollPanel.m_panel_Paint: [NA] ClientSize={0}, Clip={1}", m_panel.ClientSize, e.ClipRectangle);
        }

        public static string FormatLayoutEventArgs(LayoutEventArgs e) //debugging OnLayout()
        {
            string propValue;
            switch (e.AffectedProperty)
            {
                case "Parent": propValue = e.AffectedControl.Parent == null ? "NULL" : e.AffectedControl.Parent.Name; break;
                case "Bounds": propValue = e.AffectedControl.Bounds.ToString(); break;
                case "Visible": propValue = e.AffectedControl.Visible.ToString(); break;
                default: propValue = string.Empty; break;
            }
            return string.Format("AffectedControl={0}, AffectedProperty={1} ({2})", e.AffectedControl.Text == "" ? e.AffectedControl.Name : e.AffectedControl.Text, e.AffectedProperty, propValue);
        }
        #else
        public static string FormatLayoutEventArgs(LayoutEventArgs e) { return string.Empty; }
        #endif
        #endregion

        void m_panel_PostLayout(object sender, LayoutEventArgs e)
        {
            if (e.AffectedProperty == "Parent" && e.AffectedControl.Parent == null)
            {
                Diagnostics.WriteLine("ScrollPanel.m_panel_PostLayout: [RESET] {0}, Panel={1}, DisplayRect={2}", FormatLayoutEventArgs(e), m_panel.ClientSize, DisplayRect.Size);
                
                HorizontalScroll.Reset();
                VerticalScroll.Reset();
                DisplayRect.X = 0;
                DisplayRect.Y = 0;
                PrevColumnCount = -1; //force recompute of virtual bounds of all child controls.
            }
            else if (((e.AffectedProperty == "Parent" && e.AffectedControl.Parent == m_panel) || 
                 (e.AffectedProperty == "Bounds" && e.AffectedControl == m_panel)) && this.Children.Count > 0)
            {
                Diagnostics.WriteLine("BEGIN ScrollPanel.m_panel_PostLayout: [ComputeVirtualBounds, SB.Visibility] {0}, Panel={1}, DisplayRect={2}", FormatLayoutEventArgs(e), m_panel.ClientSize, DisplayRect.Size);

                bool changed = ComputeVirtualBounds();

                VerticalScroll.Visible = (DisplayRect.Height > m_panel.ClientSize.Height);
                HorizontalScroll.Visible = (DisplayRect.Width > m_panel.ClientSize.Width);

                var c = this.Children[0];
                VerticalScroll.LargeChange = m_panel.ClientSize.Height;
                VerticalScroll.SmallChange = (c.Height + c.Margin.Vertical) / 10;
                HorizontalScroll.LargeChange = m_panel.ClientSize.Width;
                HorizontalScroll.SmallChange = (c.Width + c.Margin.Horizontal) / 10;

                if (changed && CurrentVisibleControl != null)
                {
                    c = CurrentVisibleControl;
                    var ei = ScrollPanel.GetExtraInfo(c);
                    var pos = ei == null ? 0 : ei.Location.Y - m_panel.ClientSize.Height / 2 + c.Height / 2;
                    if (pos > m_panel.ClientSize.Height)
                    {
                        VerticalScroll.Value = pos;
                    }
                }

                Diagnostics.WriteLine("END ScrollPanel.m_panel_PostLayout: [ComputeVirtualBounds, SB.Visibility] {0}, Panel={1}, DisplayRect={2}", FormatLayoutEventArgs(e), m_panel.ClientSize, DisplayRect.Size);
            }
            else
            {
                Diagnostics.WriteLine("ScrollPanel.m_panel_PostLayout: [NA] {0}, Panel={1}, DisplayRect={2}", FormatLayoutEventArgs(e), m_panel.ClientSize, DisplayRect.Size);
            }
        }

        private int PrevColumnCount = -1;  //ComputeVirtualBounds Optimization: Abort on first row if #columns same as previously computed.
        private bool ComputeVirtualBounds()
        {
            if (this.Children.Count == 0) { VerticalScroll.Visible = false; HorizontalScroll.Visible = false; return false; }
            Diagnostics.WriteLine("BEGIN ScrollPanel.OnLayout.ComputeVirtualBounds: Panel={0}, DisplayRect={1} ", m_panel.ClientSize, DisplayRect.Size);

            //Compute size of rectangle containing all the controls in this container.
            int xMax=0, yMax=0;
            int prevY = GetExtraInfo(this.Children[0]).Location.Y;
            bool isFirstRow = true;
            int colCount = 0;
            Control c = null;
            for (int i = 0; i < this.Children.Count; i++)
            {
                c = this.Children[i];
                var ei = GetExtraInfo(c);
                if (!ei.Visible) continue;

                var x = ei.Location.X;
                var y = ei.Location.Y;
                if (x > xMax) xMax = x;
                if (y > yMax) yMax = y;

                //Optimization: Abort on first row if #columns same as previously computed.
                if (isFirstRow && y != prevY)
                {
                    if (PrevColumnCount == colCount)
                    {
                        Diagnostics.WriteLine("END ScrollPanel.OnLayout.ComputeVirtualBounds: ABORT, colCount == PrevColumnCount");
                        return false; //not changed
                    }
                    isFirstRow = false;
                    PrevColumnCount = colCount;
                    colCount = 0;
                }
                colCount++;
                prevY = y;
            }


            if (c != null)
            {
                xMax += c.Width + c.Margin.Right;
                yMax += c.Height + c.Margin.Bottom;
            }

            DisplayRect.Height = yMax;
            DisplayRect.Width = xMax;

            VerticalScroll.Maximum = yMax - (m_panel.ClientSize.Height - 1);
            HorizontalScroll.Maximum = xMax - (m_panel.ClientSize.Width - 1);

            VerticalScroll.Minimum = 0;
            HorizontalScroll.Minimum = 0;

            Diagnostics.WriteLine("END ScrollPanel.OnLayout.ComputeVirtualBounds: Panel={0}, DisplayRect={1} ", m_panel.ClientSize, new Size(xMax, yMax));

            return true; //changed
        }

        private int MinimizedScrollPos = -1;
        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.ClientSize.IsEmpty) //Minimized
            {
                Diagnostics.WriteLine("ScrollPanel.OnSizeChanged: Minimized");
                MinimizedScrollPos = VerticalScroll.Value;
            }
            else
            {
                Diagnostics.WriteLine("ScrollPanel.OnSizeChanged: Calling ResizePanelChildren({0})", this.Name);
                ResizePanelChildren(this);
                if (MinimizedScrollPos != -1)
                {
                    VerticalScroll.Value = MinimizedScrollPos;
                    MinimizedScrollPos = -1;
                }
            }
            //base.OnSizeChanged(e); //triggers Resize and SizeChanged events. no other processing.
        }

        private void ResizePanelChildren(Control sender)
        {
            Control c = sender as Control;
            Diagnostics.WriteLine("BEGIN ScrollPanel.ResizePanelChildren({0}): Panel={1}, DisplayRect={2} Horz/Vert-SB.Visible=({3},{4})", c.Name, m_panel.Bounds, DisplayRect, m_hScroll.Visible, m_vScroll.Visible);

            var cl = base.ClientSize;
            var deltaSB = new Size(m_vScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0, 
                                   m_hScroll.Visible ? SystemInformation.HorizontalScrollBarHeight : 0);

            var hDWP = BeginDeferWindowPos(3);

            var rc = new Rectangle(0, 0, cl.Width - deltaSB.Width, cl.Height - deltaSB.Height);
            if (!m_panel.Bounds.Equals(rc))
                hDWP = DeferWindowPos(hDWP, m_panel.Handle, HWND.Top, rc.X, rc.Y, rc.Width, rc.Height, SWP.NOZORDER);

            rc = new Rectangle(cl.Width - SystemInformation.VerticalScrollBarWidth, 0, SystemInformation.VerticalScrollBarWidth, cl.Height - deltaSB.Height);
            if (!m_vScroll.Bounds.Equals(rc))
                hDWP = DeferWindowPos(hDWP, m_vScroll.Handle, HWND.Top, rc.X, rc.Y, rc.Width, rc.Height, SWP.NOZORDER);

            rc = new Rectangle(0, cl.Height - SystemInformation.HorizontalScrollBarHeight, cl.Width - deltaSB.Width, SystemInformation.HorizontalScrollBarHeight);
            if (!m_hScroll.Bounds.Equals(rc))
                hDWP = DeferWindowPos(hDWP, m_hScroll.Handle, HWND.Top, rc.X, rc.Y, rc.Width, rc.Height, SWP.NOZORDER);

            
            var ok = EndDeferWindowPos(hDWP);

            VerticalScroll.Maximum = DisplayRect.Height - (m_panel.ClientSize.Height - 1);
            HorizontalScroll.Maximum = DisplayRect.Width - (m_panel.ClientSize.Width - 1);

            Diagnostics.WriteLine("END ScrollPanel.ResizePanelChildren({0}): Panel={1}, DisplayRect={2} Horz/Vert-SB.Visible=({3},{4})", c.Name, m_panel.Bounds, DisplayRect, m_hScroll.Visible, m_vScroll.Visible);
        }

        /// <summary>
        /// Captures mouse wheel actions and translates them to small decrement events.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta >= 0) { m_vScroll.ScrollLineUp(); m_vScroll.ScrollLineUp(); }
            else { m_vScroll.ScrollLineDown(); m_vScroll.ScrollLineDown(); }
            base.OnMouseWheel(e);
        }

        protected override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            const int WM_KEYDOWN = 0x0100;
            const int WM_SYSKEYDOWN = 0x0104;

            if (m.Msg != WM_KEYDOWN && m.Msg != WM_SYSKEYDOWN) return base.ProcessCmdKey(ref m, keyData);

            try
            {
                Diagnostics.WriteLine("BEGIN {0}: Key={1}, keyData={2}", (WM)m.Msg, (Keys)(int)m.WParam, keyData);
                switch (keyData)
                {
                    case Keys.Left | Keys.Alt: m_hScroll.ScrollPixelUp(); return true;
                    case Keys.Left: m_hScroll.ScrollLineUp(); return true;
                    case Keys.Right | Keys.Alt: m_hScroll.ScrollPixelDown(); return true;
                    case Keys.Right: m_hScroll.ScrollLineDown(); return true;
                    case Keys.Up | Keys.Alt: m_vScroll.ScrollPixelUp(); return true;
                    case Keys.Up: m_vScroll.ScrollLineUp(); return true;
                    case Keys.Down | Keys.Alt: m_vScroll.ScrollPixelDown(); return true;
                    case Keys.Down: m_vScroll.ScrollLineDown(); return true;

                    case Keys.PageUp:
                        if (m_vScroll.Visible) { m_vScroll.ScrollPageUp(); return true; }
                        if (m_hScroll.Visible) { m_hScroll.ScrollPageUp(); return true; }
                        break;

                    case Keys.PageDown:
                        if (m_vScroll.Visible) { m_vScroll.ScrollPageDown(); return true; }
                        if (m_hScroll.Visible) { m_hScroll.ScrollPageDown(); return true; }
                        break;

                    case Keys.Home:
                        if (m_vScroll.Visible) { m_vScroll.ScrollHome(); return true; }
                        if (m_hScroll.Visible) { m_hScroll.ScrollHome(); return true; }
                        break;

                    case Keys.End:
                        if (m_vScroll.Visible) { m_vScroll.ScrollEnd(); return true; }
                        if (m_hScroll.Visible) { m_hScroll.ScrollEnd(); return true; }
                        break;
                }
            }
            finally
            {
                Diagnostics.WriteLine("END {0}: Key={1}, keyData={2}", (WM)m.Msg, (Keys)(int)m.WParam, keyData);
            }

            return base.ProcessCmdKey(ref m, keyData);

        }

        #region public class ScrollProperties
        public class ScrollProperties
        {
            private bool _largeChangeSetExternally;
            private readonly ScrollPanel _parent;
            private readonly Orientation _orientation;
            private readonly Func<int> DefaultPageSize;
            private readonly Func<int> ClientSize;
            private readonly Func<int> DisplayRectSize;
            private readonly ProXoft.WinForms.ScrollBarEnhanced ScrollBar;
            private int _delayedSetValue = 0;

            private int ROUND(decimal v) { return (int)Math.Round(v); }

            public ScrollProperties(ScrollPanel parent, Orientation orientation)
            {
                this._parent = parent;
                _orientation = orientation;

                ClientSize = orientation == Orientation.Vertical ? (Func<int>)(() => parent.m_panel.ClientSize.Height) : () => parent.m_panel.ClientSize.Width;
                DefaultPageSize = ClientSize;
                DisplayRectSize = orientation == Orientation.Vertical ? (Func<int>)(() => parent.DisplayRect.Height) : () => parent.DisplayRect.Width;
                ScrollBar = orientation == Orientation.Vertical ? parent.m_vScroll : parent.m_hScroll;
            }

            [Category("Scrolling"), Description("Gets or sets a Boolean value controlling whether the scrollbar is enabled.")]
            [DefaultValue(true)]
            public bool Enabled
            {
                get { return ScrollBar.Enabled; }
                set { ScrollBar.Enabled = value; }
            }

            [Category("Scrolling"), Description("Page size when clicking on the scroll bar or pressing the page up/down keys. Default is client area width/height.")]
            [DefaultValue(10), RefreshProperties(RefreshProperties.Repaint)]
            public int LargeChange
            {
                get
                {
                    return _largeChangeSetExternally ? ROUND(Math.Min(ScrollBar.LargeChange, ScrollBar.Maximum - ScrollBar.Minimum + 1)) : DefaultPageSize();
                }
                set
                {
                    ScrollBar.LargeChange = value;
                    _largeChangeSetExternally = (value != DefaultPageSize());
                }
            }

            [Category("Scrolling"), Description("Maximum scrollbar range.")]
            [DefaultValue(100), RefreshProperties(RefreshProperties.Repaint)]
            public int Maximum
            {
                get { return ROUND(ScrollBar.Maximum); }
                set
                {
                    if (value < Minimum) value = Minimum;
                    if (value > DisplayRectSize()) value = DisplayRectSize();
                    if (value < Value) Value = value;
                    ScrollBar.Maximum = value;
                    //The following is necessary when making the window smaller as the new maximum hadn't been computed until now.
                    if (_delayedSetValue != 0) { Value = _delayedSetValue; _delayedSetValue = 0; }
                }
            }

            [Category("Scrolling"), Description("Minimum scrollbar range. Hardcoded to zero.")]
            [DefaultValue(0), RefreshProperties(RefreshProperties.Repaint)]
            public int Minimum
            {
                get { return ROUND(ScrollBar.Minimum); } //ALWAYS zero!
                set
                {
                    if (ScrollBar.Maximum < value) ScrollBar.Maximum = value;
                    if (value > Value) Value = value;
                    ScrollBar.Minimum = value;
                }
            }

            protected ScrollPanel ParentControl { get { return _parent; } }

            [Category("Scrolling"), Description("The increment when pressing on the arrow keys or clicking the arrow buttons on the scroll bar.")]
            [DefaultValue(1)]
            public int SmallChange
            {
                get { return ROUND(Math.Min(ScrollBar.SmallChange, LargeChange)); }
                set
                {
                    if (value < 0) value = 1;
                    ScrollBar.SmallChange = value;
                }
            }

            [Category("Scrolling"), Description("Virtual window position.")]
            [DefaultValue(0)]
            public int Value
            {
                get { return ROUND(ScrollBar.Value); }
                set
                {
                    if (value < Minimum) value = Minimum;
                    else
                    {
                        if (DisplayRectSize() < ClientSize()) value = 0;
                        else
                        {
                            //var max = Maximum - (LargeChange - 1);
                            var max = Maximum;
                            //The following is necessary when making the window smaller as the new maximum 
                            //hadn't yet been computed. Value gets set when the new Maximum gets set.
                            if (value > max) { _delayedSetValue = value; return; }
                            _delayedSetValue = 0;
                        }
                    }
                    if (Value == value) return;

                    var oldVal = ScrollBar.Value;
                    ScrollBar.Value = value;
                    ScrollBar.RaiseScrollEvent(oldVal, value);
                }
            }

            [Category("Scrolling"), Description("Show/Hide scrollbar.")]
            [DefaultValue(false)]
            public bool Visible
            {
                get { return ScrollBar.Visible; }
                set { ScrollBar.Visible = value; }
            }

            internal void Reset()
            {
                ScrollBar.Value = 0m;
            }

            internal void HideLegacyScrollBars()
            {
                const int ESB_DISABLE_BOTH = 0x3;

                SCROLLINFO si = new SCROLLINFO();
                si.cbSize = 28; // Marshal.SizeOf(typeof(SCROLLINFO));
                si.fMask = ScrollInfoMask.SIF_PAGE | ScrollInfoMask.SIF_POS | ScrollInfoMask.SIF_RANGE;
                si.nMin = 0;
                si.nMax = 0;
                si.nPage = 0;
                si.nPos = 0;
                si.nTrackPos = 0; //readonly field so never used by SetScrollInfo

                SetScrollInfo(_parent.Handle, _orientation, ref si, true);
                ShowScrollBar(_parent.Handle, _orientation, false);
                EnableScrollBar(_parent.Handle, _orientation, ESB_DISABLE_BOTH);

                SetScrollInfo(_parent.m_panel.Handle, _orientation, ref si, true);
                ShowScrollBar(_parent.m_panel.Handle, _orientation, false);
                EnableScrollBar(_parent.m_panel.Handle, _orientation, ESB_DISABLE_BOTH);
            }

            #region == HideLegacyScrollBars pInvoke ==
            [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
            private static extern bool ShowScrollBar(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] Orientation wBar, bool bShow);

            [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
            private static extern bool EnableScrollBar(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] Orientation nBar, int wArrows);

            //[DllImport("User32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
            //private static extern int GetScrollPos(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] Orientation nBar);

            [DllImport("User32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
            private static extern int SetScrollPos(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] Orientation nBar, int nPos, bool bRedraw);

            //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
            //private static extern bool GetScrollInfo(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] Orientation nBar, [In, Out] ref SCROLLINFO si);

            [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
            private static extern int SetScrollInfo(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] Orientation nBar, [In] ref SCROLLINFO si, bool bRedraw);

            #region private struct SCROLLINFO
            [Serializable, StructLayout(LayoutKind.Sequential)]
            private struct SCROLLINFO
            {
                public int cbSize; //Marshal.SizeOf(typeof(SCROLLINFO)) == 28
                [MarshalAs(UnmanagedType.U4)]
                public ScrollInfoMask fMask;
                public int nMin;
                public int nMax;
                public int nPage;
                public int nPos;
                public int nTrackPos;

                public override string ToString()
                {
                    return String.Concat(
                        "{nMin=",      nMin.ToString(),
                        ",nMax=",      nMax.ToString(),
                        ",nPage=",     nPage.ToString(),
                        ",nPos=",      nPos.ToString(),
                        ",nTrackPos=", nTrackPos.ToString(), "}");
                }
            }
            #endregion

            #region private enum ScrollInfoMask
            [Flags]
            internal enum ScrollInfoMask : uint
            {
                /// <summary>The nMin and nMax members contain the minimum and maximum values for the scrolling range.</summary>
                SIF_RANGE = 0x1,
                /// <summary>The nPage member contains the page size for a proportional scroll bar.</summary>
                SIF_PAGE = 0x2,
                /// <summary>The nPos member contains the scroll box position, which is not updated while the user drags the scroll box.</summary>
                SIF_POS = 0x4,
                /// <summary>This value is used only when setting a scroll bar's parameters. If the scroll bar's new parameters make the scroll bar unnecessary, disable the scroll bar instead of removing it.</summary>
                SIF_DISABLENOSCROLL = 0x8,
                /// <summary>The nTrackPos member contains the current position of the scroll box while the user is dragging it. Read only. Ignored by SetScrollInfo()</summary>
                SIF_TRACKPOS = 0x10,
                /// <summary>All mask values</summary>
                SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS),
            }
            #endregion
            #endregion
        }
        #endregion

        #region === Win32 ===
        //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "ScrollWindow", CharSet = CharSet.Auto)]
        //private static extern bool ScrollWindow(IntPtr hWnd, int XAmount, int YAmount, IntPtr lpScrollRect, [In] ref RECT lpClipRect);

        //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "ScrollWindow", CharSet = CharSet.Auto)]
        //private static extern bool ScrollWindow(IntPtr hWnd, int XAmount, int YAmount, IntPtr lpScrollRect, IntPtr lpClipRect);

        //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        //private static extern bool UpdateWindow(IntPtr hWnd);

        //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        //private static extern bool InvalidateRect(IntPtr hWnd, ref RECT clip, bool bErase);

        //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        //private static extern bool ValidateRect(IntPtr hWnd, IntPtr rect);

        //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        //private static extern bool GetUpdateRect(IntPtr hWnd, out RECT clip, bool bErase);

        //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        //private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        //private static Rectangle GetWindowBounds(Control c) { RECT rc; GetWindowRect(c.Handle, out rc); Rectangle r = rc; r = c.Parent.RectangleToClient(r); return r; }

        #region private static extern IntPtr DeferWindowPos(...)
        //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        //private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, [MarshalAs(UnmanagedType.I4)] SWP uFlags);

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

        #region private static extern bool BitBlt(...)

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        ///    Performs a bit-block transfer of the color data corresponding to a
        ///    rectangle of pixels from the specified source device context into
        ///    a destination device context.
        /// </summary>
        /// <param name="hdc">Handle to the destination device context.</param>
        /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
        /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
        /// <param name="hdcSrc">Handle to the source device context.</param>
        /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
        /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
        /// <param name="dwRop">A raster-operation code.</param>
        /// <returns>
        ///    <c>true</c> if the operation succeedes, <c>false</c> otherwise. To get extended error information, call <see cref="System.Runtime.InteropServices.Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        /// <summary>
        ///     Specifies a raster-operation code. These codes define how the color data for the
        ///     source rectangle is to be combined with the color data for the destination
        ///     rectangle to achieve the final color.
        /// </summary>
        private enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows 
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }
        #endregion

        #region Window Message Enums
        public enum WM  //System enum
        {
            WM_NULL = 0x0000,
            WM_CREATE = 0x0001,
            WM_DESTROY = 0x0002,
            WM_MOVE = 0x0003,
            WM_SIZE = 0x0005,
            WM_ACTIVATE = 0x0006,
            WM_SETFOCUS = 0x0007,
            WM_KILLFOCUS = 0x0008,
            WM_ENABLE = 0x000A,
            WM_SETREDRAW = 0x000B,
            WM_SETTEXT = 0x000C,
            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            WM_PAINT = 0x000F,
            WM_CLOSE = 0x0010,
            WM_QUERYENDSESSION = 0x0011,
            WM_QUIT = 0x0012,
            WM_QUERYOPEN = 0x0013,
            WM_ERASEBKGND = 0x0014,
            WM_SYSCOLORCHANGE = 0x0015,
            WM_ENDSESSION = 0x0016,
            WM_SHOWWINDOW = 0x0018,
            WM_CTLCOLOR = 0x0019,
            WM_SETTINGCHANGE = 0x001A,
            WM_DEVMODECHANGE = 0x001B,
            WM_ACTIVATEAPP = 0x001C,
            WM_FONTCHANGE = 0x001D,
            WM_TIMECHANGE = 0x001E,
            WM_CANCELMODE = 0x001F,
            WM_SETCURSOR = 0x0020,
            WM_MOUSEACTIVATE = 0x0021,
            WM_CHILDACTIVATE = 0x0022,
            WM_QUEUESYNC = 0x0023,
            WM_GETMINMAXINFO = 0x0024,
            WM_PAINTICON = 0x0026,
            WM_ICONERASEBKGND = 0x0027,
            WM_NEXTDLGCTL = 0x0028,
            WM_SPOOLERSTATUS = 0x002A,
            WM_DRAWITEM = 0x002B,
            WM_MEASUREITEM = 0x002C,
            WM_DELETEITEM = 0x002D,
            WM_VKEYTOITEM = 0x002E,
            WM_CHARTOITEM = 0x002F,
            WM_SETFONT = 0x0030,
            WM_GETFONT = 0x0031,
            WM_SETHOTKEY = 0x0032,
            WM_GETHOTKEY = 0x0033,
            WM_QUERYDRAGICON = 0x0037,
            WM_COMPAREITEM = 0x0039,
            WM_GETOBJECT = 0x003D,
            WM_COMPACTING = 0x0041,
            WM_COMMNOTIFY = 0x0044,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_POWER = 0x0048,
            WM_COPYDATA = 0x004A,
            WM_CANCELJOURNAL = 0x004B,
            WM_NOTIFY = 0x004E,
            WM_INPUTLANGCHANGEREQUEST = 0x0050,
            WM_INPUTLANGCHANGE = 0x0051,
            WM_TCARD = 0x0052,
            WM_HELP = 0x0053,
            WM_USERCHANGED = 0x0054,
            WM_NOTIFYFORMAT = 0x0055,
            WM_CONTEXTMENU = 0x007B,
            WM_STYLECHANGING = 0x007C,
            WM_STYLECHANGED = 0x007D,
            WM_DISPLAYCHANGE = 0x007E,
            WM_GETICON = 0x007F,
            WM_SETICON = 0x0080,
            WM_NCCREATE = 0x0081,
            WM_NCDESTROY = 0x0082,
            WM_NCCALCSIZE = 0x0083,
            WM_NCHITTEST = 0x0084,
            WM_NCPAINT = 0x0085,
            WM_NCACTIVATE = 0x0086,
            WM_GETDLGCODE = 0x0087,
            WM_SYNCPAINT = 0x0088,
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCLBUTTONUP = 0x00A2,
            WM_NCLBUTTONDBLCLK = 0x00A3,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NCRBUTTONUP = 0x00A5,
            WM_NCRBUTTONDBLCLK = 0x00A6,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_NCMBUTTONUP = 0x00A8,
            WM_NCMBUTTONDBLCLK = 0x00A9,
            WM_NCXBUTTONDOWN = 0x00AB,
            WM_NCXBUTTONUP = 0x00AC,
            WM_NCXBUTTONDBLCLK = 0x00AD,

            #region "EDIT" EM_ control messages
            EM_GETSEL = 0x00B0,
            EM_SETSEL = 0x00B1,
            EM_GETRECT = 0x00B2,
            EM_SETRECT = 0x00B3,
            EM_SETRECTNP = 0x00B4,
            EM_SCROLL = 0x00B5,
            EM_LINESCROLL = 0x00B6,
            EM_SCROLLCARET = 0x00B7,
            EM_GETMODIFY = 0x00B8,
            EM_SETMODIFY = 0x00B9,
            EM_GETLINECOUNT = 0x00BA,
            EM_LINEINDEX = 0x00BB,
            EM_SETHANDLE = 0x00BC,
            EM_GETHANDLE = 0x00BD,
            EM_GETTHUMB = 0x00BE,
            EM_LINELENGTH = 0x00C1,
            EM_REPLACESEL = 0x00C2,
            EM_GETLINE = 0x00C4,
            EM_SETLIMITTEXT = 0x00C5,
            EM_CANUNDO = 0x00C6,
            EM_UNDO = 0x00C7,
            EM_FMTLINES = 0x00C8,
            EM_LINEFROMCHAR = 0x00C9,
            EM_SETTABSTOPS = 0x00CB,
            EM_SETPASSWORDCHAR = 0x00CC,
            EM_EMPTYUNDOBUFFER = 0x00CD,
            EM_GETFIRSTVISIBLELINE = 0x00CE,
            EM_SETREADONLY = 0x00CF,
            EM_SETWORDBREAKPROC = 0x00D0,
            EM_GETWORDBREAKPROC = 0x00D1,
            EM_GETPASSWORDCHAR = 0x00D2,
            EM_SETMARGINS = 0x00D3,
            EM_GETMARGINS = 0x00D4,
            EM_GETLIMITTEXT = 0x00D5,
            EM_POSFROMCHAR = 0x00D6,
            EM_CHARFROMPOS = 0x00D7,
            EM_SETIMESTATUS = 0x00D8,
            EM_GETIMESTATUS = 0x00D9,
            #endregion

            #region "STATIC" (aka Label) SBM_ control messages
            SBM_SETPOS = 0x00E0,
            SBM_GETPOS = 0x00E1,
            SBM_SETRANGE = 0x00E2,
            SBM_GETRANGE = 0x00E3,
            SBM_ENABLE_ARROWS = 0x00E4,
            SBM_SETRANGEREDRAW = 0x00E6,
            SBM_SETSCROLLINFO = 0x00E9,
            SBM_GETSCROLLINFO = 0x00EA,
            SBM_GETSCROLLBARINFO = 0x00EB,
            #endregion

            #region "BUTTON" BM_ control messages
            BM_GETCHECK = 0x00F0,
            BM_SETCHECK = 0x00F1,
            BM_GETSTATE = 0x00F2,
            BM_SETSTATE = 0x00F3,
            BM_SETSTYLE = 0x00F4,
            BM_CLICK = 0x00F5,
            BM_GETIMAGE = 0x00F6,
            BM_SETIMAGE = 0x00F7,
            BM_SETDONTCLICK = 0x00F8,
            #endregion

            WM_INPUT_DEVICE_CHANGE = 0x00FE,
            WM_INPUT = 0x00FF,
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_CHAR = 0x0102,
            WM_DEADCHAR = 0x0103,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            WM_SYSCHAR = 0x0106,
            WM_SYSDEADCHAR = 0x0107,
            WM_UNICHAR = 0x0109,
            WM_CONVERTREQUEST = 0x010A,
            WM_CONVERTRESULT = 0x010B,
            WM_INTERIM = 0x010C,
            WM_IME_STARTCOMPOSITION = 0x010D,
            WM_IME_ENDCOMPOSITION = 0x010E,
            WM_IME_COMPOSITION = 0x010F,
            WM_INITDIALOG = 0x0110,
            WM_COMMAND = 0x0111,
            WM_SYSCOMMAND = 0x0112,
            WM_TIMER = 0x0113,
            WM_HSCROLL = 0x0114,
            WM_VSCROLL = 0x0115,
            WM_INITMENU = 0x0116,
            WM_INITMENUPOPUP = 0x0117,
            WM_GESTURE = 0x0119,
            WM_GESTURENOTIFY = 0x011A,
            WM_MENUSELECT = 0x011F,
            WM_MENUCHAR = 0x0120,
            WM_ENTERIDLE = 0x0121,
            WM_MENURBUTTONUP = 0x0122,
            WM_MENUDRAG = 0x0123,
            WM_MENUGETOBJECT = 0x0124,
            WM_UNINITMENUPOPUP = 0x0125,
            WM_MENUCOMMAND = 0x0126,
            WM_CHANGEUISTATE = 0x0127,
            WM_UPDATEUISTATE = 0x0128,
            WM_QUERYUISTATE = 0x0129,
            WM_CTLCOLORMSGBOX = 0x0132,
            WM_CTLCOLOREDIT = 0x0133,
            WM_CTLCOLORLISTBOX = 0x0134,
            WM_CTLCOLORBTN = 0x0135,
            WM_CTLCOLORDLG = 0x0136,
            WM_CTLCOLORSCROLLBAR = 0x0137,
            WM_CTLCOLORSTATIC = 0x0138,

            #region "COMBOBOX" CB_ control messages
            CB_GETEDITSEL = 0x0140,
            CB_LIMITTEXT = 0x0141,
            CB_SETEDITSEL = 0x0142,
            CB_ADDSTRING = 0x0143,
            CB_DELETESTRING = 0x0144,
            CB_DIR = 0x0145,
            CB_GETCOUNT = 0x0146,
            CB_GETCURSEL = 0x0147,
            CB_GETLBTEXT = 0x0148,
            CB_GETLBTEXTLEN = 0x0149,
            CB_INSERTSTRING = 0x014A,
            CB_RESETCONTENT = 0x014B,
            CB_FINDSTRING = 0x014C,
            CB_SELECTSTRING = 0x014D,
            CB_SETCURSEL = 0x014E,
            CB_SHOWDROPDOWN = 0x014F,
            CB_GETITEMDATA = 0x0150,
            CB_SETITEMDATA = 0x0151,
            CB_GETDROPPEDCONTROLRECT = 0x0152,
            CB_SETITEMHEIGHT = 0x0153,
            CB_GETITEMHEIGHT = 0x0154,
            CB_SETEXTENDEDUI = 0x0155,
            CB_GETEXTENDEDUI = 0x0156,
            CB_GETDROPPEDSTATE = 0x0157,
            CB_FINDSTRINGEXACT = 0x0158,
            CB_SETLOCALE = 0x0159,
            CB_GETLOCALE = 0x015A,
            CB_GETTOPINDEX = 0x015B,
            CB_SETTOPINDEX = 0x015C,
            CB_GETHORIZONTALEXTENT = 0x015D,
            CB_SETHORIZONTALEXTENT = 0x015E,
            CB_GETDROPPEDWIDTH = 0x015F,
            CB_SETDROPPEDWIDTH = 0x0160,
            CB_INITSTORAGE = 0x0161,
            CB_MULTIPLEADDSTRING = 0x0163,
            CB_GETCOMBOBOXINFO = 0x0164,
            #endregion

            #region "LISTBOX" LB_ control messages
            LB_ADDSTRING = 0x0180,
            LB_INSERTSTRING = 0x0181,
            LB_DELETESTRING = 0x0182,
            LB_SELITEMRANGEEX = 0x0183,
            LB_RESETCONTENT = 0x0184,
            LB_SETSEL = 0x0185,
            LB_SETCURSEL = 0x0186,
            LB_GETSEL = 0x0187,
            LB_GETCURSEL = 0x0188,
            LB_GETTEXT = 0x0189,
            LB_GETTEXTLEN = 0x018A,
            LB_GETCOUNT = 0x018B,
            LB_SELECTSTRING = 0x018C,
            LB_DIR = 0x018D,
            LB_GETTOPINDEX = 0x018E,
            LB_FINDSTRING = 0x018F,
            LB_GETSELCOUNT = 0x0190,
            LB_GETSELITEMS = 0x0191,
            LB_SETTABSTOPS = 0x0192,
            LB_GETHORIZONTALEXTENT = 0x0193,
            LB_SETHORIZONTALEXTENT = 0x0194,
            LB_SETCOLUMNWIDTH = 0x0195,
            LB_ADDFILE = 0x0196,
            LB_SETTOPINDEX = 0x0197,
            LB_GETITEMRECT = 0x0198,
            LB_GETITEMDATA = 0x0199,
            LB_SETITEMDATA = 0x019A,
            LB_SELITEMRANGE = 0x019B,
            LB_SETANCHORINDEX = 0x019C,
            LB_GETANCHORINDEX = 0x019D,
            LB_SETCARETINDEX = 0x019E,
            LB_GETCARETINDEX = 0x019F,
            LB_SETITEMHEIGHT = 0x01A0,
            LB_GETITEMHEIGHT = 0x01A1,
            LB_FINDSTRINGEXACT = 0x01A2,
            LB_SETLOCALE = 0x01A5,
            LB_GETLOCALE = 0x01A6,
            LB_SETCOUNT = 0x01A7,
            LB_INITSTORAGE = 0x01A8,
            LB_ITEMFROMPOINT = 0x01A9,
            LB_MULTIPLEADDSTRING = 0x01B1,
            LB_GETLISTBOXINFO = 0x01B2,
            #endregion

            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_RBUTTONDBLCLK = 0x0206,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MBUTTONDBLCLK = 0x0209,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C,
            WM_XBUTTONDBLCLK = 0x020D,
            WM_MOUSEHWHEEL = 0x020E,
            WM_PARENTNOTIFY = 0x0210,
            WM_ENTERMENULOOP = 0x0211,
            WM_EXITMENULOOP = 0x0212,
            WM_NEXTMENU = 0x0213,
            WM_SIZING = 0x0214,
            WM_CAPTURECHANGED = 0x0215,
            WM_MOVING = 0x0216,
            WM_POWERBROADCAST = 0x0218,
            WM_DEVICECHANGE = 0x0219,
            WM_MDICREATE = 0x0220,
            WM_MDIDESTROY = 0x0221,
            WM_MDIACTIVATE = 0x0222,
            WM_MDIRESTORE = 0x0223,
            WM_MDINEXT = 0x0224,
            WM_MDIMAXIMIZE = 0x0225,
            WM_MDITILE = 0x0226,
            WM_MDICASCADE = 0x0227,
            WM_MDIICONARRANGE = 0x0228,
            WM_MDIGETACTIVE = 0x0229,
            WM_MDISETMENU = 0x0230,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232,
            WM_DROPFILES = 0x0233,
            WM_MDIREFRESHMENU = 0x0234,
            WM_POINTERDEVICECHANGE = 0x0238,
            WM_POINTERDEVICEINRANGE = 0x0239,
            WM_POINTERDEVICEOUTOFRANGE = 0x023A,
            WM_TOUCH = 0x0240,
            WM_NCPOINTERUPDATE = 0x0241,
            WM_NCPOINTERDOWN = 0x0242,
            WM_NCPOINTERUP = 0x0243,
            WM_POINTERUPDATE = 0x0245,
            WM_POINTERDOWN = 0x0246,
            WM_POINTERUP = 0x0247,
            WM_POINTERENTER = 0x0249,
            WM_POINTERLEAVE = 0x024A,
            WM_POINTERACTIVATE = 0x024B,
            WM_POINTERCAPTURECHANGED = 0x024C,
            WM_TOUCHHITTESTING = 0x024D,
            WM_POINTERWHEEL = 0x024E,
            WM_POINTERHWHEEL = 0x024F,
            WM_IME_REPORT = 0x0280,
            WM_IME_SETCONTEXT = 0x0281,
            WM_IME_NOTIFY = 0x0282,
            WM_IME_CONTROL = 0x0283,
            WM_IME_COMPOSITIONFULL = 0x0284,
            WM_IME_SELECT = 0x0285,
            WM_IME_CHAR = 0x0286,
            WM_IME_REQUEST = 0x0288,
            WM_IME_KEYDOWN = 0x0290,
            WM_IME_KEYUP = 0x0291,
            WM_NCMOUSEHOVER = 0x02A0,
            WM_MOUSEHOVER = 0x02A1,
            WM_NCMOUSELEAVE = 0x02A2,
            WM_MOUSELEAVE = 0x02A3,
            WM_WTSSESSION_CHANGE = 0x02B1,
            WM_TABLET_FIRST = 0x02C0,
            WM_TABLET_ADDED = 0x02C8,
            WM_TABLET_DELETED = 0x02C9,
            WM_TABLET_FLICK = 0x02CB,
            WM_TABLET_QUERYSYSTEMGESTURESTATUS = 0x02CC,
            WM_TABLET_LAST = 0x02DF,
            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,
            WM_CLEAR = 0x0303,
            WM_UNDO = 0x0304,
            WM_RENDERFORMAT = 0x0305,
            WM_RENDERALLFORMATS = 0x0306,
            WM_DESTROYCLIPBOARD = 0x0307,
            WM_DRAWCLIPBOARD = 0x0308,
            WM_PAINTCLIPBOARD = 0x0309,
            WM_VSCROLLCLIPBOARD = 0x030A,
            WM_SIZECLIPBOARD = 0x030B,
            WM_ASKCBFORMATNAME = 0x030C,
            WM_CHANGECBCHAIN = 0x030D,
            WM_HSCROLLCLIPBOARD = 0x030E,
            WM_QUERYNEWPALETTE = 0x030F,
            WM_PALETTEISCHANGING = 0x0310,
            WM_PALETTECHANGED = 0x0311,
            WM_HOTKEY = 0x0312,
            WM_PRINT = 0x0317,
            WM_PRINTCLIENT = 0x0318,
            WM_APPCOMMAND = 0x0319,
            WM_THEMECHANGED = 0x031A,
            WM_CLIPBOARDUPDATE = 0x031D,
            WM_DWMCOMPOSITIONCHANGED = 0x031E,
            WM_DWMNCRENDERINGCHANGED = 0x031F,
            WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,
            WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
            WM_DWMSENDICONICTHUMBNAIL = 0x0323,
            WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326,
            WM_GETTITLEBARINFOEX = 0x033F,
            WM_HANDHELDFIRST = 0x0358,
            WM_HANDHELDLAST = 0x035F,
            WM_QUERYAFXWNDPROC = 0x0360,
            WM_SIZEPARENT = 0x0361,
            WM_SETMESSAGESTRING = 0x0362,
            WM_IDLEUPDATECMDUI = 0x0363,
            WM_INITIALUPDATE = 0x0364,
            WM_COMMANDHELP = 0x0365,
            WM_HELPHITTEST = 0x0366,
            WM_EXITHELPMODE = 0x0367,
            WM_RECALCPARENT = 0x0368,
            WM_SIZECHILD = 0x0369,
            WM_KICKIDLE = 0x036A,
            WM_QUERYCENTERWND = 0x036B,
            WM_DISABLEMODAL = 0x036C,
            WM_FLOATSTATUS = 0x036D,
            WM_ACTIVATETOPLEVEL = 0x036E,
            WM_RESERVED_036F = 0x036F,
            WM_RESERVED_0370 = 0x0370,
            WM_RESERVED_0371 = 0x0371,
            WM_RESERVED_0372 = 0x0372,
            WM_SOCKET_NOTIFY = 0x0373,
            WM_SOCKET_DEAD = 0x0374,
            WM_POPMESSAGESTRING = 0x0375,
            WM_HELPPROMPTADDR = 0x0376,
            WM_OCC_LOADFROMSTREAM = 0x0376,
            WM_OCC_LOADFROMSTORAGE = 0x0377,
            WM_OCC_INITNEW = 0x0378,
            WM_QUEUE_SENTINEL = 0x0379,
            WM_OCC_LOADFROMSTREAM_EX = 0x037A,
            WM_OCC_LOADFROMSTORAGE_EX = 0x037B,
            WM_MFC_INITCTRL = 0x037C,
            WM_RESERVED_037D = 0x037D,
            WM_RESERVED_037E = 0x037E,
            WM_FORWARDMSG = 0x037F,
            WM_PENWINFIRST = 0x0380,
            WM_PENWINLAST = 0x038F,
            WM_DDE_INITIATE = 0x03E0,
            WM_DDE_TERMINATE = 0x03E1,
            WM_DDE_ADVISE = 0x03E2,
            WM_DDE_UNADVISE = 0x03E3,
            WM_DDE_ACK = 0x03E4,
            WM_DDE_DATA = 0x03E5,
            WM_DDE_REQUEST = 0x03E6,
            WM_DDE_POKE = 0x03E7,
            WM_DDE_EXECUTE = 0x03E8,
            WM_CPL_LAUNCH = 0x07E8,
            WM_CPL_LAUNCHED = 0x07E9,

            #region "SysLink" hyperlink common control messages
            LM_HITTEST = 0x0700,
            LM_GETIDEALHEIGHT = 0x0701,
            LM_SETITEM = 0x0702,
            LM_GETITEM = 0x0703,
            #endregion

            WM_ADSPROP_NOTIFY_PAGEINIT = 0x084D,
            WM_ADSPROP_NOTIFY_PAGEHWND = 0x084E,
            WM_ADSPROP_NOTIFY_CHANGE = 0x084F,
            WM_ADSPROP_NOTIFY_APPLY = 0x0850,
            WM_ADSPROP_NOTIFY_SETFOCUS = 0x0851,
            WM_ADSPROP_NOTIFY_FOREGROUND = 0x0852,
            WM_ADSPROP_NOTIFY_EXIT = 0x0853,
            WM_ADSPROP_NOTIFY_ERROR = 0x0856,

            #region "SysTreeView32" TreeView common control messages
            TVM_INSERTITEMA = 0x1100,
            TVM_DELETEITEM = 0x1101,
            TVM_EXPAND = 0x1102,
            TVM_GETITEMRECT = 0x1104,
            TVM_GETCOUNT = 0x1105,
            TVM_GETINDENT = 0x1106,
            TVM_SETINDENT = 0x1107,
            TVM_GETIMAGELIST = 0x1108,
            TVM_SETIMAGELIST = 0x1109,
            TVM_GETNEXTITEM = 0x110A,
            TVM_SELECTITEM = 0x110B,
            TVM_GETITEMA = 0x110C,
            TVM_SETITEMA = 0x110D,
            TVM_EDITLABELA = 0x110E,
            TVM_GETEDITCONTROL = 0x110F,
            TVM_GETVISIBLECOUNT = 0x1110,
            TVM_HITTEST = 0x1111,
            TVM_CREATEDRAGIMAGE = 0x1112,
            TVM_SORTCHILDREN = 0x1113,
            TVM_ENSUREVISIBLE = 0x1114,
            TVM_SORTCHILDRENCB = 0x1115,
            TVM_ENDEDITLABELNOW = 0x1116,
            TVM_GETISEARCHSTRINGA = 0x1117,
            TVM_SETTOOLTIPS = 0x1118,
            TVM_GETTOOLTIPS = 0x1119,
            TVM_SETINSERTMARK = 0x111A,
            TVM_SETITEMHEIGHT = 0x111B,
            TVM_GETITEMHEIGHT = 0x111C,
            TVM_SETBKCOLOR = 0x111D,
            TVM_SETTEXTCOLOR = 0x111E,
            TVM_GETBKCOLOR = 0x111F,
            TVM_GETTEXTCOLOR = 0x1120,
            TVM_SETSCROLLTIME = 0x1121,
            TVM_GETSCROLLTIME = 0x1122,
            TVM_SETBORDER = 0x1123,
            TVM_SETINSERTMARKCOLOR = 0x1125,
            TVM_GETINSERTMARKCOLOR = 0x1126,
            TVM_GETITEMSTATE = 0x1127,
            TVM_SETLINECOLOR = 0x1128,
            TVM_GETLINECOLOR = 0x1129,
            TVM_MAPACCIDTOHTREEITEM = 0x112A,
            TVM_MAPHTREEITEMTOACCID = 0x112B,
            TVM_SETEXTENDEDSTYLE = 0x112C,
            TVM_GETEXTENDEDSTYLE = 0x112D,
            TVM_INSERTITEMW = 0x1132,
            TVM_SETHOT = 0x113A,
            TVM_SETAUTOSCROLLINFO = 0x113B,
            TVM_GETITEMW = 0x113E,
            TVM_SETITEMW = 0x113F,
            TVM_GETISEARCHSTRINGW = 0x1140,
            TVM_EDITLABELW = 0x1141,
            TVM_GETSELECTEDCOUNT = 0x1146,
            TVM_SHOWINFOTIP = 0x1147,
            TVM_GETITEMPARTRECT = 0x1148,
            #endregion

            #region "SysHeader32" Header bar control messages
            HDM_GETITEMCOUNT = 0x1200,
            HDM_INSERTITEMA = 0x1201,
            HDM_DELETEITEM = 0x1202,
            HDM_GETITEMA = 0x1203,
            HDM_SETITEMA = 0x1204,
            HDM_LAYOUT = 0x1205,
            HDM_HITTEST = 0x1206,
            HDM_GETITEMRECT = 0x1207,
            HDM_SETIMAGELIST = 0x1208,
            HDM_GETIMAGELIST = 0x1209,
            HDM_INSERTITEMW = 0x120A,
            HDM_GETITEMW = 0x120B,
            HDM_SETITEMW = 0x120C,
            HDM_ORDERTOINDEX = 0x120F,
            HDM_CREATEDRAGIMAGE = 0x1210,
            HDM_GETORDERARRAY = 0x1211,
            HDM_SETORDERARRAY = 0x1212,
            HDM_SETHOTDIVIDER = 0x1213,
            HDM_SETBITMAPMARGIN = 0x1214,
            HDM_GETBITMAPMARGIN = 0x1215,
            HDM_SETFILTERCHANGETIMEOUT = 0x1216,
            HDM_EDITFILTER = 0x1217,
            HDM_CLEARFILTER = 0x1218,
            HDM_GETITEMDROPDOWNRECT = 0x1219,
            HDM_GETOVERFLOWRECT = 0x121A,
            HDM_GETFOCUSEDITEM = 0x121B,
            HDM_SETFOCUSEDITEM = 0x121C,
            #endregion

            #region "SysTabControl32" Tab control messages
            TCM_GETIMAGELIST = 0x1302,
            TCM_SETIMAGELIST = 0x1303,
            TCM_GETITEMCOUNT = 0x1304,
            TCM_GETITEMA = 0x1305,
            TCM_SETITEMA = 0x1306,
            TCM_INSERTITEMA = 0x1307,
            TCM_DELETEITEM = 0x1308,
            TCM_DELETEALLITEMS = 0x1309,
            TCM_GETITEMRECT = 0x130A,
            TCM_GETCURSEL = 0x130B,
            TCM_SETCURSEL = 0x130C,
            TCM_HITTEST = 0x130D,
            TCM_SETITEMEXTRA = 0x130E,
            TCM_ADJUSTRECT = 0x1328,
            TCM_SETITEMSIZE = 0x1329,
            TCM_REMOVEIMAGE = 0x132A,
            TCM_SETPADDING = 0x132B,
            TCM_GETROWCOUNT = 0x132C,
            TCM_GETTOOLTIPS = 0x132D,
            TCM_SETTOOLTIPS = 0x132E,
            TCM_GETCURFOCUS = 0x132F,
            TCM_SETCURFOCUS = 0x1330,
            TCM_SETMINTABWIDTH = 0x1331,
            TCM_DESELECTALL = 0x1332,
            TCM_HIGHLIGHTITEM = 0x1333,
            TCM_SETEXTENDEDSTYLE = 0x1334,
            TCM_GETEXTENDEDSTYLE = 0x1335,
            TCM_GETITEMW = 0x133C,
            TCM_SETITEMW = 0x133D,
            TCM_INSERTITEMW = 0x133E,
            #endregion

            #region "SysPager" page scroller common control messages
            PGM_SETCHILD = 0x1401,
            PGM_RECALCSIZE = 0x1402,
            PGM_FORWARDMOUSE = 0x1403,
            PGM_SETBKCOLOR = 0x1404,
            PGM_GETBKCOLOR = 0x1405,
            PGM_SETBORDER = 0x1406,
            PGM_GETBORDER = 0x1407,
            PGM_SETPOS = 0x1408,
            PGM_GETPOS = 0x1409,
            PGM_SETBUTTONSIZE = 0x140A,
            PGM_GETBUTTONSIZE = 0x140B,
            PGM_GETBUTTONSTATE = 0x140C,
            PGM_SETSCROLLINFO = 0x140D,
            #endregion

            #region "Edit" control messages
            EM_SETCUEBANNER = 0x1501,
            EM_GETCUEBANNER = 0x1502,
            EM_SHOWBALLOONTIP = 0x1503,
            EM_HIDEBALLOONTIP = 0x1504,
            EM_SETHILITE = 0x1505,
            EM_GETHILITE = 0x1506,
            EM_NOSETFOCUS = 0x1507,
            EM_TAKEFOCUS = 0x1508,
            #endregion

            #region "Button" common control messages
            BCM_GETIDEALSIZE = 0x1601,
            BCM_SETIMAGELIST = 0x1602,
            BCM_GETIMAGELIST = 0x1603,
            BCM_SETTEXTMARGIN = 0x1604,
            BCM_GETTEXTMARGIN = 0x1605,
            BCM_SETDROPDOWNSTATE = 0x1606,
            BCM_SETSPLITINFO = 0x1607,
            BCM_GETSPLITINFO = 0x1608,
            BCM_SETNOTE = 0x1609,
            BCM_GETNOTE = 0x160A,
            BCM_GETNOTELENGTH = 0x160B,
            BCM_SETSHIELD = 0x160C,
            #endregion

            #region "Combobox" control messages
            CB_SETMINVISIBLE = 0x1701,
            CB_GETMINVISIBLE = 0x1702,
            CB_SETCUEBANNER = 0x1703,
            CB_GETCUEBANNER = 0x1704,
            #endregion

            #region Common control shared messages
            CCM_SETBKCOLOR = 0x2001,
            CCM_SETCOLORSCHEME = 0x2002,
            CCM_GETCOLORSCHEME = 0x2003,
            CCM_GETDROPTARGET = 0x2004,
            CCM_SETUNICODEFORMAT = 0x2005,
            CCM_GETUNICODEFORMAT = 0x2006,
            CCM_SETVERSION = 0x2007,
            CCM_GETVERSION = 0x2008,
            CCM_SETNOTIFYWINDOW = 0x2009,
            CCM_SETWINDOWTHEME = 0x200B,
            CCM_DPISCALE = 0x200C,
            CCM_LAST = 0x2200,
            #endregion

            WM_APP = 0x8000,
            WM_REFLECT_BASE = 0xBC00,
            WM_RASDIALEVENT = 0xCCCD
        }
        #endregion
        #endregion
    }
}
