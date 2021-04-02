using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

//
// Author: Adam A. Zgagacz
// Copyright: ProXoft L.L.C. 2013
// Home Page: http://www.proxoft.com
// https://www.codeproject.com/Articles/624997/Enhanced-Scrollbar
//
//ScrollBarEnhanced control is the replacement for standard VScrollBar and HscrollBar controls from Visual Studio
//It mimics their behavior wile adding at the some time substantial amount of new functionality:
//- extended domain of values to decimal
//- added customizable graphical bookmarks
//- added customizable dynamic ToolTips.
//- since context menu is accessible as property, it is now easily customizable
//- extended amount of information passed to event handlers and added new events
//- added new property 'BookmarksOnTop' if set to true Bookmarks are displayed as topmost items
//
//History of changes:
//August 6th, 2013:
//Bug fix for Minimum!=0
//August 8th, 2013:
//Added context menu for scroll commands
//August 11th, 2013
//Added ValueRangeScrollBarBookmark
//September 13th, 2013
//Added documentation comments
//December 12th, 2013
//Cleanup comments and simplifying the code
//Correcting runtime editor to allow adding bookmarks at runtime (previously it was impossible due to exception: cannot instantiate abstract class)

namespace ProXoft.WinForms
{
    /// <summary>
    ///
    /// </summary>
    [RefreshProperties(RefreshProperties.All)]
    public class ScrollBarEnhanced : UserControl
    {
        #region Private fields
        public enum MouseButtonState { Released, Pressed }

        private MouseButtonState MouseUpDownStatus = MouseButtonState.Released;
        EnhancedScrollBarMouseLocation MouseScrollBarArea = EnhancedScrollBarMouseLocation.OutsideScrollBar;
        private Point MouseActivePointPoint;
        private int MouseRelativeYFromThumbTop=0;

        decimal PrevouslyReportedHotValue = -1;
        decimal HotValue = -1;

        decimal PreviousValue = -1;

        private decimal m_Minimum = 0;
        private decimal m_Maximum = 100;
        private decimal m_Value = 0;

        private bool m_disposed;

        private ToolTip toolTip1;
        private Timer timerMouseDownRepeater;

        Dictionary<Color, Brush> m_BrushesCache = new Dictionary<Color, Brush>();
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem scrollHereToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem topToolStripMenuItem;
        private ToolStripMenuItem bottomToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem pageUpToolStripMenuItem;
        private ToolStripMenuItem pageDownToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem scrollUpToolStripMenuItem;
        private ToolStripMenuItem scrollDownToolStripMenuItem;
        Dictionary<Color, Pen> m_PensCache = new Dictionary<Color, Pen>();

        [NonSerialized]
        ObservableCollection<ScrollBarBookmark> m_Bookmarks = null;

        private Orientation m_Orientation = Orientation.Vertical;
        private bool m_BookmarksOnTop = true;

        //Every time mouse is pressed down this is populated to be used by the repeat timer
        MouseEventArgs m_MouseDownArgs = null;

        #endregion

        #region Public Events

        /// <summary>
        /// Fires every time mouse is clicked over track area.
        /// </summary>
        [Description("Fires every time mouse is clicked over track area.")]
        public new event EventHandler<EnhancedMouseEventArgs> MouseClick = null;

        /// <summary>
        /// Fires every time mouse moves over track area.
        /// </summary>
        [Description("Fires every time mouse moves over track area.")]
        public new event EventHandler<EnhancedMouseEventArgs> MouseMove = null;

        /// <summary>
        /// Occurs each time scrollbar orientation has changed.
        /// </summary>
        [Description("Occurs each time scrollbar orientation has changed.")]
        public event EventHandler OrientationChanged = null;

        /// <summary>
        /// Occurs every time scrollbar orientation is about to change.
        /// </summary>
        [Description("Occurs every time scrollbar orientation is about to change.")]
        public event EventHandler<CancelEventArgs> OrientationChanging = null;

        /// <summary>
        ///
        /// </summary>
        public new event EventHandler<EnhancedScrollEventArgs> Scroll = null;

        /// <summary>
        /// Fired every time mouse moves over bookmark (or multiple bookmarks).
        /// Allows to overwrite default ToolTip value.
        /// </summary>
        [Description("Allows to overwrite default ToolTip value.")]
        public event EventHandler<TooltipNeededEventArgs> ToolTipNeeded = null;

        /// <summary>
        /// Fired every time <c>Value</c> of the ScrollBar changes.
        /// </summary>
        [Description("Occurs every time scrollbar value changes.")]
        public event EventHandler ValueChanged = null;

        #endregion

        #region Public Methods -- Chuck

        public void RaiseScrollEvent(decimal oldValue, decimal newValue)
        {
            OnScroll(newValue, oldValue, ScrollEventType.ThumbPosition);
        }

