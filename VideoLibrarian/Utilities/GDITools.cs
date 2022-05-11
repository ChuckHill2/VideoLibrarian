using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace VideoLibrarian
{

    public static class GDI
    {
        public static void DrawRoundedRectangle(this Graphics g, Pen p, Rectangle r, int d)
        {
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();

            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            gp.AddLine(r.X, r.Y + r.Height - d, r.X, r.Y + d / 2);

            g.DrawPath(p, gp);
        }

        public static void FillRoundedRectangle(this Graphics g, Brush b, Rectangle r, int d)
        {
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();

            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            gp.AddLine(r.X, r.Y + r.Height - d, r.X, r.Y + d / 2);

            g.FillPath(b, gp);
        }

        /// <summary>
        /// Create image from control.
        /// </summary>
        /// <param name="ctl">Control to create image from.</param>
        /// <param name="filename">Optional: Save image to file.</param>
        /// <param name="userComment">Optional: Embed comment into image</param>
        /// <returns>Created image</returns>
        public static Bitmap ToImage(this Control ctl, string filename = null, string userComment = null)
        {
            return ToImage(ctl, new Bitmap(ctl.Width, ctl.Height, PixelFormat.Format24bppRgb), new Rectangle(0, 0, ctl.Width, ctl.Height), filename, userComment);
        }

        /// <summary>
        /// Write image of control to existing bitmap buffer.
        /// </summary>
        /// <param name="ctl">Control to create image from.</param>
        /// <param name="targetBmp">Bitmap to write to</param>
        /// <param name="targetRect">What portion of target bitmap to write to.</param>
        /// <param name="filename">Optional: Save image to file.</param>
        /// <param name="userComment">Optional: Embed comment into image</param>
        /// <returns>Updated target bitmap</returns>
        public static Bitmap ToImage(this Control ctl, Bitmap targetBmp, Rectangle targetRect, string filename = null, string userComment = null)
        {
            const int ExifModel = 0x0110;
            const int ExifUserComment = 0x9286;
            const int ExifStringType = 2;

            //Bitmap bm = new Bitmap(ctl.Width, ctl.Height);
            ctl.DrawToBitmap(targetBmp, targetRect);

            if (!userComment.IsNullOrEmpty())
            {
                if (userComment[userComment.Length - 1] != '\0')
                    userComment = string.Concat(userComment, "\0");

                var pi = FormatterServices.GetUninitializedObject(typeof(PropertyItem)) as PropertyItem;
                pi.Id = ExifModel;
                pi.Type = ExifStringType;
                pi.Value = Encoding.UTF8.GetBytes("MediaGuide\0");
                pi.Len = pi.Value.Length;
                targetBmp.SetPropertyItem(pi);

                pi = FormatterServices.GetUninitializedObject(typeof(PropertyItem)) as PropertyItem;
                pi.Id = ExifUserComment;
                pi.Type = ExifStringType;
                pi.Value = Encoding.UTF8.GetBytes(userComment);
                pi.Len = pi.Value.Length;
                targetBmp.SetPropertyItem(pi);
            }

            if (!filename.IsNullOrEmpty())
            {
                ImageFormat iFormat;
                switch (Path.GetExtension(filename).ToLower())
                {
                    case ".bmp": iFormat = ImageFormat.Bmp; break;
                    case ".emf": iFormat = ImageFormat.Emf; break;
                    case ".gif": iFormat = ImageFormat.Gif; break;
                    case ".ico": iFormat = ImageFormat.Icon; break;
                    case ".jpg": iFormat = ImageFormat.Jpeg; break;
                    case ".png": iFormat = ImageFormat.Png; break;
                    case ".tif": iFormat = ImageFormat.Tiff; break;
                    case ".wmf": iFormat = ImageFormat.Wmf; break;
                    default: return targetBmp;
                }

                if (FileEx.Exists(filename)) FileEx.Delete(filename);
                targetBmp.Save(filename, iFormat);
            }

            return targetBmp;
        }

        /// <summary>
        /// Retrieve embedded user comment string from bitmap.
        /// </summary>
        /// <param name="bmp">Bitmap to retrieve user comment string from.</param>
        /// <returns>User comment string or null if not found</returns>
        public static string UserComment(this Bitmap bmp)
        {
            const int ExifUserComment = 0x9286;
            const int ExifStringType = 2;

            var prop = bmp.PropertyItems.FirstOrDefault(x => x.Id == ExifUserComment);
            if (prop != null && prop.Type == ExifStringType && prop.Value.Length > 0)
            {
                var value = Encoding.UTF8.GetString(prop.Value);
                if (value[value.Length - 1] == '\0') value = value.Substring(0, value.Length - 1);
                return value;
            }

            return null;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap Resize(this Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, image.PixelFormat);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="nusize">The new dimensions resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap Resize(this Image image, Size nusize)
        {
            return Resize(image, nusize.Width, nusize.Height);
        }

        [DllImport("gdi32.dll")] private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        private enum DeviceCap { VERTRES = 10, DESKTOPVERTRES = 117, LOGPIXELSY = 90 }
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        /// Get current DPI scaling factor as a percentage
        /// </summary>
        /// <returns>Scaling percentage</returns>
        public static int DpiScalingFactor()
        {
            IntPtr hDC = IntPtr.Zero;
            try
            {
                hDC = GetDC(IntPtr.Zero);
                int logpixelsy = GetDeviceCaps(hDC, (int)DeviceCap.LOGPIXELSY);
                float dpiScalingFactor = logpixelsy / 96f;
                //Smaller - 100% == screenScalingFactor=1.0 dpiScalingFactor=1.0
                //Medium - 125% (default) == screenScalingFactor=1.0 dpiScalingFactor=1.25
                //Larger - 150% == screenScalingFactor=1.0 dpiScalingFactor=1.5
                return (int)(dpiScalingFactor * 100f);
            }
            finally
            {
                if (hDC != IntPtr.Zero) ReleaseDC(IntPtr.Zero, hDC);
            }
        }

        /// <summary>
        /// Blur an entire image by a scale value.
        /// </summary>
        /// <param name="image">Image to blur.</param>
        /// <param name="blurScale">A number greater 0.0 and less than 1.0</param>
        /// <returns>Blurred image</returns>
        public static Bitmap Blur(this Image original, double blurScale)
        {
            var b1 = new Bitmap(original, (int)(original.Width * blurScale), (int)(original.Height * blurScale));
            var b2 = new Bitmap(b1, original.Size);
            b1.Dispose();
            return b2;
        }

        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipLoadImageFromFile(string filename, out IntPtr image);
        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipDisposeImage(IntPtr image);
        private static readonly MethodInfo miFromGDIplus = typeof(Bitmap).GetMethod("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Load image file into Bitmap object without any validation. Supposedly about 
        /// 3x faster than 'new Bitmap(filename);'. Does not support Windows EMF 
        /// metafiles. This uses the file as a cache thus uses less memory but more CPU.
        /// </summary>
        /// <param name="filename">Name of image file to load.</param>
        /// <returns>Loaded cached Bitmap object</returns>
        public static Bitmap FastLoadFromFile(string filename)
        {
            filename = Path.GetFullPath(filename);
            IntPtr loadingImage = IntPtr.Zero;

            var errorCode = GdipLoadImageFromFile(filename, out loadingImage);
            if (errorCode != 0)
            {
                if (loadingImage != IntPtr.Zero) GdipDisposeImage(loadingImage);
                throw new Win32Exception(errorCode, "GdipLoadImageFromFile: GDI+ threw a status error code.");
            }

            return (Bitmap)miFromGDIplus.Invoke(null, new object[] { loadingImage });
        }

        /// <summary>
        /// Loads entire image file into Bitmap object. This slurps up the entire file into memory.
        /// No caching. Less CPU but more memory. Cannot use low level GdipCreateBitmapFromStream() 
        /// because it uses internal GPStream class which in turn uses virtual methods that are not 
        /// implemented. Other 3rd-party image readers don't appear to be any faster.
        /// </summary>
        /// <param name="filename">Name of image file to load.</param>
        /// <returns>Loaded Bitmap object</returns>
        public static Bitmap FastLoadFromFileStream(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open, System.Security.AccessControl.FileSystemRights.Read, FileShare.ReadWrite, 4096 * 8, FileOptions.RandomAccess))
            {
                return new Bitmap(fs);
            }
        }

        [DllImport("Shell32.dll")] private static extern int SHDefExtractIconW([MarshalAs(UnmanagedType.LPWStr)] string pszIconFile, int iIndex, int uFlags, out IntPtr phiconLarge, /*out*/ IntPtr phiconSmall, int nIconSize);
        /// <summary>Returns an icon of the specified size that is contained in the specified file.</summary>
        /// <param name="filePath">The path to the file that contains the icon.</param>
        /// <param name="size">Size of icon to retrieve.</param>
        /// <returns>The Icon representation of the image that is contained in the specified file. Must be disposed after use.</returns>
        /// <exception cref="System.ArgumentException">The parameter filePath does not indicate a valid file.-or- indicates a Universal Naming Convention (UNC) path.</exception>
        /// <remarks>
        /// Icons files contain multiple sizes and bit-depths of an image ranging from 16x16 to 256x256 in multiples of 8. Example: 16x16, 24x24, 32x32, 48x48, 64x64, 96x96, 128*128, 256*256.
        /// Icon.ExtractAssociatedIcon(filePath), retrieves only the 32x32 icon, period. This method will use the icon image that most closely matches the specified size and then resizes it to fit the specified size.
        /// </remarks>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shdefextracticonw"/>
        /// <see cref="https://devblogs.microsoft.com/oldnewthing/20140501-00/?p=1103"/>
        public static Icon ExtractAssociatedIcon(string filePath, int size)
        {
            const int SUCCESS = 0;
            IntPtr hIcon;

            if (SHDefExtractIconW(filePath, 0, 0, out hIcon, IntPtr.Zero, size) == SUCCESS)
            {
                return Icon.FromHandle(hIcon);
            }

            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        [DllImport("dwmapi.dll")] private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        /// <summary>
        /// Enable dropshadow to a borderless form. Unlike forms with borders, forms with FormBorderStyle.None have no dropshadow. 
        /// </summary>
        /// <param name="form">Borderless form to add dropshadow to. Must be called AFTER form handle has been created. see Form.Created or Form.Shown events.</param>
        /// <exception cref="InvalidOperationException">Must be called AFTER the form handle has been created.</exception>
        /// <see cref="https://stackoverflow.com/questions/60913399/border-less-winform-form-shadow/60916421#60916421"/>
        /// <remarks>
        /// This method does nothing if the form does not have FormBorderStyle.None.
        /// </remarks>
        public static void ApplyShadows(Form form)
        {
            if (form.FormBorderStyle != FormBorderStyle.None) return;
            if (Environment.OSVersion.Version.Major < 6) return;
            if (!form.IsHandleCreated) throw new InvalidOperationException("Must be called AFTER the form handle has been created.");

            var v = 2;
            DwmSetWindowAttribute(form.Handle, 2, ref v, 4);

            MARGINS margins = new MARGINS()
            {
                bottomHeight = 1,
                leftWidth = 0,
                rightWidth = 1,
                topHeight = 0
            };

            DwmExtendFrameIntoClientArea(form.Handle, ref margins);
        }
    }

}
