//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Common.cs" company="Chuck Hill">
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public enum Severity
    {
        None,    //No severity level written
        Success, //Action successful, Color.Green
        Error,   //Action failed, Color.Red
        Warning, //Action failed but recovered, Color.Gold
        Info,    //Action status, Color.Blue
        Verbose  //Detailed action status, Color.Purple
    }

    public static class Log
    {
        private static readonly string _logName = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".log");
        public static string LogName { get { return _logName; } }
        private static StreamWriter LogStream = null;
        private static readonly object lockObj = new object();

        public static event Action<Severity, string> MessageCapture;

        public static void Write(Severity severity, string fmt, params object[] args)
        {
            if (fmt == null && LogStream == null) return; //Nothing to do
            if (fmt == null && LogStream != null) //Close
            {
                lock (lockObj) { LogStream.Close(); LogStream.Dispose(); LogStream = null; }
                return;
            }
            if (fmt != null && LogStream == null) //Open
            {
                lock (lockObj)
                {
                    //Roll over log at 100MB
                    if (File.Exists(LogName) && new FileInfo(LogName).Length > (1024 * 1024 * 100)) File.Delete(LogName);
                    var fs = File.Open(LogName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    LogStream = new StreamWriter(fs) { AutoFlush = true };
                    LogStream.WriteLine(@"-------- {0:MM/dd/yyyy hh:mm:ss tt} ------------------------------------------", DateTime.Now); 
                }
            }

            if (LogStream != null) lock (lockObj)
            {
                if (severity != Severity.None) LogStream.Write(severity.ToString() + ": ");
#if DEBUG
                //Cleanup string and indent succeeding lines
                if (args != null && args.Length > 0)
                    fmt = string.Format(fmt, args);
                fmt = fmt.Beautify(false, "    ").TrimStart();
                LogStream.WriteLine(fmt);
#else
                //Cleanup string and indent succeeding lines. But as this is release
                //mode, exceptions show only the message not the entire call stack.
                //Users wouldn't know what to do with the call stack, anyway.
                if (args != null && args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] is Exception) args[i] = ((Exception)args[i]).Message;
                    }
                    fmt = string.Format(fmt, args);
                }
                fmt = fmt.Beautify(false, "    ").TrimStart();
                LogStream.WriteLine(fmt); 