        public void ScrollHome()     { topToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void ScrollEnd()      { bottomToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void ScrollPageUp()   { pageUpToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void ScrollPageDown() { pageDownToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void ScrollLineUp()   { scrollUpToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void ScrollLineDown() { scrollDownToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void ScrollPixelUp()
        {
            OnScroll(Value - 1, Value, ScrollEventType.SmallDecrement);
            Value -= 1;
            OnScroll(Value, Value, ScrollEventType.EndScroll);
        }
        public void ScrollPixelDown()
        {
            OnScroll(Value + 1, Value, ScrollEventType.SmallIncrement);
            Value += 1;
            OnScroll(Value, Value, ScrollEventType.EndScroll);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(true), Category("Enhanced")]
        [Description("Set to 'false' to disable scrollbar context menu.")]
        public bool ShowContextMenu
        {
            get { return this.ContextMenu != null; }
            set { this.ContextMenuStrip = value ? this.contextMenuStrip1 : null; }
        }

        private bool _showTooltips = true;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(true), Category("Enhanced")]
        [Description("Set to 'false' to disable all scrollbar tooltips.")]
        public bool ShowToolTips { get { return _showTooltips; } set { _showTooltips = value; } }

        #endregion

        #region Constructor and related

        /// <summary>
        /// Constructor. Initialize properties.
        /// </summary>
        public ScrollBarEnhanced()
        {

            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            //Initialize default values for properties
            Bookmarks = new ObservableCollection<ScrollBarBookmark>();
            ShowTooltipOnMouseMove = false;
            InitialDelay =800;
            RepeatRate =62;
            LargeChange = 10;
            SmallChange = 1;
            QuickBookmarkNavigation = true;

            timerMouseDownRepeater.Tick += new EventHandler(timerMouseDownRepeater_Tick);

            Dock = DockStyle.None;
            this.ClientSize = new Size(SystemInformation.VerticalScrollBarWidth, this.ClientSize.Height);
            this.Width = SystemInformation.VerticalScrollBarWidth;

            Orientation = Orientation.Vertical;
        }

        /// <summary>
        /// Generates repeat events when mouse is pressed and hold.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timerMouseDownRepeater_Tick(object sender, EventArgs e)
        {
            base.OnMouseDown(m_MouseDownArgs);

            DoMouseDown(m_MouseDownArgs);

            if (timerMouseDownRepeater.Enabled)
                timerMouseDownRepeater.Interval = RepeatRate;
            else
                timerMouseDownRepeater.Interval = InitialDelay;

            //Do this only if not dragging thumb
            if (MouseScrollBarArea != EnhancedScrollBarMouseLocation.Thumb)
                timerMouseDownRepeater.Enabled = true;
            else
                timerMouseDownRepeater.Enabled = false;
        }

        private IContainer components;
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.timerMouseDownRepeater = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.scrollHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.topToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.pageUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pageDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.scrollUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scrollDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // contextMenuStrip1
            //
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scrollHereToolStripMenuItem,
            this.toolStripMenuItem1,
            this.topToolStripMenuItem,
            this.bottomToolStripMenuItem,
            this.toolStripMenuItem2,
            this.pageUpToolStripMenuItem,
            this.pageDownToolStripMenuItem,
            this.toolStripMenuItem3,
            this.scrollUpToolStripMenuItem,
            this.scrollDownToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(130, 176);
            //
            // scrollHereToolStripMenuItem
            //
            this.scrollHereToolStripMenuItem.Name = "scrollHereToolStripMenuItem";
            this.scrollHereToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.scrollHereToolStripMenuItem.Text = "Scroll Here";
            this.scrollHereToolStripMenuItem.Click += new System.EventHandler(this.scrollHereToolStripMenuItem_Click);
            //
            // toolStripMenuItem1
            //
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(126, 6);
            //
            // topToolStripMenuItem
            //
            this.topToolStripMenuItem.Name = "topToolStripMenuItem";
            this.topToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.topToolStripMenuItem.Text = "Top";
            this.topToolStripMenuItem.Click += new System.EventHandler(this.topToolStripMenuItem_Click);
            //
            // bottomToolStripMenuItem
            //
            this.bottomToolStripMenuItem.Name = "bottomToolStripMenuItem";
            this.bottomToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.bottomToolStripMenuItem.Text = "Bottom";
            this.bottomToolStripMenuItem.Click += new System.EventHandler(this.bottomToolStripMenuItem_Click);
            //
            // toolStripMenuItem2
            //
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(126, 6);
            //
            // pageUpToolStripMenuItem
            //
            this.pageUpToolStripMenuItem.Name = "pageUpToolStripMenuItem";
            this.pageUpToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.pageUpToolStripMenuItem.Text = "Page Up";
            this.pageUpToolStripMenuItem.Click += new System.EventHandler(this.pageUpToolStripMenuItem_Click);
            //
            // pageDownToolStripMenuItem
            //
            this.pageDownToolStripMenuItem.Name = "pageDownToolStripMenuItem";
            this.pageDownToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.pageDownToolStripMenuItem.Text = "Page Down";
            this.pageDownToolStripMenuItem.Click += new System.EventHandler(this.pageDownToolStripMenuItem_Click);
            //
            // toolStripMenuItem3
            //
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(126, 6);
            //
            // scrollUpToolStripMenuItem
            //
            this.scrollUpToolStripMenuItem.Name = "scrollUpToolStripMenuItem";
            this.scrollUpToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.scrollUpToolStripMenuItem.Text = "Scroll Up";
            this.scrollUpToolStripMenuItem.Click += new System.EventHandler(this.scrollUpToolStripMenuItem_Click);
            //
            // scrollDownToolStripMenuItem
            //
            this.scrollDownToolStripMenuItem.Name = "scrollDownToolStripMenuItem";
            this.scrollDownToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.scrollDownToolStripMenuItem.Text = "Scroll Down";
            this.scrollDownToolStripMenuItem.Click += new System.EventHandler(this.scrollDownToolStripMenuItem_Click);
            //
            // ScrollBarEnhanced
            //
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Name = "ScrollBarEnhanced";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        /// <summary>
        /// Dispose overridden method. When called from the host <c>disposing</c> parameter is <b>true</b>.
        /// When called from the finalize parameter is <b>false</b>.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    if (components != null)
                    {
                        components.Dispose();
                    }
                }
                foreach (Brush brush in m_BrushesCache.Values)
                    brush.Dispose();

                foreach (Pen pen in m_PensCache.Values)
                    pen.Dispose();

                m_disposed = true;

                base.Dispose();

            }
        }

        /// <summary>
        /// Every bookmark is added or removed, scrollbar has to refresh itself.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Bookmarks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Invalidate();
        }

        #endregion

        #region Public Properties

        //We don't need BackColor to be exposed as property, so let's try to hide it form property list
        /// <summary>
        /// BackColor doesn't have any meaning for the ScrollBar.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)]
        public new Color BackColor { get; set; }

        /// <summary>
        /// List of bookmarks associated with the ScrollBar.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [DefaultValue(null), Category("Enhanced")]
        [Description("List of ScrollBar bookmarks")]
        [NotifyParentProperty(true)]
        [Editor(typeof(BookmarksCollectionEditor), typeof(UITypeEditor))]
        public ObservableCollection<ScrollBarBookmark> Bookmarks
        {
            get
            {
                if (m_Bookmarks == null)
                    m_Bookmarks = new ObservableCollection<ScrollBarBookmark>();
                return m_Bookmarks;
            }
            set
            {
                if (m_Bookmarks!=null)
                    m_Bookmarks.CollectionChanged -= new NotifyCollectionChangedEventHandler(Bookmarks_CollectionChanged);

                m_Bookmarks = value;

                if (m_Bookmarks!=null)
                    m_Bookmarks.CollectionChanged += new NotifyCollectionChangedEventHandler(Bookmarks_CollectionChanged);

            }
        }

        /// <summary>
        /// If set to <b>true</b> bookmarks are displayed as the topmost elements. If <b>false</b> thumb covers bookmarks that might be hidden beneath.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(true), Category("Enhanced")]
        [Description("If set to 'true' bookmarks are displayed as the topmost elements. If 'false' thumb covers bookmarks that might be hidden beneath.")]
        public bool BookmarksOnTop
        {
            get { return m_BookmarksOnTop; }
            set
            {
                if (m_BookmarksOnTop != value)
                {
                    m_BookmarksOnTop = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Delay in milliseconds to start autorepeat behavior when mouse is pressed down and hold.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(400), Category("Enhanced")]
        [Description("Delay in milliseconds to start autorepeat behavior when mouse is pressed down and hold.")]
        public int InitialDelay { set; get; }

        /// <summary>
        /// Large scrollbar change.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(10), Category("Enhanced")]
        [Description("Large scrollbar change.")]
        public decimal LargeChange {set; get;}

        /// <summary>
        /// "Maximum scrollbar value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(100), Category("Enhanced")]
        [Description("Maximum scrollbar value.")]
        public decimal Maximum
        {
            get { return m_Maximum; }
            set
            {
                if (value < Minimum)
                    throw new ArgumentException("Minimum has to be less or equal Maximum", "Minimum");

                //The following line will throw exception is range is more than decimal.MaxValue
                decimal rangeTestValue = value - Minimum;

                m_Maximum = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Minimum scrollbar value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(0), Category("Enhanced")]
        [Description("Minimum scrollbar value.")]
        public decimal Minimum
        {
            get { return m_Minimum; }
            set
            {
                if (Maximum < value)
                    throw new ArgumentException("Minimum has to be less or equal Maximum", "Minimum");

                //The following line will throw exception is range is more than decimal.MaxValue
                decimal rangeTestValue = Maximum - value;

                m_Minimum = value;
                Invalidate();
            }
        }

       /// <summary>
        /// ScrollBar orientation.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(Orientation.Vertical), Category("Enhanced")]
        [Description("ScrollBar orientation.")]
        public Orientation Orientation
        {
            set
            {
                if (OrientationChanging != null)
                {
                    CancelEventArgs ea = new CancelEventArgs(false);
                    OrientationChanging(this, ea);
                    if (ea.Cancel)
                        return;
                }
                if (m_Orientation != value)
                {
                    m_Orientation = value;

                    //Switch width with height
                    int tmpWidth = this.Width;
                    this.Width = this.Height;
                    this.Height = tmpWidth;

                    //in all bookmarks switch width with height (only basic shape bookmarks)
                    foreach (ScrollBarBookmark Bookmark in Bookmarks)
                    {
                        if (Bookmark is BasicShapeScrollBarBookmark)
                        {
                            BasicShapeScrollBarBookmark BSBookmark = (BasicShapeScrollBarBookmark)Bookmark;
                            tmpWidth = BSBookmark.Width;
                            BSBookmark.Width = BSBookmark.Height;
                            BSBookmark.Height = tmpWidth;
                        }
                    }
                    if (Orientation == Orientation.Vertical)
                    {
                        base.MinimumSize = new Size(0, 2 * SystemInformation.VerticalScrollBarArrowHeight + ThumbLength);
                        switch (this.Dock)
                        {
                            case DockStyle.Bottom:
                                Dock = DockStyle.Right; break;
                            case DockStyle.Top:
                                Dock = DockStyle.Left; break;
                        }
                    }
                    else
                    {
                        base.MinimumSize = new Size(2 * SystemInformation.HorizontalScrollBarArrowWidth + ThumbLength, 0);
                        switch (this.Dock)
                        {
                            case DockStyle.Right:
                                Dock = DockStyle.Bottom; break;
                            case DockStyle.Left:
                                Dock = DockStyle.Top; break;
                        }

                    }
                    if (OrientationChanged != null)
                    {
                        OrientationChanged(this, EventArgs.Empty);
                    }
                }

            }
            get
            {
                return m_Orientation;
            }
        }

        /// <summary>
        /// "When <b>true</b>, clicking on bookmark image, changes scrollbar value to the bookmark value
        /// (moves thumb position to bookmark value).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(true), Category("Enhanced")]
        [Description("When 'true', clicking on bookmark image, changes scrollbar value to the bookmark value.")]
        public bool QuickBookmarkNavigation
        { set; get; }

        /// <summary>
        /// Delay in milliseconds between autorepeat MouseDown events when mouse is pressed down and hold.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(62), Category("Enhanced")]
        [Description("Delay in milliseconds between autorepeat MouseDown events when mouse is pressed down and hold.")]
        public int RepeatRate { set; get; }

        /// <summary>
        /// When set to <b>true</b>, allows to show ToolTip when mouse moves over scrollbar area.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Enhanced")]
        [Description("When set to 'true', allows to show ToolTip when mouse moves over scrollbar area.")]
        public bool ShowTooltipOnMouseMove { set; get; }

        /// <summary>
        /// Small bookmark change.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(1), Category("Enhanced")]
        [Description("Small change.")]
        public decimal SmallChange {set; get;}

        /// <summary>
        /// Bookmark value. Determines current thumb position.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(0), Category("Enhanced")]
        [Description("Value")]
        public decimal Value
        {
            get { return m_Value; }
            set
            {
                if (value < Minimum)
                    m_Value = Minimum;
                else if (value > Maximum)
                    m_Value = Maximum;
                else
                   m_Value = value;

                OnValueChanged();
                Invalidate();
            }
        }

        #endregion

        #region Overridden events

        /// <summary>
        /// What should happen here:
        /// 1. Save information that mouse is down
        /// 2. Call timer event handler (it will repeat periodically MouseDown events as long as mouse is down)
        /// </summary>
        /// <param name="e">Standard <c>MouseEventArgs</c>.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //Save arguments passed to mouse down
            m_MouseDownArgs = e;
            MouseUpDownStatus = MouseButtonState.Pressed;

            //Make sure timer is disabled
            timerMouseDownRepeater.Enabled = false;

            //Call timer event; it will call DoMouseDown every RepeatRate interval
            timerMouseDownRepeater_Tick(null, EventArgs.Empty);
        }

        /// <summary>
        /// This private methods called from repeater timer event handler
        /// </summary>
        /// <param name="e"></param>
        private void DoMouseDown(MouseEventArgs e)
        {
            //1. Save info about fact that mouse is presses
            //2. Save scrollbar area where mouse was pressed
            if (Orientation == Orientation.Vertical)
                MouseScrollBarArea = MouseLocation(e.Y, out MouseRelativeYFromThumbTop);
            else
                MouseScrollBarArea = MouseLocation(e.X, out MouseRelativeYFromThumbTop);

            //3. Save exact location of mouse press
            MouseActivePointPoint = e.Location;

            //4. Calculate ScrollEvent arguments and adjust Value if needed
            decimal NewValue = Value;
            ScrollEventType et;
            List<ScrollBarBookmark> bookmarksUnder =  BookmarksUnderPosition(e.X, e.Y);
            if ((bookmarksUnder.Count > 0) && (QuickBookmarkNavigation))
            {
                et = ScrollEventType.ThumbPosition;
                ScrollBarBookmark topMostBookmark = bookmarksUnder[bookmarksUnder.Count - 1];
                if (topMostBookmark is ValueRangeScrollBarBookmark)
                    NavigateTo(HotValue);
                else
                    Value = topMostBookmark.Value;
            }
            else
            {
                switch (MouseScrollBarArea)
                {
                    case EnhancedScrollBarMouseLocation.BottomOrRightArrow:
                    case EnhancedScrollBarMouseLocation.BottomOrRightTrack:
                        if (MouseScrollBarArea == EnhancedScrollBarMouseLocation.BottomOrRightArrow)
                        {
                            NewValue += SmallChange;
                            et = ScrollEventType.SmallIncrement;
                        }
                        else    // EnhancedScrollBarMouseLocation.bottomTrack
                        {
                            NewValue += LargeChange;
                            et = ScrollEventType.LargeIncrement;
                        }
                        if (NewValue >= Maximum)
                        {
                            NewValue = Maximum;
                            et = ScrollEventType.Last;
                        }
                        OnScroll(NewValue, Value, et);
                        break;
                    case EnhancedScrollBarMouseLocation.Thumb:
                        OnScroll(Value, Value, ScrollEventType.ThumbTrack);
                        break;
                    case EnhancedScrollBarMouseLocation.TopOrLeftArrow:
                    case EnhancedScrollBarMouseLocation.TopOrLeftTrack:
                        if (MouseScrollBarArea == EnhancedScrollBarMouseLocation.TopOrLeftArrow)
                        {
                            NewValue -= SmallChange;
                            et = ScrollEventType.SmallDecrement;
                        }
                        else
                        {
                            NewValue -= LargeChange;
                            et = ScrollEventType.LargeIncrement;
                        }
                        if (NewValue <= Minimum)
                        {
                            NewValue = Minimum;
                            et = ScrollEventType.First;
                        }
                        OnScroll(NewValue, Value, et);
                        break;
                }
                Value = NewValue;
            }

            //3. Repaint
            Invalidate();

        }

        //--CHUCK OnMouseWheel() disabled. Allow parent handle this so we don't have to worry about who has focus.
        /// <summary>
        /// Captures mouse wheel actions and translates them to small decrement events.
        /// </summary>
        /// <param name="e"></param>
        //protected override void OnMouseWheel(MouseEventArgs e)
        //{
        //    int ScrollStep = e.Delta / SystemInformation.MouseWheelScrollDelta;

        //    //Calculate new value
        //    decimal NewValue = Value - SmallChange * ScrollStep;

        //    if (NewValue<Value)
        //        OnScroll(NewValue, Value, ScrollEventType.SmallDecrement);
        //    else
        //        OnScroll(NewValue, Value, ScrollEventType.SmallIncrement);

        //    Value = NewValue;
        //    OnScroll(Value, Value, ScrollEventType.EndScroll);

        //    base.OnMouseWheel(e);
        //}

        /// <summary>
        /// MouseClick override. Calls MouseClick event handled with enhanced arguments.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            //enrich arguments and call back the host
            OnMouseClick(e, Value, BookmarksUnderPosition(e.X, e.Y));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {

            base.OnMouseMove(e);

            //Mouse is down and is over thumb
            if ((MouseUpDownStatus == MouseButtonState.Pressed) && (MouseScrollBarArea == EnhancedScrollBarMouseLocation.Thumb))
            {
                //Dragging thumb button

                //Calculate Value based on new e.Y
                decimal NewValue;
                if (Orientation == Orientation.Vertical)
                    NewValue = ThumbTopPosition2Value(e.Y - MouseRelativeYFromThumbTop);
                else
                    NewValue = ThumbTopPosition2Value(e.X - MouseRelativeYFromThumbTop);

                if (NewValue < Minimum) NewValue = Minimum;
                if (NewValue > Maximum) NewValue = Maximum;

                //Call onScroll event
                OnScroll(NewValue, Value, ScrollEventType.ThumbTrack);

                //Assign new value
                Value = NewValue;  //New value will move scrollbar to proper position

                //Refresh display
                this.Invalidate();
            }
            else
            {
                string toolTip = "";
                //Moving mouse over different areas

                //1. Save current (before mouse move) mouse location
                EnhancedScrollBarMouseLocation oldLocation = MouseScrollBarArea;

                int tmpInt;
                EnhancedScrollBarMouseLocation newLocation;

                int TrackPosition;
                if (Orientation == Orientation.Vertical)
                {
                    newLocation = MouseLocation(e.Y, out tmpInt);
                    TrackPosition = e.Y - SystemInformation.VerticalScrollBarArrowHeight;
                    switch (newLocation)
                    {
                        case EnhancedScrollBarMouseLocation.TopOrLeftArrow:
                            TrackPosition = 0;
                            break;
                        case EnhancedScrollBarMouseLocation.BottomOrRightArrow:
                            TrackPosition = ClientSize.Height - 2 * SystemInformation.VerticalScrollBarArrowHeight;
                            break;

                    }
                }
                else
                {
                    newLocation = MouseLocation(e.X, out tmpInt);

                    TrackPosition = e.X - SystemInformation.VerticalScrollBarArrowHeight;
                    switch (newLocation)
                    {
                        case EnhancedScrollBarMouseLocation.TopOrLeftArrow:
                            TrackPosition = 0;
                            break;
                        case EnhancedScrollBarMouseLocation.BottomOrRightArrow:
                            TrackPosition = ClientSize.Width - 2 * SystemInformation.HorizontalScrollBarArrowWidth;
                            break;

                    }
                }
                HotValue = (Maximum - Minimum) * ((decimal)TrackPosition / TrackLength)+ Minimum;
                MouseScrollBarArea = newLocation;

                if ((TrackPosition < 0) || (TrackPosition > TrackLength))
                    toolTip1.Hide(this);
                else
                {
                    //Get list of bookmarks under cursor
                    List<ScrollBarBookmark> bookmarksOver =  BookmarksUnderPosition(e.X, e.Y);

                    if (ShowToolTips)
                    {
                        if (PrevouslyReportedHotValue != HotValue)
                        {
                            PrevouslyReportedHotValue = HotValue;
                            string defaultToolTip = Name + " " + HotValue.ToString("###,##0");
                            if ((ToolTipNeeded != null))
                            {
                                TooltipNeededEventArgs ea = new TooltipNeededEventArgs(HotValue, defaultToolTip, bookmarksOver);
                                ToolTipNeeded(this, ea);
                                toolTip = ea.ToolTip;
                            }
                            else  //display default value ToolTip
                            {
                                toolTip = defaultToolTip;
                            }

                            if (this.toolTip1.GetToolTip(this) != toolTip)
                                this.toolTip1.SetToolTip(this, toolTip);
                        }
                    }

                    //Find section of scrollbar the mouse is moving over

                    //Call the host to notify about the MouseMove event		
                    OnMouseMove(e, HotValue, bookmarksOver);


                    //If moving over different area- refresh display
                    if (oldLocation != MouseScrollBarArea)
                        this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Forces repaint of ScrollBar when mouse moves outside of ScrollBar area.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(EventArgs e)
        {
            MouseUpDownStatus = MouseButtonState.Released;
            MouseScrollBarArea = EnhancedScrollBarMouseLocation.OutsideScrollBar;
            timerMouseDownRepeater.Enabled = false;

            base.OnMouseLeave(e);
            this.Invalidate();
        }

        /// <summary>
        /// Fires <c>Scroll</c> events and refreshes ScrollBar display.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            timerMouseDownRepeater.Enabled = false;
            MouseUpDownStatus = MouseButtonState.Released;

            switch (MouseScrollBarArea)
            {
                case EnhancedScrollBarMouseLocation.BottomOrRightArrow:
                case EnhancedScrollBarMouseLocation.TopOrLeftArrow:
                case EnhancedScrollBarMouseLocation.BottomOrRightTrack:
                case EnhancedScrollBarMouseLocation.TopOrLeftTrack:
                    OnScroll(Value, Value, ScrollEventType.EndScroll);
                    break;
                case EnhancedScrollBarMouseLocation.Thumb:
                    OnScroll(Value, Value, ScrollEventType.ThumbPosition);
                    OnScroll(Value, Value, ScrollEventType.EndScroll);
                    break;
            }
            Invalidate();
        }

        #endregion

        #region Private helpers/wrappers of public events

        private void OnValueChanged()
        {
            if ((ValueChanged != null) && (this.Value != PreviousValue))
            {
                PreviousValue = this.Value;
                ValueChanged(this, new EventArgs());
            }
        }

        private void OnMouseClick(MouseEventArgs mouseArgs, decimal Value, List<ScrollBarBookmark> BookmarksOver)
        {
            if (MouseClick != null)
            {
                //Call event handler
                //Aug 07, 2013. Changed to pass HotValue instead Value.
                //This makes more since since value cannot be examined directly
                //as property of ScrollBarEnhanced
                EnhancedMouseEventArgs e = new EnhancedMouseEventArgs(HotValue, mouseArgs, BookmarksOver, MouseScrollBarArea);
                 MouseClick(this, e);
            }

        }

        private void OnMouseMove(MouseEventArgs ea, decimal HotValue, List<ScrollBarBookmark> BookmarksOver)
        {
            if (MouseMove!=null)
            {
                //Call event handler
                EnhancedMouseEventArgs e = new EnhancedMouseEventArgs(HotValue, ea, BookmarksOver, MouseScrollBarArea);
                MouseMove(this, e);
            }
        }

        private void OnScroll(decimal newVal, decimal oldVal, ScrollEventType scrollEventType )
        {
            if (Scroll != null)
            {
                ScrollOrientation ScrollOrientation;
                if (Orientation == Orientation.Horizontal)
                    ScrollOrientation = ScrollOrientation.HorizontalScroll;
                else
                    ScrollOrientation = ScrollOrientation.VerticalScroll;

                //Make sure NewVal is within valid range
                newVal = Math.Max(Math.Min(Maximum, newVal), Minimum);

                EnhancedScrollEventArgs ea = new EnhancedScrollEventArgs(oldVal, newVal,scrollEventType, ScrollOrientation);

                Scroll(this, ea);
            }
        }

        #endregion

        #region OnPaint override and DrawBookmark methods

        private void DrawBookmark(Graphics graphics, ScrollBarBookmark bookmark)
        {
            BasicShapeScrollBarBookmark shapeBookmark = null;
            ImageScrollBarBookmark imageBookmark=null;

            if (bookmark is ValueRangeScrollBarBookmark)
            {
                shapeBookmark = (ValueRangeScrollBarBookmark)bookmark;
                int bookmarkLength =(int)((( ((ValueRangeScrollBarBookmark)shapeBookmark).EndValue - shapeBookmark.Value) / (Maximum - Minimum)) * TrackLength);
                if (bookmarkLength == 0) bookmarkLength = 1;

                //For ValueRangeBook recalculate size in pixels
                if (Orientation == Orientation.Vertical)
                    bookmark.Height = bookmarkLength;  //calculate hight for vertical
                else
                    bookmark.Width = bookmarkLength;  //calculate width for horizontal
            }
            else if (bookmark is BasicShapeScrollBarBookmark)
                shapeBookmark = (BasicShapeScrollBarBookmark)bookmark;
            else if (bookmark is ImageScrollBarBookmark)
                imageBookmark = (ImageScrollBarBookmark)bookmark;


            if ((shapeBookmark != null) && (shapeBookmark.Stretch))
            {
                if (Orientation == Orientation.Vertical)
                {
                    bookmark.X = 0;
                    shapeBookmark.Width = this.ClientSize.Width;
                }
                else
                {
                    bookmark.Y = 0;
                    shapeBookmark.Height = this.ClientSize.Height;
                }
            }
            else
            {
                CalculateBookmarkEdgePosition(bookmark);
            }

            //Calculate top Y position of bookmark
            int ArrowLength;
            int BookmarkLength;
            if (Orientation == Orientation.Vertical)
            {
                ArrowLength = SystemInformation.VerticalScrollBarArrowHeight;
                BookmarkLength = bookmark.Height;
            }
            else
            {
                ArrowLength = SystemInformation.HorizontalScrollBarArrowWidth;
                BookmarkLength = bookmark.Width;
            }
            int BookmarkLongPosition;
            if (Maximum == Minimum)
            {
                //Move position out of sight if bookmark value != Minimump
                BookmarkLongPosition = (bookmark.Value == Minimum) ? ArrowLength : -1000;
            }
            else
            {
                BookmarkLongPosition = (int)(TrackLength * (bookmark.Value - Minimum) / (Maximum - Minimum) + ArrowLength);
                if (!(bookmark is ValueRangeScrollBarBookmark))
                    BookmarkLongPosition -= BookmarkLength / 2;
            }

                if (Orientation == Orientation.Vertical)
                    bookmark.Y = BookmarkLongPosition;
                else
                    bookmark.X = BookmarkLongPosition;


            if (imageBookmark != null)
            {
                if (imageBookmark.Image!=null)
                    graphics.DrawImage(imageBookmark.Image, new Point(bookmark.X, bookmark.Y));
            }

            else
            {
                //Make sure that brush needed for drawing is ready to use
                RefreshBrushFromCache(shapeBookmark);

                //Make sure that pen needed for drawing is ready to use
                RefreshPenFromCache(shapeBookmark);

                if ((shapeBookmark != null) && (shapeBookmark.FillBookmarkShape))
                {
                    if (shapeBookmark.Shape == ScrollbarBookmarkShape.Oval)
                        graphics.FillEllipse(shapeBookmark.Brush, new Rectangle(bookmark.X, bookmark.Y, shapeBookmark.Width, shapeBookmark.Height));
                    else
                        graphics.FillRectangle(shapeBookmark.Brush, new Rectangle(bookmark.X, bookmark.Y, shapeBookmark.Width, shapeBookmark.Height));
                }
                else
                {
                    if (shapeBookmark.Shape == ScrollbarBookmarkShape.Oval)
                        graphics.DrawEllipse(shapeBookmark.Pen, new Rectangle(bookmark.X, bookmark.X, shapeBookmark.Width, shapeBookmark.Height));
                    else
                        graphics.DrawRectangle(shapeBookmark.Pen, new Rectangle(bookmark.X, bookmark.X, shapeBookmark.Width, shapeBookmark.Height));
                }
            }
        }

        private int ArrowLegth()
        {
            return Orientation == Orientation.Vertical ? SystemInformation.VerticalScrollBarArrowHeight : SystemInformation.HorizontalScrollBarArrowWidth;
        }

        private void CalculateBookmarkEdgePosition(ScrollBarBookmark bookmark)
        {
            switch (bookmark.Alignment)
            {
                case ScrollBarBookmarkAlignment.LeftOrTop:
                    if (Orientation == Orientation.Vertical)
                        bookmark.X = 0;
                    else
                        bookmark.Y = 0;
                    break;
                case ScrollBarBookmarkAlignment.RightOrBottom:
                    if (Orientation== Orientation.Vertical)
                       bookmark.X = this.ClientSize.Width - bookmark.Width;
                    else
                       bookmark.Y = this.ClientSize.Height - bookmark.Height;
                    break;
                case ScrollBarBookmarkAlignment.Center:
                    if (Orientation == Orientation.Vertical)
                        bookmark.X = (this.ClientSize.Width - bookmark.Width) / 2;
                    else
                        bookmark.Y = (this.ClientSize.Height - bookmark.Height) / 2;

                    break;
            }
        }

        /// <summary>
        /// Overridden OnPaint. Draws all EnhancedScrollBar elements and draws all associated bookmarks.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {

            //Draw top arrow
            Rectangle rec;
            ScrollBarState state = GetScrollBarAreaState(EnhancedScrollBarMouseLocation.TopOrLeftArrow);
            if (Orientation == Orientation.Vertical)
            {
                rec = new Rectangle(0, 0, ClientSize.Width, ArrowLegth());
                switch (state)
                {
                    case ScrollBarState.Disabled:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.UpDisabled); break;
                    case ScrollBarState.Pressed:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.UpPressed); break;
                    case ScrollBarState.Normal:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.UpNormal); break;
                    case ScrollBarState.Hot:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.UpHot); break;
                }
            }
            else
            {
                rec = new Rectangle(0, 0, ArrowLegth(), ClientSize.Height);
                switch (state)
                {
                    case ScrollBarState.Disabled:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.LeftDisabled); break;
                    case ScrollBarState.Pressed:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.LeftPressed); break;
                    case ScrollBarState.Normal:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.LeftNormal); break;
                    case ScrollBarState.Hot:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.LeftHot); break;
                }
            }

            //Draw top track
            int ThumbPos = Value2ThumbTopPosition(Value);
            state = GetScrollBarAreaState(EnhancedScrollBarMouseLocation.TopOrLeftTrack);
            if (Orientation == Orientation.Vertical)
            {
                rec = new Rectangle(0, ArrowLegth() + 1, ClientSize.Width, ThumbPos - ArrowLegth());
                ScrollBarRenderer.DrawUpperVerticalTrack(e.Graphics, rec, state);
            }
            else
            {
                rec = new Rectangle(ArrowLegth() + 1, 0, ThumbPos - ArrowLegth(), ClientSize.Height);
                ScrollBarRenderer.DrawLeftHorizontalTrack(e.Graphics, rec, state);
            }

            //draw thumb
            int nThumbLength = ThumbLength;
            DrawThumb(e.Graphics, nThumbLength, ThumbPos);

            //Draw bottom track
            state = GetScrollBarAreaState(EnhancedScrollBarMouseLocation.BottomOrRightTrack);
            if (Orientation == Orientation.Vertical)
            {
                rec = new Rectangle(0, ThumbPos + nThumbLength, ClientSize.Width, TrackLength + ArrowLegth() - (ThumbPos + nThumbLength));
                ScrollBarRenderer.DrawLowerVerticalTrack(e.Graphics, rec, state);
            }
            else
            {
                rec = new Rectangle(ThumbPos + nThumbLength, 0, TrackLength + ArrowLegth() - (ThumbPos + nThumbLength), ClientSize.Height);
                ScrollBarRenderer.DrawRightHorizontalTrack(e.Graphics, rec, state);
            }

            //Draw bottom arrow
            state = GetScrollBarAreaState(EnhancedScrollBarMouseLocation.BottomOrRightArrow);
            if (Orientation == Orientation.Vertical)
            {
                rec = new Rectangle(0, ClientSize.Height - ArrowLegth(), ClientSize.Width, ArrowLegth());
                switch (state)
                {
                    case ScrollBarState.Disabled:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.DownDisabled); break;
                    case ScrollBarState.Pressed:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.DownPressed); break;
                    case ScrollBarState.Normal:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.DownNormal); break;
                    case ScrollBarState.Hot:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.DownHot); break;
                }
            }
            else
            {
                rec = new Rectangle(ClientSize.Width - ArrowLegth(), 0, ArrowLegth(), ClientSize.Height);
                switch (state)
                {
                    case ScrollBarState.Disabled:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.RightDisabled); break;
                    case ScrollBarState.Pressed:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.RightPressed); break;
                    case ScrollBarState.Normal:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.RightNormal); break;
                    case ScrollBarState.Hot:
                        ScrollBarRenderer.DrawArrowButton(e.Graphics, rec, ScrollBarArrowButtonState.RightHot); break;
                }
            }

            //Draw  bookmarks
            DrawBookmarks(e.Graphics);

            if (!BookmarksOnTop)  //Redraw thumb over bookmarks
                DrawThumb(e.Graphics, nThumbLength, ThumbPos);

        }

        private void DrawBookmarks(Graphics g)
        {
            if (Bookmarks != null)
            {
                foreach (ScrollBarBookmark bk in Bookmarks)
                    DrawBookmark(g, bk);
            }

        }

        private void RefreshBrushFromCache(BasicShapeScrollBarBookmark shapeBookmark)
        {
            if (shapeBookmark.Brush == null)  //this bookmark is used to the first time
            {
                if (!m_BrushesCache.ContainsKey(shapeBookmark.Color))  //this color is used for the first time
                    m_BrushesCache.Add(shapeBookmark.Color, new SolidBrush(shapeBookmark.Color));

                shapeBookmark.Brush = m_BrushesCache[shapeBookmark.Color];
            }
        }

        private void RefreshPenFromCache(BasicShapeScrollBarBookmark shapeBookmark)
        {
            if (shapeBookmark.Pen == null)  //this bookmark is used to the first time
            {
                if (!m_PensCache.ContainsKey(shapeBookmark.Color))  //this color is used for the first time
                    m_PensCache.Add(shapeBookmark.Color, new Pen(shapeBookmark.Color));

                shapeBookmark.Pen = m_PensCache[shapeBookmark.Color];
            }
        }

        private void DrawThumb(Graphics graphics, int thumbLength, int thumbPosition)
        {
            ScrollBarState state = GetScrollBarAreaState(EnhancedScrollBarMouseLocation.Thumb);
            Rectangle rec;
            if (Orientation == Orientation.Vertical)
            {
                rec = new Rectangle(0, thumbPosition, ClientSize.Width, thumbLength);
                ScrollBarRenderer.DrawVerticalThumb(graphics, rec, state);
                //draw thumb grip
                if (thumbLength >= SystemInformation.VerticalScrollBarThumbHeight)
                    ScrollBarRenderer.DrawVerticalThumbGrip(graphics, rec, ScrollBarState.Normal);
            }
            else
            {
                rec = new Rectangle(thumbPosition, 0, thumbLength, ClientSize.Height);
                ScrollBarRenderer.DrawHorizontalThumb(graphics, rec, state);
                //draw thumb grip
                if (thumbLength >= SystemInformation.HorizontalScrollBarThumbWidth)
                    ScrollBarRenderer.DrawHorizontalThumbGrip(graphics, rec, ScrollBarState.Normal);
            }
        }

        private ScrollBarState GetScrollBarAreaState(EnhancedScrollBarMouseLocation mouseHotLocation)
        {
            if (this.Enabled)
            {
                if (MouseScrollBarArea == mouseHotLocation)
                    return MouseUpDownStatus == MouseButtonState.Pressed ? ScrollBarState.Pressed : ScrollBarState.Hot;
                else
                    return ScrollBarState.Normal;
            }
            else
                return ScrollBarState.Disabled;

        }

        #endregion

        #region private helper methods

        private void NavigateTo(decimal NewValue)
        {
            OnScroll(Value, Value, ScrollEventType.ThumbTrack);
            OnScroll(Value, NewValue, ScrollEventType.ThumbTrack);
            Value = NewValue;
            OnScroll(Value, Value, ScrollEventType.ThumbPosition);
            OnScroll(Value, Value, ScrollEventType.EndScroll);
        }

         private int TrackLength
         {
             get
             {
                 if (Orientation == Orientation.Vertical)
                     return this.ClientSize.Height - 2 * SystemInformation.VerticalScrollBarArrowHeight;
                 else
                     return this.ClientSize.Width - 2 * SystemInformation.HorizontalScrollBarArrowWidth;
             }
         }

         private int ThumbLength
         {
             get
             {
                 if (Minimum == Maximum) return TrackLength;

                 int nThumbLength =(int)( TrackLength/( Maximum - Minimum+1));

                 if (Orientation == Orientation.Vertical)
                 {
                     if (nThumbLength < SystemInformation.VerticalScrollBarThumbHeight)
                         nThumbLength = SystemInformation.VerticalScrollBarThumbHeight;
                 }
                 else
                 {
                     if (nThumbLength < SystemInformation.HorizontalScrollBarThumbWidth)
                         nThumbLength = SystemInformation.HorizontalScrollBarThumbWidth;
                 }
                 return nThumbLength;
             }
         }

         private int Value2ThumbTopPosition(decimal value)
         {
             if (Orientation == Orientation.Vertical)
             {
                 if (Maximum == Minimum) return SystemInformation.VerticalScrollBarArrowHeight;

                 decimal ratio = (decimal)(ClientSize.Height - 2 * SystemInformation.VerticalScrollBarArrowHeight - ThumbLength) / (Maximum - Minimum);
                 return (int)((value-Minimum )* ratio) + SystemInformation.VerticalScrollBarArrowHeight;
             }
             else
             {
                 if (Maximum == Minimum) return SystemInformation.HorizontalScrollBarArrowWidth;

                 decimal ratio = (decimal)(ClientSize.Width - 2 * SystemInformation.HorizontalScrollBarArrowWidth - ThumbLength) / (Maximum - Minimum);
                 return (int)((value -Minimum) * ratio) + SystemInformation.HorizontalScrollBarArrowWidth;
             }
         }

         private decimal ThumbTopPosition2Value(int y)
         {
             if (Orientation == Orientation.Vertical)
             {
                 if (ClientSize.Height - 2 * SystemInformation.VerticalScrollBarArrowHeight - ThumbLength == 0)
                     return 0;
                 else
                 {
                     decimal ratio = (decimal)((y - SystemInformation.VerticalScrollBarArrowHeight)) / (ClientSize.Height - 2 * SystemInformation.VerticalScrollBarArrowHeight - ThumbLength);
                     return (Maximum - Minimum) * ratio + Minimum;
                 }
             }
             else
             {
                 if (ClientSize.Width - 2 * SystemInformation.HorizontalScrollBarArrowWidth - ThumbLength == 0)
                     return 0;
                 else
                 {
                     decimal ratio = (decimal)((y - SystemInformation.HorizontalScrollBarArrowWidth)) / (ClientSize.Width - 2 * SystemInformation.HorizontalScrollBarArrowWidth - ThumbLength);
                     return (Maximum - Minimum) * ratio + Minimum;
                 }
             }
         }

         private EnhancedScrollBarMouseLocation MouseLocation(int absolutePosition, out int relativeY)
         {
             if (Orientation == Orientation.Vertical)
             {
                 if (absolutePosition <= SystemInformation.VerticalScrollBarArrowHeight)
                 {
                     relativeY = absolutePosition;
                     return EnhancedScrollBarMouseLocation.TopOrLeftArrow;
                 }
                 else if (absolutePosition > ClientSize.Height - SystemInformation.VerticalScrollBarArrowHeight)
                 {
                     relativeY = absolutePosition - ClientSize.Height + SystemInformation.VerticalScrollBarArrowHeight;
                     return EnhancedScrollBarMouseLocation.BottomOrRightArrow;
                 }
                 else
                 {
                     int thumbTop = Value2ThumbTopPosition(Value);
                     if (absolutePosition < thumbTop)
                     {
                         relativeY = absolutePosition - SystemInformation.VerticalScrollBarArrowHeight;
                         return EnhancedScrollBarMouseLocation.TopOrLeftTrack;
                     }
                     else if (absolutePosition < thumbTop + ThumbLength)
                     {
                         relativeY = absolutePosition - thumbTop;
                         return EnhancedScrollBarMouseLocation.Thumb;
                     }
                     else
                     {
                         relativeY = absolutePosition - thumbTop - ThumbLength;
                         return EnhancedScrollBarMouseLocation.BottomOrRightTrack;
                     }
                 }
             }
             else
             {
                 if (absolutePosition <= SystemInformation.HorizontalScrollBarArrowWidth)
                 {
                     relativeY = absolutePosition;
                     return EnhancedScrollBarMouseLocation.TopOrLeftArrow;
                 }
                 else if (absolutePosition > ClientSize.Width - SystemInformation.HorizontalScrollBarArrowWidth)
                 {
                     relativeY = absolutePosition - ClientSize.Width + SystemInformation.HorizontalScrollBarArrowWidth;
                     return EnhancedScrollBarMouseLocation.BottomOrRightArrow;
                 }
                 else
                 {
                     int thumbTop = Value2ThumbTopPosition(Value);
                     if (absolutePosition < thumbTop)
                     {
                         relativeY = absolutePosition - SystemInformation.HorizontalScrollBarArrowWidth;
                         return EnhancedScrollBarMouseLocation.TopOrLeftTrack;
                     }
                     else if (absolutePosition < thumbTop + ThumbLength)
                     {
                         relativeY = absolutePosition - thumbTop;
                         return EnhancedScrollBarMouseLocation.Thumb;
                     }
                     else
                     {
                         relativeY = absolutePosition - thumbTop - ThumbLength;
                         return EnhancedScrollBarMouseLocation.BottomOrRightTrack;
                     }
                 }
             }
         }

         private List<ScrollBarBookmark> BookmarksUnderPosition(int x, int y)
         {
             List<ScrollBarBookmark> bookmarksUnderPosition = new List<ScrollBarBookmark>();
             if (Bookmarks != null)
             {
                 foreach (ScrollBarBookmark bookmark in Bookmarks)
                 {
                     if (Orientation == Orientation.Vertical)
                     {
                         if ((bookmark.Y <= y) && (bookmark.Y + bookmark.Height >= y) && (bookmark.X <= x) && (bookmark.X + bookmark.Width >= x))
                             bookmarksUnderPosition.Add(bookmark);
                     }
                     else
                     {
                         if (bookmark is BasicShapeScrollBarBookmark)
                         {
                             if ((bookmark.Y <= y) && (bookmark.Y + bookmark.Height >= y) && (bookmark.X <= x) && (bookmark.X + bookmark.Width >= x))
                                 bookmarksUnderPosition.Add(bookmark);

                         }
                         else
                         {
                             if ((bookmark.X <= x) && (bookmark.X + bookmark.Width >= x) && (bookmark.Y <= y) && (bookmark.Y + bookmark.Height >= y))
                                 bookmarksUnderPosition.Add(bookmark);
                         }
                     }
                 }
             }
             return bookmarksUnderPosition;
         }

         #endregion

        #region Context menu event handlers

        private void topToolStripMenuItem_Click(object sender, EventArgs e)
        {
             OnScroll(Minimum, Value, ScrollEventType.First);
             Value = Minimum;
             OnScroll(Value, Value, ScrollEventType.EndScroll);
        }

        private void bottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
             OnScroll(Maximum, Value, ScrollEventType.Last);
             Value = Maximum;
             OnScroll(Value, Value, ScrollEventType.EndScroll);
        }

        private void scrollHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NavigateTo(HotValue);
        }

        private void pageUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnScroll(Value - LargeChange, Value, ScrollEventType.LargeDecrement);
            Value -= LargeChange;
            OnScroll(Value, Value, ScrollEventType.EndScroll);
        }

        private void pageDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnScroll(Value + LargeChange, Value, ScrollEventType.LargeIncrement);
            Value += LargeChange;
            OnScroll(Value, Value, ScrollEventType.EndScroll);
        }

        private void scrollUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnScroll(Value - SmallChange, Value, ScrollEventType.SmallDecrement);
            Value -= SmallChange;
            OnScroll(Value , Value, ScrollEventType.EndScroll);
        }

        private void scrollDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnScroll(Value + SmallChange, Value, ScrollEventType.SmallIncrement);
            Value += SmallChange;
            OnScroll(Value, Value, ScrollEventType.EndScroll);
        }

        #endregion

        #region Non-Aero Themes Support - Chuck
        protected override void WndProc(ref Message m)
        {
            const int WM_DWMCOMPOSITIONCHANGED = 0x031E;
            const int WM_THEMECHANGED = 0x031A;

            //If user changes the theme, make sure we check IsSupported flag to adjust support as necessary.
            if (m.Msg == WM_THEMECHANGED || m.Msg == WM_DWMCOMPOSITIONCHANGED)
                ScrollBarRenderer.Refresh();

            base.WndProc(ref m);
        }

        /// <summary>
        /// Need to fake out renderer because it does not support non-Aero 'Basic and High Contrast Themes' (aka Windows Classic, etc)
        /// </summary>
        private class ScrollBarRenderer
        {
            #region Public method variables
            public static Action<Graphics,Rectangle,ScrollBarArrowButtonState> DrawArrowButton;
            public static Action<Graphics, Rectangle, ScrollBarState> DrawHorizontalThumb;
            public static Action<Graphics, Rectangle, ScrollBarState> DrawHorizontalThumbGrip;
            public static Action<Graphics, Rectangle, ScrollBarState> DrawLeftHorizontalTrack;
            public static Action<Graphics, Rectangle, ScrollBarState> DrawLowerVerticalTrack;
            public static Action<Graphics, Rectangle, ScrollBarState> DrawRightHorizontalTrack;
            public static Action<Graphics, Rectangle, ScrollBarSizeBoxState> DrawSizeBox;
            public static Action<Graphics, Rectangle, ScrollBarState> DrawUpperVerticalTrack;
            public static Action<Graphics, Rectangle, ScrollBarState> DrawVerticalThumb;
            public static Action<Graphics, Rectangle, ScrollBarState> DrawVerticalThumbGrip;
            public static Func<Graphics, ScrollBarState, Size> GetSizeBoxSize;
            public static Func<Graphics, ScrollBarState, Size> GetThumbGripSize;
            #endregion

            static ScrollBarRenderer() { Refresh(); }

            private static Brush _horizontalTrackBrush;
            private static Brush _verticalTrackBrush;
            private static Brush GetTrackBrush(Rectangle bounds, Orientation orient)
            {
                if (orient == Orientation.Vertical)
                {
                    if (_verticalTrackBrush == null)
                    {
                        var dark = ControlPaint.LightLight(SystemColors.ButtonFace);
                        var light = ControlPaint.Light(dark);
                        _verticalTrackBrush = new System.Drawing.Drawing2D.LinearGradientBrush(bounds, dark, light, 0f);
                    }
                    return _verticalTrackBrush;
                }
                else
                {
                    if (_horizontalTrackBrush == null)
                    {
                        var dark = ControlPaint.LightLight(SystemColors.ButtonFace);
                        var light = ControlPaint.Light(dark);
                        _horizontalTrackBrush = new System.Drawing.Drawing2D.LinearGradientBrush(bounds, dark, light, 90f);
                    }
                    return _horizontalTrackBrush;
                }
            }

            public static void Refresh()
            {
                //Recompute DrawXXXX() sources.
                bool isSupported = System.Windows.Forms.ScrollBarRenderer.IsSupported;
                DrawArrowButton = isSupported ? (Action<Graphics, Rectangle, ScrollBarArrowButtonState>)System.Windows.Forms.ScrollBarRenderer.DrawArrowButton : drawArrowButton;
                DrawHorizontalThumb = isSupported ? (Action<Graphics, Rectangle, ScrollBarState>)System.Windows.Forms.ScrollBarRenderer.DrawHorizontalThumb : drawHorizontalThumb;
                DrawHorizontalThumbGrip = isSupported ? (Action<Graphics, Rectangle, ScrollBarState>)System.Windows.Forms.ScrollBarRenderer.DrawHorizontalThumbGrip : drawHorizontalThumbGrip;
                DrawLeftHorizontalTrack = isSupported ? (Action<Graphics, Rectangle, ScrollBarState>)System.Windows.Forms.ScrollBarRenderer.DrawLeftHorizontalTrack : drawLeftHorizontalTrack;
                DrawLowerVerticalTrack = isSupported ? (Action<Graphics, Rectangle, ScrollBarState>)System.Windows.Forms.ScrollBarRenderer.DrawLowerVerticalTrack : drawLowerVerticalTrack;
                DrawRightHorizontalTrack = isSupported ? (Action<Graphics, Rectangle, ScrollBarState>)System.Windows.Forms.ScrollBarRenderer.DrawRightHorizontalTrack : drawRightHorizontalTrack;
                DrawSizeBox = isSupported ? (Action<Graphics, Rectangle, ScrollBarSizeBoxState>)System.Windows.Forms.ScrollBarRenderer.DrawSizeBox : drawSizeBox;
                DrawUpperVerticalTrack = isSupported ? (Action<Graphics, Rectangle, ScrollBarState>)System.Windows.Forms.ScrollBarRenderer.DrawUpperVerticalTrack : drawUpperVerticalTrack;
                DrawVerticalThumb = isSupported ? (Action<Graphics, Rectangle, ScrollBarState>)System.Windows.Forms.ScrollBarRenderer.DrawVerticalThumb : drawVerticalThumb;
                DrawVerticalThumbGrip = isSupported ? (Action<Graphics, Rectangle, ScrollBarState>)System.Windows.Forms.ScrollBarRenderer.DrawVerticalThumbGrip : drawVerticalThumbGrip;
                GetSizeBoxSize = isSupported ? (Func<Graphics, ScrollBarState, Size>)System.Windows.Forms.ScrollBarRenderer.GetSizeBoxSize : getSizeBoxSize;
                GetThumbGripSize = isSupported ? (Func<Graphics, ScrollBarState, Size>)System.Windows.Forms.ScrollBarRenderer.GetThumbGripSize : getThumbGripSize;

                if (isSupported && _horizontalTrackBrush != null)
                {
                    _horizontalTrackBrush.Dispose();
                    _horizontalTrackBrush = null;
                    _verticalTrackBrush.Dispose();
                    _verticalTrackBrush = null;
                }
            }

            public static bool IsSupported { get { return true; } }

            private static void drawArrowButton(Graphics g, Rectangle bounds, ScrollBarArrowButtonState state)
            {
                ScrollButton button;
                ButtonState state2;

                switch (state)
                {
                    case ScrollBarArrowButtonState.UpNormal:      button = ScrollButton.Up;    state2 = ButtonState.Normal; break;
                    case ScrollBarArrowButtonState.UpHot:         button = ScrollButton.Up;    state2 = ButtonState.Normal; break;
                    case ScrollBarArrowButtonState.UpPressed:     button = ScrollButton.Up;    state2 = ButtonState.Pushed; break;
                    case ScrollBarArrowButtonState.UpDisabled:    button = ScrollButton.Up;    state2 = ButtonState.Inactive; break;
                    case ScrollBarArrowButtonState.DownNormal:    button = ScrollButton.Down;  state2 = ButtonState.Normal; break;
                    case ScrollBarArrowButtonState.DownHot:       button = ScrollButton.Down;  state2 = ButtonState.Normal; break;
                    case ScrollBarArrowButtonState.DownPressed:   button = ScrollButton.Down;  state2 = ButtonState.Pushed; break;
                    case ScrollBarArrowButtonState.DownDisabled:  button = ScrollButton.Down;  state2 = ButtonState.Inactive; break;
                    case ScrollBarArrowButtonState.LeftNormal:    button = ScrollButton.Left;  state2 = ButtonState.Normal; break;
                    case ScrollBarArrowButtonState.LeftHot:       button = ScrollButton.Left;  state2 = ButtonState.Normal; break;
                    case ScrollBarArrowButtonState.LeftPressed:   button = ScrollButton.Left;  state2 = ButtonState.Pushed; break;
                    case ScrollBarArrowButtonState.LeftDisabled:  button = ScrollButton.Left;  state2 = ButtonState.Inactive; break;
                    case ScrollBarArrowButtonState.RightNormal:   button = ScrollButton.Right; state2 = ButtonState.Normal; break;
                    case ScrollBarArrowButtonState.RightHot:      button = ScrollButton.Right; state2 = ButtonState.Normal; break;
                    case ScrollBarArrowButtonState.RightPressed:  button = ScrollButton.Right; state2 = ButtonState.Pushed; break;
                    case ScrollBarArrowButtonState.RightDisabled: button = ScrollButton.Right; state2 = ButtonState.Inactive; break;
                    default:                                      button = ScrollButton.Up;    state2 = ButtonState.Normal; break;
                }

                ControlPaint.DrawScrollButton(g, bounds, button, state2);
            }

            private static void drawHorizontalThumb(Graphics g, Rectangle bounds, ScrollBarState state)
            {
                ButtonState state2;
                switch(state)
                {
                    case ScrollBarState.Normal:   state2 = ButtonState.Normal; break;
                    case ScrollBarState.Hot:      state2 = ButtonState.Normal; break;
                    case ScrollBarState.Pressed:  state2 = ButtonState.Pushed; break;
                    case ScrollBarState.Disabled: state2 = ButtonState.Inactive; break;
                    default:                      state2 = ButtonState.Normal; break;
                }

                ControlPaint.DrawButton(g, bounds, state2);
            }

            private static void drawHorizontalThumbGrip(Graphics g, Rectangle bounds, ScrollBarState state)
            {
            }

            private static void drawLeftHorizontalTrack(Graphics g, Rectangle bounds, ScrollBarState state)
            {
                if (bounds.Width < 1 || bounds.Height < 1) return;
                g.FillRectangle(GetTrackBrush(bounds,Orientation.Horizontal), bounds);
            }

            private static void drawRightHorizontalTrack(Graphics g, Rectangle bounds, ScrollBarState state)
            {
                if (bounds.Width < 1 || bounds.Height < 1) return;
                g.FillRectangle(GetTrackBrush(bounds, Orientation.Horizontal), bounds);
            }

            private static void drawLowerVerticalTrack(Graphics g, Rectangle bounds, ScrollBarState state)
            {
                if (bounds.Width < 1 || bounds.Height < 1) return;
                g.FillRectangle(GetTrackBrush(bounds, Orientation.Vertical), bounds);
            }

            private static void drawUpperVerticalTrack(Graphics g, Rectangle bounds, ScrollBarState state)
            {
                if (bounds.Width < 1 || bounds.Height < 1) return;
                g.FillRectangle(GetTrackBrush(bounds, Orientation.Vertical), bounds);
            }

            private static void drawVerticalThumb(Graphics g, Rectangle bounds, ScrollBarState state)
            {
                ButtonState state2;
                switch(state)
                {
                    case ScrollBarState.Normal:   state2 = ButtonState.Normal; break;
                    case ScrollBarState.Hot:      state2 = ButtonState.Normal; break;
                    case ScrollBarState.Pressed:  state2 = ButtonState.Pushed; break;
                    case ScrollBarState.Disabled: state2 = ButtonState.Inactive; break;
                    default:                      state2 = ButtonState.Normal; break;
                }

                ControlPaint.DrawButton(g,bounds, state2);
            }

            private static void drawVerticalThumbGrip(Graphics g, Rectangle bounds, ScrollBarState state)
            {
            }

            private static void drawSizeBox(Graphics g, Rectangle bounds, ScrollBarSizeBoxState state)
            {
            }

            private static Size getSizeBoxSize(Graphics g, ScrollBarState state)
            {
                throw new NotImplementedException("GetSizeBoxSize not implemented for non-themed scroll bars.");
                //return new Size();
            }

            public static Size getThumbGripSize(Graphics g, ScrollBarState state)
            {
                throw new NotImplementedException("GetThumbGripSize not implemented for non-themed scroll bars.");
                //return new Size();
            }
        }
        #endregion
    }

    #region Enhanced event argument definitions

    /// <summary>
    /// Argument for <c>ToolTipNeeded</c> event.
    /// </summary>
    public class TooltipNeededEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Value">ToolTip value. Hot ToolTip value mouse is moving over.</param>
        /// <param name="ToolTip">Default ToolTip message.</param>
        /// <param name="Bookmarks">List of over-wrapping bookmarks under <c>Value</c> position.</param>
        public TooltipNeededEventArgs(decimal Value, string ToolTip, List<ScrollBarBookmark> Bookmarks)
        {
            this.Value=Value;
            this.ToolTip=ToolTip;
            this.Bookmarks = Bookmarks;
        }

        /// <summary>
        /// ToolTip value. Hot ToolTip value mouse is moving over.
        /// </summary>
        public decimal Value { private set; get; }

        /// <summary>
        /// Default ToolTip message.
        /// </summary>
        public string ToolTip {set; get;}

        /// <summary>
        /// List of over-wrapping bookmarks under <c>Value</c> position.
        /// </summary>
        public List<ScrollBarBookmark> Bookmarks {private set; get; }
    }

    /// <summary>
    /// Arguments for EnhancedScrollEvent.
    /// </summary>
    public class EnhancedScrollEventArgs: EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public EnhancedScrollEventArgs()
        {
            this.NewValue=0;
            this.OldValue=0;
            this.Type= ScrollEventType.EndScroll;
            this.ScrollOrientation= ScrollOrientation.VerticalScroll;

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="OldValue">Previous EnhancedScrollBar value.</param>
        /// <param name="NewValue">New EnhancedScrollBar value.</param>
        /// <param name="Type">Type of the scroll event.</param>
        /// <param name="ScrollOrientation">EnhancedScrollBar orientation.</param>
        public EnhancedScrollEventArgs(decimal OldValue, decimal NewValue, ScrollEventType Type, ScrollOrientation ScrollOrientation)
        {
            this.NewValue= NewValue;
            this.OldValue= OldValue;
            this.ScrollOrientation= ScrollOrientation;
            this.Type=Type;
        }

        /// <summary>
        /// Previous EnhancedScrollBar value.
        /// </summary>
        public decimal OldValue {private set; get;}

        /// <summary>
        /// New EnhancedScrollBar value.
        /// </summary>
        public decimal NewValue {private set; get;}

        /// <summary>
        /// EnhancedScrollBar orientation.
        /// </summary>
        public ScrollOrientation ScrollOrientation {private set; get;}

        /// <summary>
        /// Type of the scroll event.
        /// </summary>
        public ScrollEventType Type {private set; get;}
    }

    /// <summary>
    /// Arguments for mouse related events.
    /// </summary>
    public class EnhancedMouseEventArgs:MouseEventArgs
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Value">ScrollBar <c>Value when event occurred.</c></param>
        /// <param name="MouseArgs">Original <c>MouseArgs</c>.</param>
        /// <param name="Bookmarks">List of bookmarks under mouse position.</param>
        /// <param name="ScrollBarSection">Section of the EnhancedScrollBar where mouse pointer is located.</param>
        public EnhancedMouseEventArgs(decimal Value, MouseEventArgs MouseArgs, List<ScrollBarBookmark> Bookmarks, EnhancedScrollBarMouseLocation ScrollBarSection ):base(MouseArgs.Button, MouseArgs.Clicks, MouseArgs.X, MouseArgs.Y, MouseArgs.Delta)
        {

            this.Value = Value;
            this.Bookmarks = Bookmarks;
            this.ScrollBarSection = ScrollBarSection;

        }

        /// <summary>
        /// ScrollBar <c>Value</c> when event occurred.
        /// </summary>
        public decimal Value {private set; get;}

        /// <summary>
        /// List of bookmarks under mouse position.
        /// </summary>
        public List<ScrollBarBookmark> Bookmarks {private set; get;}

        /// <summary>
        /// Section of the EnhancedScrollBar where mouse pointer is located.
        /// </summary>
        public EnhancedScrollBarMouseLocation ScrollBarSection { private set; get; }
    }

    #endregion

    #region Public Enumerators

    /// <summary>
    /// Area of ScrollBar definitions. Used to describe relation of mouse pointer location
    /// to the distinct part of ScrollBar.
    /// </summary>
    public enum EnhancedScrollBarMouseLocation
    {
        /// <summary>
        /// Located outside of the ScrollBar.
        /// </summary>
        OutsideScrollBar,

        /// <summary>
        /// Located over top (for vertical ScrollBar) or
        /// over left hand side arrow (for horizontal ScrollBar).
        /// </summary>
        TopOrLeftArrow,

        /// <summary>
        /// Located over top (for vertical Scrollbar) or
        /// over left hand side track (for horizontal ScrollBar).
        /// Track is the area between arrow and thumb images.
        /// </summary>
        TopOrLeftTrack,

        /// <summary>
        /// Located over ScrollBar thumb. Thumb is movable portion of the ScrollBar.
        /// </summary>
        Thumb,

        /// <summary>
        /// Located over bottom (for vertical Scrollbar) or
        /// over right hand side track (for horizontal ScrollBar).
        /// Track is the area between arrow and thumb images.
        /// </summary>
        BottomOrRightTrack,

        /// <summary>
        /// Located over bottom (for vertical ScrollBar) or
        /// over right hand side arrow (for horizontal ScrollBar).
        /// </summary>
        BottomOrRightArrow
    }

    #endregion

    #region Collection editor for Bookmark collection, to allow editing in Design Mode.

    /// <summary>
    /// Collection editor for bookmark. Without it, adding bookmark in design mode is impossible
    /// (exception is thrown). With it collection editor add button has drop-down list to pick from.
    /// List is defined in CreateNewItemTypes override.
    /// </summary>
    public class BookmarksCollectionEditor : CollectionEditor
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        public BookmarksCollectionEditor(Type type) : base(type) { }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] {typeof(BasicShapeScrollBarBookmark), typeof(ImageScrollBarBookmark), typeof(ValueRangeScrollBarBookmark)};
        }
    }

    #endregion

    #region SandCastle help utility requires existence of this empty class
    /* The following empty class exists only in order to provide SandCastle help utility
     * extract namespace summary.
     */

    /// <summary>
    /// Collection of WinForms components and controls.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {

    }
    #endregion
}