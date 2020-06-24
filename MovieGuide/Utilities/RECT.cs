using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace MovieGuide
{
    /// <summary>
    /// Used everywhere when using Win32 RECT's via pInvoke.
    /// Also use this instead of System.Drawing.Rectangle when XML serializing because Rectangle does not serialize well.
    /// Includes implicit conversion between this RECT structure and System.Drawing.Rectangle and System.Drawing.RectangleF.
    /// </summary>
    [XmlInclude(typeof(Rectangle))] //necessary when using implicit operators
    [XmlInclude(typeof(RectangleF))] //necessary when using implicit operators
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        [XmlIgnore]
        public int Left, Top, Right, Bottom;

        public RECT(int left, int top, int right, int bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }
        public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }
        public RECT(RectangleF r) : this((int)(r.Left + 0.5f), (int)(r.Top + 0.5f), (int)(r.Right + 0.5f), (int)(r.Bottom + 0.5f)) { }

        [XmlAttribute]
        public int X { get { return Left; } set { Right -= (Left - value); Left = value; } }

        [XmlAttribute]
        public int Y { get { return Top; } set { Bottom -= (Top - value); Top = value; } }

        [XmlAttribute]
        public int Height { get { return Bottom - Top; } set { Bottom = value + Top; } }

        [XmlAttribute]
        public int Width { get { return Right - Left; } set { Right = value + Left; } }

        [XmlIgnore]
        public Point Location { get { return new Point(Left, Top); } set { X = value.X; Y = value.Y; } }

        [XmlIgnore]
        public Size Size { get { return new Size(Width, Height); } set { Width = value.Width; Height = value.Height; } }

        public static implicit operator Rectangle(RECT r) { return new Rectangle(r.Left, r.Top, r.Width, r.Height); }
        public static implicit operator RectangleF(RECT r) { return new RectangleF(r.Left, r.Top, r.Width, r.Height); }
        public static implicit operator RECT(Rectangle r) { return new RECT(r); }
        public static implicit operator RECT(RectangleF r) { return new RECT(r); }
        public static bool operator ==(RECT r1, RECT r2) { return r1.Equals(r2); }
        public static bool operator !=(RECT r1, RECT r2) { return !r1.Equals(r2); }

        public bool Equals(RECT r) { return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom; }
        public bool Equals(Rectangle r) { return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom; }
        public bool Equals(RectangleF r) { return (int)(r.Left + 0.5f) == Left && (int)(r.Top + 0.5f) == Top && (int)(r.Right + 0.5f) == Right && (int)(r.Bottom + 0.5f) == Bottom; }

        public override bool Equals(object obj)
        {
            if (obj is RECT) return Equals((RECT)obj);
            else if (obj is Rectangle) return Equals((Rectangle)obj);
            else if (obj is RectangleF) return Equals((RectangleF)obj);
            return false;
        }

        public override int GetHashCode() { return Left ^ ((Top << 13) | (Top >> 0x13)) ^ ((Width << 0x1a) | (Width >> 6)) ^ ((Height << 7) | (Height >> 0x19)); }

        public override string ToString()
        {
            return string.Concat(
                "{X=", Left.ToString(CultureInfo.CurrentCulture),
                ",Y=", Top.ToString(CultureInfo.CurrentCulture),
                ",Width=", Width.ToString(CultureInfo.CurrentCulture),
                ",Height=", Height.ToString(CultureInfo.CurrentCulture), "}");
        }

        /// <summary>
        /// Copy content of IntPtr to a RECT structure. Useful in WndProc Message IntPtr's.
        /// Also see: public object System.Windows.Forms.Message.GetLParam(Type cls);
        /// </summary>
        /// <param name="lParam">Pointer to read from.</param>
        /// <returns></returns>
        public static RECT CopyToRECT(IntPtr lParam)
        {
            if (lParam == IntPtr.Zero) return new RECT();
            return (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
        }

        /// <summary>
        /// Copy RECT structure to a pre-allocated IntPtr. Useful in WndProc Message IntPtr's.
        /// </summary>
        /// <param name="lParam">Pointer to write 16 bytes to.</param>
        public void CopyToIntPtr(IntPtr lParam)
        {
            if (lParam == IntPtr.Zero) throw new ArgumentNullException("IntPtr is Zero");
            Marshal.StructureToPtr(this, lParam, false);
        }
    }
}