#endif
                LogStream.BaseStream.Flush();
                MessageCapture?.Invoke(severity, fmt);
            }
        }

        public static void Dispose() => Write(0, null);
    }

    public static class Diagnostics
    {
        /// <summary>
        /// Write string to debug output.
        /// Uses Win32 OutputDebugString() or System.Diagnostics.Trace.Write() if running under a debugger.
        /// The reason for all this trickery is due to the fact that OutputDebugString() output DOES NOT get
        /// written to VisualStudio output window. Trace.Write() does write to the VisualStudio output window
        /// (by virtue of OutputDebugString somewhere deep inside), BUT it also is can be redirected
        /// to other destination(s) in the app config. This API Delegate is a compromise.
        /// </summary>
        private static readonly WriteDelegate _rawWrite = (System.Diagnostics.Debugger.IsAttached ? (WriteDelegate)new System.Diagnostics.DefaultTraceListener().Write : (WriteDelegate)OutputDebugString);
        private delegate void WriteDelegate(string msg);
        [DllImport("Kernel32.dll")]  private static extern void OutputDebugString(string errmsg);

        [Conditional("DEBUG")]
        public static void WriteLine(string msg, params object[] args)
        {
            if (args != null && args.Length > 0) msg = string.Format(msg, args);
            if (msg[msg.Length - 1] != '\n') msg += Environment.NewLine;
            //Prefix diagnostic message with something unique that can be filtered upon by DebugView.exe
            _rawWrite("DEBUG: " + msg);
        }
    }

    /// <summary>
    /// Generic Equality comparer, so a new unique Equality comparer does not need to be hand crafted.  
    /// For methods that require an IEqualityComparer to be passed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EqualityComparer<T> : IEqualityComparer, IEqualityComparer<T>
    {
        private Func<T, T, bool> _equals;
        private Func<T, int> _hashCode;

        public EqualityComparer()
        {
        }

        public EqualityComparer(Func<T, T, bool> equals)
        {
            _equals = equals;
        }

        public EqualityComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
        {
            _equals = equals;
            _hashCode = hashCode;
        }

        public bool Equals(T x, T y)
        {
            if (x==null && y==null) return true;
            if (x==null || y==null) return false;
            return _equals==null ? x.Equals(y) : _equals(x,y);
        }

        public int GetHashCode(T obj)
        {
            return _hashCode==null ? obj.GetHashCode() : _hashCode(obj);
        }

        public new bool Equals(object x, object y)
        {
            return Equals((T)x, (T)y);
        }

        public int GetHashCode(object obj)
        {
            return GetHashCode((T)obj);
        }
    }

    public static class DateTimeEx
    {
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1);
        private static readonly string dateFormat = GetDateFormat();

        /// <summary>
        /// Get localized date WITHOUT the day-of-week. Can't use "D" format because it includes the day-of-week!
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToDateString(this DateTime dt)
        {
            return dt.ToString(dateFormat);
        }

        //Get localized date format WITHOUT the day-of-week. Can't use "D" format because it includes the day-of-week!
        private static string GetDateFormat()
        {
            var dtf = CultureInfo.CurrentCulture.DateTimeFormat;
            string[] patterns = dtf.GetAllDateTimePatterns();
            string longPattern = dtf.LongDatePattern;
            string acceptablePattern = String.Empty;

            foreach (string pattern in patterns)
            {
                if (longPattern.Contains(pattern) && !pattern.Contains("ddd") && !pattern.Contains("dddd"))
                {
                    if (pattern.Length > acceptablePattern.Length)
                    {
                        acceptablePattern = pattern;
                    }
                }
            }

            if (String.IsNullOrEmpty(acceptablePattern))
            {
                return longPattern;
            }
            return acceptablePattern;
        }

        /// <summary>
        /// Gets the build/link timestamp from the specified executable file header.
        /// WARNING: When compiled in a .netcore application/library, the PE timestamp 
        /// is NOT set with the the application link time. It contains some other non-
        /// timestamp (hash?) value. To force the .netcore linker to embed the true 
        /// timestamp as previously, add the csproj property 
        /// "<Deterministic>False</Deterministic>".
        /// </summary>
        /// <param name="asm">Assembly to retrieve build date from</param>
        /// <returns>The local DateTime that the specified assembly was built.</returns>
        public static DateTime PEtimestamp(string filePath)
        {
            uint TimeDateStamp = 0;
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //Minimum possible executable file size.
                if (stream.Length < 268) throw new BadImageFormatException("Not a PE file. File too small.", filePath);
                //The first 2 bytes in file == IMAGE_DOS_SIGNATURE, 0x5A4D, or MZ.
                if (stream.ReadByte() != 'M' || stream.ReadByte() != 'Z') throw new BadImageFormatException("Not a PE file. DOS Signature not found.", filePath);
                stream.Position = 60; //offset of IMAGE_DOS_HEADER.e_lfanew
                stream.Position = ReadUInt32(stream); // e_lfanew = 128
                uint ntHeadersSignature = ReadUInt32(stream); // ntHeadersSignature == 17744 aka "PE\0\0"
                if (ntHeadersSignature != 17744) throw new BadImageFormatException("Not a PE file. NT Signature not found.", filePath);
                stream.Position += 4; //offset of IMAGE_FILE_HEADER.TimeDateStamp
                TimeDateStamp = ReadUInt32(stream); //unix-style time_t value
            }

            DateTime returnValue = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimeDateStamp);
            returnValue = returnValue.ToLocalTime();

            if (returnValue < new DateTime(2000, 1, 1) || returnValue > DateTime.Now)
            {
                //PEHeader link timestamp field is random junk because csproj property "Deterministic" == true
                //so we just return the 2nd best "build" time (iffy, unreliable).
                return File.GetCreationTime(filePath);
            }

            return returnValue;
        }

        /// <summary>
        /// Gets the build/link timestamp from the specified assembly file header.
        /// </summary>
        /// <param name="asm">Assembly to retrieve build date from</param>
        /// <returns>The local DateTime that the specified assembly was built.</returns>
        public static DateTime PEtimestamp(this Assembly asm)
        {
            if (asm.IsDynamic)
            {
                //The assembly was dynamically built in-memory so the build date is Now. Besides, 
                //accessing the location of a dynamically built assembly will throw an exception!
                return DateTime.Now;
            }

            return PEtimestamp(asm.Location);
        }

        /// <summary>
        /// Support utility exclusively for PEtimestamp()
        /// </summary>
        /// <param name="fs">File stream</param>
        /// <returns>32-bit unsigned int at current offset</returns>
        private static uint ReadUInt32(FileStream fs)
        {
            byte[] bytes = new byte[4];
            fs.Read(bytes, 0, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }

    }

    public static class CommonExtensions
    {
        public static string Beautify(this string s, bool stripComments, string indent)
        {
            if (stripComments)
            {
                s = Regex.Replace(s, @"^[ \t]*(--|//).*?\r\n", "", RegexOptions.Multiline); //remove whole line sql or c++ comments
                s = Regex.Replace(s, @"[ \t]*(--|//).*?$", "", RegexOptions.Multiline); //remove trailing sql or c++ comments
                s = Regex.Replace(s, @"\r\n([ \t]*/\*.*?\*/[ \t]*\r\n)+", "\r\n", RegexOptions.Singleline); //remove whole line c-like comments
                s = Regex.Replace(s, @"[ \t]*/\*.*?\*/[ \t]*", "", RegexOptions.Singleline); //remove trailing c-like comments
            }

            s = s.Trim().Replace("\t", "  "); //replace tabs with 2 spaces
            s = Regex.Replace(s, @" +$", "", RegexOptions.Multiline); //remove trailing whitespace
            s = Regex.Replace(s, "(\r\n){2,}", "\r\n"); //squeeze out multiple newlines
            if (!string.IsNullOrEmpty(indent)) s = Regex.Replace(s, @"^(.*)$", indent + "$1", RegexOptions.Multiline);  //indent
            return s;
        }

        /// <summary>
        /// Strip one or more whitspace chars (including newlines) and replace with a single space char.
        /// </summary>
        /// <param name="s">String to operate upon</param>
        /// <returns>fixed up single-line string</returns>
        public static string Squeeze(this string s)
        {
            if (s.IsNullOrEmpty()) return string.Empty;
            //This is 2.6x faster than ""return Regex.Replace(s.Trim(), "[\r\n \t]+", " ");""
            StringBuilder sb = new StringBuilder(s.Length);
            char prev = ' ';
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c > 0 && c < 32) c = ' ';
                if (prev == ' ' && prev == c) continue;
                prev = c;
                sb.Append(c);
            }
            if (prev == ' ') sb.Length = sb.Length - 1;
            return sb.ToString();
        }

        public static bool IsNullOrEmpty(this string s, bool lookForWhitespace=false) 
        {
            //return string.IsNullOrWhiteSpace(s);
            if (s == null) return true;
            if (s.Length==0) return true;
            if (lookForWhitespace && s.All(c=>c==' ')) return true;
            return false;
        }

        public static int IndexOf<T>(this IList<T> list, Func<T, bool> match) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (match(list[i])) return i;
            }
            return -1;
        }

        public static bool EqualsI(this string s, string value)
        {
            if (s == null && value == null) return true;
            return (s != null && value != null && s.Equals(value, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool ContainsI(this string s, string value)
        {
            if (s == null && value == null) return true;
            return (s != null && value != null && s.IndexOf(value, 0, StringComparison.InvariantCultureIgnoreCase) != -1);
        }

        public static string Attribute<T>(this Assembly asm) where T : Attribute
        {
            foreach (var data in asm.CustomAttributes)
            {
                if (typeof(T) != data.AttributeType) continue;
                if (data.ConstructorArguments.Count > 0) return data.ConstructorArguments[0].Value.ToString();
                break;
            }
            return string.Empty;
        }

        /// <summary>
        /// Perform action upon each item in enumerable loop, ascending.
        /// </summary>
        /// <typeparam name="T">Type of item in enumeration</typeparam>
        /// <param name="source">Enumerable array</param>
        /// <param name="action">Action to perform on each item in enumeration. Return true to continue to next item or false to break enumeration</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) return;

            if (source is IList<T> list)
            {
                var k = list.Count;
                for (int i = 0; i < k; i++)
                {
                    action(list[i]);
                    k = list.Count;
                }
            }
            else
            {
                foreach (var v in source) action(v);
            }
        }

        /// <summary>
        /// Perform action upon each item in enumerable loop, descending (reverse order).
        /// Useful when the count of items may change.
        /// </summary>
        /// <typeparam name="T">Type of item in enumeration</typeparam>
        /// <param name="source">Enumerable array</param>
        /// <param name="action">Action to perform on each item in enumeration. Return true to continue to next item or false to break enumeration</param>
        public static void ForEachDesc<T>(this IEnumerable<T> source, Func<T,bool> action)
        {
            //Action on sequence items is performed in descending order just in case elements are removed by the action.
            //Equivalant to: foreach (var v in source.Reverse()) action(v); but more efficient.

            if (source is IList<T> list)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                    if (!action(list[i])) break;
            }
            else if (source is ICollection<T> collection)
            {
                var length = collection.Count;
                if (length == 0) return;
                T[] array = new T[length];
                collection.CopyTo(array, 0);

                for (int i = length - 1; i >= 0; i--)
                    if (!action(array[i])) break;
            }
            else
            {
                T[] array = null;
                int length = 0;
                foreach (T element in source)
                {
                    if (array == null) array = new T[4];
                    else if (array.Length == length)
                    {
                        T[] elementArray = new T[checked(length * 2)];
                        Array.Copy((Array)array, 0, (Array)elementArray, 0, length);
                        array = elementArray;
                    }
                    array[length] = element;
                    ++length;
                }

                for (int i = length - 1; i >= 0; i--)
                    if (!action(array[i])) break;
            }
        }

        /// <summary>
        /// Create a shallow copy of any object. Nested class objects
        /// are not duplicated. They are just referenced again.
        /// Object does not need to be marked as [Serializable].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Object to copy</param>
        /// <returns>copy of object</returns>
        public static T MemberwiseClone<T>(this T obj)
        {
            if (obj == null) return default(T);
            return (T)MemberwiseCloneMethod.Invoke(obj, new object[0]);
        }
        private static readonly MethodInfo MemberwiseCloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
    }

    public static class FormsExtensions
    {
        [DllImport("user32.dll")]
        private extern static IntPtr SendMessage(IntPtr hWnd, int msg, bool wParam, int lParam);
        private const int WM_SETREDRAW = 0x000B;

        public static void SuspendDrawing(this Control ctrl)
        {
            SendMessage(ctrl.Handle, WM_SETREDRAW, false, 0); //Stop redrawing
        }

        public static void ResumeDrawing(this Control ctrl)
        {
            SendMessage(ctrl.Handle, WM_SETREDRAW, true, 0);  //Turn on redrawing
            ctrl.Invalidate();
            ctrl.Refresh();
        }

        public static Rectangle ToParentRect(this Control parent, Control child)
        {
            var p = child.Parent;
            var rc = child.Bounds;
            while(p != null)
            {
                rc.X += p.Bounds.X;
                rc.Y += p.Bounds.Y;
                if (p == parent) break;
                p = p.Parent;
            }

            return rc;
        }

        public static T FindParent<T>(this Control ctl)
        {
            while (ctl != null)
            {
                if (ctl is T) return (T)(object)ctl;
                if (ctl.Tag is T) return (T)(object)ctl.Tag; //occurs with SummaryPopup
                ctl = ctl.Parent;
            }
            return default(T);
        }
    }

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

                if (File.Exists(filename)) File.Delete(filename);
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
            using (var fs = new FileStream(filename, FileMode.Open, System.Security.AccessControl.FileSystemRights.Read, FileShare.ReadWrite, 4096*8, FileOptions.RandomAccess))
            {
                return new Bitmap(fs);
            }
        }
    }

    public static class ProcessEx
    {
        /// <summary>
        /// Open file or url with supplied executable.
        /// </summary>
        /// <param name="exe">Executable used to open 'arg'. May include command-line arguments. if null or empty, uses the system default executable for the arg type.</param>
        /// <param name="arg">The full filename or URL to open.</param>
        public static void OpenExec(string exe, string arg)
        {
            var dir = string.Empty;

            if (!arg.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                dir = Path.GetDirectoryName(arg);
                arg = Path.GetFileName(arg);
            }

            //Split path from potentially long movie name to minimize chance 
            //of exception when full path exceeds the maximum path length.
            var si = new ProcessStartInfo();
            if (exe.IsNullOrEmpty()) //use system default executable
            {
                si.FileName = arg;
            }
            else
            {
                var s = SplitProcessCommandline(exe); //split executable and command-line args

                if (!File.Exists(s[0]))
                {
                    Log.Write(Severity.Warning, "Executable not found: " + s[0]);
                    si.FileName = arg;
                }
                else
                {
                    si.FileName = s[0];
                    si.Arguments = $"{s[1]} \"{arg}\"".Trim();
                }
            }

            si.WorkingDirectory = dir; //start within directory

            var fn = si.FileName;
            if (fn.Contains(' ')) fn = String.Concat("\"", fn, "\"");
            Log.Write(Severity.Info, $"Exec: {fn} {si.Arguments??""}");

            Process.Start(si);
        }

        //Split the settings Browser or VideoPlayer command where s[0]== the executable, and s[1]== any optional command-line arguments.
        public static string[] SplitProcessCommandline(string path)
        {
            if (path.IsNullOrEmpty()) return new string[] { "", "" };

            int idx = path.IndexOf(".exe", 0, StringComparison.CurrentCultureIgnoreCase);
            if (idx <= 0) return new string[] { "", "" }; //not an executable

            return new string[]
            {
                path.Substring(0, idx + 4),
                path.Substring(idx + 4)
            };
        }
    }
}
