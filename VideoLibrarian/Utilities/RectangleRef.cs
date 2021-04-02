using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Globalization;
using System.Xml.Serialization;

namespace VideoLibrarian
{
    /// <summary>
    /// Stores the location and size of a rectangular region. 
    /// Based upon System.Drawing.Rectangle except unlike struct Rectangle (ByValue), 
    /// class RectangleRef is ByRef.
    /// Important when updating values that need to be passed elsewhere.
    /// In addition:
    ///   (X,Y,Width,Height) are XML Serializable as Attributes.
    ///   (X,Y,Width,Height) setters support event handlers XxxxChanged.
    ///   Implicitly castable between RectangleRef and:
    ///      System.Drawing.Rectangle
    ///      System.Drawing.RectangleF (values rounded).
    /// </summary>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    [XmlInclude(typeof(Rectangle))] //necessary when using implicit operators
    public class RectangleRef : IEquatable<RectangleRef>, IEquatable<Rectangle>
    {
        public static readonly RectangleRef Empty = new RectangleRef();

        private int x;
        private int y;
        private int width;
        private int height;
        private object userData; //aka Tag -- custom user data

        #region Constructors
        public RectangleRef() { }

        public RectangleRef(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public RectangleRef(Point location, Size size)
        {
            this.x = location.X;
            this.y = location.Y;
            this.width = size.Width;
            this.height = size.Height;
        }

        public RectangleRef(PointF location, SizeF size)
        {
            this.x = (int)Math.Round(location.X);
            this.y = (int)Math.Round(location.Y);
            this.width = (int)Math.Round(size.Width);
            this.height = (int)Math.Round(size.Height);
        }

        public RectangleRef(Rectangle rc)
        {
            this.x = rc.X;
            this.y = rc.Y;
            this.width = rc.Width;
            this.height = rc.Height;
        }

        public RectangleRef(RectangleRef rc)
        {
            this.x = rc.X;
            this.y = rc.Y;
            this.width = rc.Width;
            this.height = rc.Height;
        }

        public RectangleRef(RectangleF rc)
        {
            this.x = (int)Math.Round(rc.X);
            this.y = (int)Math.Round(rc.Y);
            this.width = (int)Math.Round(rc.Width);
            this.height = (int)Math.Round(rc.Height);
        }
        #endregion

        #region public events
        /// <summary>XChanged(RectangleRefM thisObject, int previousValue, int currentValue);</summary>
        public event Action<RectangleRef, int, int> XChanged;

        /// <summary>YChanged(RectangleRefM thisObject, int previousValue, int currentValue);</summary>
        public event Action<RectangleRef, int, int> YChanged;

        /// <summary>WidthChanged(RectangleRefM thisObject, int previousValue, int currentValue);</summary>
        public event Action<RectangleRef, int, int> WidthChanged;

        /// <summary>HeightChanged(RectangleRefM thisObject, int previousValue, int currentValue);</summary>
        public event Action<RectangleRef, int, int> HeightChanged;
        #endregion

        #region public properties
        [Browsable(false)]
        [XmlIgnore]
        public Point Location { get { return new Point(X, Y); } set { X = value.X; Y = value.Y; } }

        [Browsable(false)]
        [XmlIgnore]
        public Size Size { get { return new Size(Width, Height); } set { this.Width = value.Width; this.Height = value.Height; } }

        [Browsable(false)]
        [XmlIgnore]
        public SizeF SizeF { get { return new SizeF(Width, Height); } set { this.Width = (int)Math.Round(value.Width); this.Height = (int)Math.Round(value.Height); } }

        [XmlAttribute]
        public int X { get { return x; } set { var v = x; x = value; if (XChanged != null) XChanged(this, v, x); } }

        [XmlAttribute]
        public int Y { get { return y; } set { var v = y; y = value; if (YChanged != null) YChanged(this, v, y); } }

        [XmlAttribute]
        public int Width { get { return width; } set { var v = width; width = value; if (WidthChanged != null) WidthChanged(this, v, width); } }

        [XmlAttribute]
        public int Height { get { return height; } set { var v = height; height = value; if (HeightChanged != null) HeightChanged(this, v, height); } }

        [Browsable(false)]
        public int Left { get { return X; } }

        [Browsable(false)]
        public int Top { get { return Y; } }

        [Browsable(false)]
        public int Right { get { return X + Width; } }

        [Browsable(false)]
        public int Bottom { get { return Y + Height; } }

        [Browsable(false)]
        public bool IsEmpty { get { return height == 0 && width == 0 && x == 0 && y == 0; } }

        [Browsable(false)]
        [XmlIgnore]
        public object Tag { get { return userData; } set { userData = value; } }
        #endregion

        #region public bool Equals(...)
        public override bool Equals(object obj)
        {
            if (!(obj is RectangleRef)) return false;
            RectangleRef other = (RectangleRef)obj;
            return (other.x == this.x) &&
                   (other.y == this.y) &&
                   (other.width == this.width) &&
                   (other.height == this.height);
        }

        public bool Equals(RectangleRef other)
        {
            if (RectangleRef.ReferenceEquals(other, null)) return false; //can't use '==' because it will be recursive!
            return (other.x == this.x) &&
                   (other.y == this.y) &&
                   (other.width == this.width) &&
                   (other.height == this.height);
        }

        public bool Equals(Rectangle other)
        {
            return (other.X == this.x) &&
                   (other.Y == this.y) &&
                   (other.Width == this.width) &&
                   (other.Height == this.height);
        }
        #endregion

        #region Operator Overloads
        public static bool operator ==(RectangleRef left, RectangleRef right)
        {
            if (RectangleRef.ReferenceEquals(left, null) && 
                RectangleRef.ReferenceEquals(right, null)) return true; //can't use '==' because it will be recursive!
            if (RectangleRef.ReferenceEquals(left, null) ||
                RectangleRef.ReferenceEquals(right, null)) return false;

            return (left.x == right.x) &&
                   (left.y == right.y) &&
                   (left.width == right.width) &&
                   (left.height == right.height);
        }

        public static bool operator !=(RectangleRef left, RectangleRef right)
        {
            return !(left == right);
        }

        public static implicit operator RectangleRef(Rectangle r) { return new RectangleRef(r.X, r.Y, r.Width, r.Height); }

        public static implicit operator Rectangle(RectangleRef r) { return new Rectangle(r.X, r.Y, r.Width, r.Height); }

        public static implicit operator RectangleRef(RectangleF r) { return RectangleRef.Round(r); }

        public static implicit operator RectangleF(RectangleRef r) { return new RectangleF(r.X, r.Y, r.Width, r.Height); }
        #endregion

        #region Public Methods
        public static RectangleRef FromLTRB(int left, int top, int right, int bottom)
        {
            return new RectangleRef(left, top, right - left, bottom - top);
        }

        public void SetValues(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public static RectangleRef Ceiling(RectangleF value)
        {
            return new RectangleRef((int)Math.Ceiling(value.X),
                                 (int)Math.Ceiling(value.Y),
                                 (int)Math.Ceiling(value.Width),
                                 (int)Math.Ceiling(value.Height));
        }

        public static RectangleRef Truncate(RectangleF value)
        {
            return new RectangleRef((int)value.X,
                                 (int)value.Y,
                                 (int)value.Width,
                                 (int)value.Height);
        }

        public static RectangleRef Round(RectangleF value)
        {
            return new RectangleRef((int)Math.Round(value.X),
                                 (int)Math.Round(value.Y),
                                 (int)Math.Round(value.Width),
                                 (int)Math.Round(value.Height));
        }

        public static RectangleRef Round(double x, double y, double width, double height)
        {
            return new RectangleRef((int)Math.Round(x),
                                 (int)Math.Round(y),
                                 (int)Math.Round(width),
                                 (int)Math.Round(height));
        }

        [Pure]
        public bool Contains(int x, int y)
        {
            return this.X <= x &&
                x < this.X + this.Width &&
                this.Y <= y &&
                y < this.Y + this.Height;
        }

        [Pure]
        public bool Contains(Point pt)
        {
            return Contains(pt.X, pt.Y);
        }

        [Pure]
        public bool Contains(RectangleRef rect)
        {
            return (this.X <= rect.X) &&
                ((rect.X + rect.Width) <= (this.X + this.Width)) &&
                (this.Y <= rect.Y) &&
                ((rect.Y + rect.Height) <= (this.Y + this.Height));
        }

        public override int GetHashCode()
        {
            return unchecked((int)((UInt32)X ^
                        (((UInt32)Y << 13) | ((UInt32)Y >> 19)) ^
                        (((UInt32)Width << 26) | ((UInt32)Width >> 6)) ^
                        (((UInt32)Height << 7) | ((UInt32)Height >> 25))));
        }

        public void Inflate(int width, int height)
        {
            this.X -= width;
            this.Y -= height;
            this.Width += 2 * width;
            this.Height += 2 * height;
        }

        public void Inflate(Size size)
        {
            Inflate(size.Width, size.Height);
        }

        public static RectangleRef Inflate(RectangleRef rect, int x, int y)
        {
            RectangleRef r = rect;
            r.Inflate(x, y);
            return r;
        }

        public void Intersect(RectangleRef rect)
        {
            RectangleRef result = RectangleRef.Intersect(rect, this);

            this.X = result.X;
            this.Y = result.Y;
            this.Width = result.Width;
            this.Height = result.Height;
        }

        public static RectangleRef Intersect(RectangleRef a, RectangleRef b)
        {
            int x1 = Math.Max(a.X, b.X);
            int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Max(a.Y, b.Y);
            int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

            if (x2 >= x1 && y2 >= y1)
            {
                return new RectangleRef(x1, y1, x2 - x1, y2 - y1);
            }

            return RectangleRef.Empty;
        }

        [Pure]
        public bool IntersectsWith(RectangleRef rect)
        {
            return (rect.X < this.X + this.Width) &&
            (this.X < (rect.X + rect.Width)) &&
            (rect.Y < this.Y + this.Height) &&
            (this.Y < rect.Y + rect.Height);
        }

        [Pure]
        public static RectangleRef Union(RectangleRef a, RectangleRef b)
        {
            int x1 = Math.Min(a.X, b.X);
            int x2 = Math.Max(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Min(a.Y, b.Y);
            int y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);

            return new RectangleRef(x1, y1, x2 - x1, y2 - y1);
        }

        public void Offset(Point pos)
        {
            Offset(pos.X, pos.Y);
        }

        public void Offset(int x, int y)
        {
            this.X += x;
            this.Y += y;
        }
        #endregion

        public override string ToString()
        {
            return string.Concat(
                "{X=", X.ToString(CultureInfo.CurrentCulture), 
                ",Y=", Y.ToString(CultureInfo.CurrentCulture),
                ",Width=",  Width.ToString(CultureInfo.CurrentCulture),
                ",Height=", Height.ToString(CultureInfo.CurrentCulture), "}");
        }
    }
}
