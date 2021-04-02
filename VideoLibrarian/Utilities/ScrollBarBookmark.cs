/*
 * User: Adam A Zgagacz
 * http://www.proxoft.com
 * https://www.codeproject.com/Articles/624997/Enhanced-Scrollbar
 * Date: 7/26/2013
 */

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace ProXoft.WinForms
{

    /// <summary>
    /// Abstract class, ancestor of all bookmark types.
    /// </summary>
    ///
    public abstract class ScrollBarBookmark
    {
        /// <summary>
        /// Name of the bookmark.
        /// </summary>
        [DefaultValue("")]
        [Description("Bookmark Name")]
        public string Name { set; get; }

        /// <summary>
        /// Bookmark value. Defines position on the scrollbar where bookmark graphical indicator is displayed.
        /// </summary>
        [DefaultValue(0)]
        [Description("Bookmark value. Defines position on the scrollbar where bookmark graphical indicator is displayed.")]
        public decimal Value { set; get; }

        /// <summary>
        /// Bookmark alignment along shortest scrollbar dimension.
        /// </summary>
        [DefaultValue(ScrollBarBookmarkAlignment.LeftOrTop)]
        [Description("Bookmark alignment along shortest scrollbar dimension.")]
        public ScrollBarBookmarkAlignment Alignment { set; get; }

        /// <summary>
        /// Any object associated with the bookmark instance. Bookmark object from host can contain any sort of info.
        /// It can be handy when bookmark is clicked; moved over etc.
        /// </summary>
        [DefaultValue(null)]
        [Description("Tag object associated with the bookmark")]
        public object Tag { set; get; }

        /// <summary>
        /// Height has to be abstract since it has different meaning in descending classes.
        /// </summary>
        [DefaultValue(5)]
        public virtual int Height {set; get;}

        /// <summary>
        /// Width has to be abstract since it has different meaning in descending classes.
        /// </summary>
        [DefaultValue(5)]
        public virtual int Width {set; get;}

        //Every time bookmark is repainted the following two values are recalculated.
        /// <summary>
        /// X coordinate of bookmark center.
        /// </summary>
        protected internal int X { set; get; }
        /// <summary>
        /// Y coordinate of bookmark center.
        /// </summary>
        protected internal int Y { set; get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Bookmark name.</param>
        /// <param name="value">Bookmark value (position along long scrollbar dimension).</param>
        /// <param name="alignment">Bookmark alignment along short scrollbar dimension.</param>
        /// <param name="tag">Tag associated with the bookmark.</param>
        public ScrollBarBookmark(string name, decimal value, ScrollBarBookmarkAlignment alignment, object tag)
        {
            Name = name;
            Value = value;
            Alignment = alignment;
            Tag = tag;
        }
    }

    /// <summary>
    /// Concrete bookmark - shown as an image
    /// on <c>Value</c> position.</summary>
    [RefreshProperties(RefreshProperties.All)]
    [Serializable]
    public class ImageScrollBarBookmark : ScrollBarBookmark
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ImageScrollBarBookmark():base("", 0, ScrollBarBookmarkAlignment.LeftOrTop, null)
        {
            Image = null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Bookmark name.</param>
        /// <param name="value">Bookmark value (position along long scrollbar dimension).</param>
        /// <param name="image">Image to be shown as a bookmark marker.</param>
        /// <param name="alignment">Bookmark alignment along short scrollbar dimension.</param>
        /// <param name="tag">Tag associated with the bookmark.</param>
        public ImageScrollBarBookmark(string name, decimal value, Image image, ScrollBarBookmarkAlignment alignment, object tag)
            :base(name, value, alignment, tag)
        {
            Image = image;
        }

        /// <summary>
        /// Image to be used for displaying bookmark.
        /// </summary>
        public Image Image {  set; get; }

        /// <summary>
        /// Image width. This is read only property for ImageScrollBookmark.
        /// </summary>
        public override  int Width
        {
            get { return Image==null?0: Image.Width; }
            set {throw new Exception("'Width' setter is not implemented for ImageScrollBarBookmark. if you want to change size of 'Image' in ImageScrollBarBokmark resize image before passing it into Bookmark.");}
        }

        /// <summary>
        /// Image hight. This is read only property for ImageScrollBookmark.
        /// </summary>
        public override int Height
        {
            get { return Image ==null?0: Image.Height; }
            set { throw new Exception("'Height' setter is not implemented for ImageScrollBarBookmark. if you want to change size of 'Image' in ImageScrollBarBokmark resize image before passing it into Bookmark."); }
        }

    }


    /// <summary>
    /// Concrete bookmark - shown as colored rectangle or oval.
    /// </summary>
    [RefreshProperties(RefreshProperties.All)]
    [Serializable]
    [Description("Bookmark shown as colored rectangle or oval.")]
    public class BasicShapeScrollBarBookmark : ScrollBarBookmark
    {
        private Color m_Color = Color.Empty;

        /// <summary>
        /// Constructor.
        /// </summary>
        public BasicShapeScrollBarBookmark():base("", 0, ScrollBarBookmarkAlignment.LeftOrTop, null)
        {
            Height = 5;
            Width = 5;
            Shape = ScrollbarBookmarkShape.Rectangle;
            Color = Color.Orange;
            FillBookmarkShape = true;
            Stretch = false;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Bookmark name.</param>
        /// <param name="value">Bookmark value.</param>
        /// <param name="alignment">Bookmark alignment along short scrollbar dimension.</param>
        /// <param name="height">Bookmark dimension along long dimension of the scrollbar.</param>
        /// <param name="width">Bookmark dimension along short dimension of the scrollbar. </param>
        /// <param name="shape">Bookmark shape (oval or rectangle).</param>
        /// <param name="color">Bookmark color.</param>
        /// <param name="fillBookmarkShape"></param>
        /// <param name="stretchToScrollBarWidth">If set to <b>true</b>, value of <c>width</c> property
        /// is ignored and bookmark is drown to fill entire short scrollbar dimension.</param>
        /// <param name="tag">Tag associated with the bookmark.</param>
        public BasicShapeScrollBarBookmark(string name, decimal value, ScrollBarBookmarkAlignment alignment, int height, int width, ScrollbarBookmarkShape shape, Color color, bool fillBookmarkShape, bool stretchToScrollBarWidth, object tag)
            :base(name, value, alignment, tag)
        {
            this.Height = height;
            this.Width = width;
            this.Shape = shape;
            this.Color = color;
            this.FillBookmarkShape = fillBookmarkShape;
            this.Stretch = stretchToScrollBarWidth;
        }


        /// <summary>
        /// Stretch bookmark across shortest dimension of the bookmark.
        /// </summary>
        [DefaultValue(false)]
        public bool Stretch { set; get; }

        /// <summary>
        /// Bookmark shape.
        /// </summary>
        [DefaultValue(ScrollbarBookmarkShape.Rectangle)]
        public ScrollbarBookmarkShape Shape { set; get; }

        /// <summary>
        /// If <b>true</b> bookmark is drawn as shape filled with color, otherwise
        /// only border is drawn.
        /// </summary>
        [DefaultValue(true)]
        public bool FillBookmarkShape { set; get; }

        /// <summary>
        /// Bookmark color.
        /// </summary>
        [DefaultValue(typeof(Color), "Orange")]
        public Color Color
        {
            set
            {
                if (m_Color != value)
                {
                    m_Color = value;
                    Pen = null;
                    Brush = null;
                }
            }
            get { return m_Color; }
        }


        [NonSerialized]
        private Pen m_Pen;
        /// <summary>
        /// Bookmark Pen.
        /// </summary>
        protected internal Pen Pen
        {
            set { m_Pen = value; }
            get { return m_Pen; }
        }

        [NonSerialized]
        private Brush m_Brush;
        /// <summary>
        /// Bookmark Brush.
        /// </summary>
        protected internal Brush Brush
        {
            set {m_Brush=value;}
            get { return m_Brush; }
        }


    }

    /// <summary>
    /// Range bookmark with "length" defined by the range of values.
    /// </summary>
    [RefreshProperties(RefreshProperties.All)]
    [Serializable]
    [Description("Range bookmark with \"length\" defined by the range of values.")]
    public class ValueRangeScrollBarBookmark : BasicShapeScrollBarBookmark
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ValueRangeScrollBarBookmark():
            base( "", 0, ScrollBarBookmarkAlignment.LeftOrTop, 5,5,ScrollbarBookmarkShape.Rectangle, Color.Coral, true, false, null)

        {
            EndValue = 0;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Bookmark name.</param>
        /// <param name="startValue">Range start value.</param>
        /// <param name="endValue">Range end value.</param>
        /// <param name="alignment">Bookmark alignment (left, right, center) </param>
        /// <param name="depth"></param>
        /// <param name="color"></param>
        /// <param name="fillBookmarkShape"></param>
        /// <param name="stretchToScrollBarWidth"></param>
        /// <param name="tag"></param>
        public ValueRangeScrollBarBookmark(string name, decimal startValue, decimal endValue, ScrollBarBookmarkAlignment alignment, int depth,  Color color, bool fillBookmarkShape, bool stretchToScrollBarWidth, object tag):
            base(name, startValue, alignment, 0, depth, ScrollbarBookmarkShape.Rectangle, color, fillBookmarkShape, stretchToScrollBarWidth, tag)
        {
            this.EndValue = endValue;
        }

        /// <summary>
        /// End value for the bookmark range (ValueRangeScrollBarBookmark).
        /// </summary>
        public decimal EndValue { set; get; }

        /// <summary>
        /// Bookmark width (for vertically oriented scrollbar) or hight (for horizontally oriented scrollbar).
        /// </summary>
        [DefaultValue(5)]
        [Description("ValueRangeScrollBarBookmark Height (for vertical) or Width (for horizontal) are calculated every time scrollbar is repainted.Value will change every time Minimum, Maximum or scrollbar size change.")]
        public override int Width {set; get;}

        /// <summary>
        ///
        /// </summary>
        [DefaultValue(5)]
        [Description("ValueRangeScrollBarBookmark Height (for vertical) or Width (for horizontal) are calculated every time scrollbar is repainted.Value will change every time Minimum, Maximum or scrollbar size chage.")]
        public override int Height {set; get;}



    }

    /// <summary>
    /// Shape of ValueRangeScrollBarBookmark.
    /// </summary>
    public enum ScrollbarBookmarkShape
    {
        /// <summary>
        /// Bookmark is shaped as rectangle with dimension: Width x (EndRange - StartRange).
        /// </summary>
        Rectangle,
        /// <summary>
        /// Bookmark is shaped as oval.
        /// </summary>
        Oval,
     }

    /// <summary>
    /// Bookmark alignment vs. shortest dimension of the scrollbar: Left/Center/Right for the vertical scrollbar;
    /// Top/Center/Bottom for horizontal scrollbar.
    /// </summary>
    public enum ScrollBarBookmarkAlignment
    {
        /// <summary>
        /// Left border for vertical scrollbar or top border for horizontal scrollbar.
        /// </summary>
        LeftOrTop,
        /// <summary>
        /// Right border for vertical scrollbar or bottom border for horizontal scrollbar.
        /// </summary>
        RightOrBottom,
        /// <summary>
        /// Centered along shortest dimension of the scrollbar.
        /// </summary>
        Center
    }

}
