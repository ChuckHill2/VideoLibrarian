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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace VideoLibrarian
{
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
        /// <summary>
        /// Trim, remove empty lines, and indent suceeding lines of multiline string.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="indentAmount"></param>
        /// <returns></returns>
        public static string Indent(this string s, int indentAmount = 4)
        {
            if (indentAmount < 0) throw new ArgumentOutOfRangeException(nameof(indentAmount), indentAmount, "The indent amount must be greatr than 0.");
            if (string.IsNullOrEmpty(s)) return s;
            if (!s.Contains('\n')) return s.Trim();
            string indent = new string(' ', indentAmount);
            int row = 0;
            return RegexCache.RegEx(@"^\s*(.+?)\s*$", RegexOptions.Multiline).Replace(s, m =>
            {
                row++;
                if (row == 1) return m.Groups[1].Value;
                return indent + m.Groups[1].Value;
            });
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

        /// <summary>
        /// Truncate string to specified length and add ellipsis if string length exceeds maximum specified length.
        /// </summary>
        /// <param name="s">String to truncate</param>
        /// <param name="textAlign">Which side of string to truncate: left, right, or center</param>
        /// <param name="maxLen">Maximum length to truncate string to.</param>
        /// <returns></returns>
        public static string Truncate(this string s, HorizontalAlignment textAlign = HorizontalAlignment.Left, int maxLen = 40)
        {
            if (maxLen < 3) throw new ArgumentOutOfRangeException(nameof(maxLen), maxLen, "Maximum length must be greater than 3.");
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length <= maxLen) return s;

            if (textAlign== HorizontalAlignment.Left)
            {
                return s.Substring(0, maxLen-1) + "\x2026";
            }
            if (textAlign == HorizontalAlignment.Right)
            {
                var len = maxLen - 1;
                return "\x2026" + s.Substring(s.Length - len, len);
            }
            if (textAlign == HorizontalAlignment.Center)
            {
                var rlen = maxLen / 2 + ((maxLen % 2) - 1);
                return string.Concat(s.Substring(0, maxLen / 2), "\x2026", s.Substring(s.Length - rlen, rlen));
            }

            return s; //will never get here because all enum states are covered.
        }

        public static bool IsNullOrEmpty(this string s, bool lookForWhitespace=false) 
        {
            //return string.IsNullOrWhiteSpace(s);
            if (s == null) return true;
            if (s.Length==0) return true;
            if (lookForWhitespace && s.All(c=>c==' ')) return true;
            return false;
        }

        public static bool IsNullOrEmpty(this ICollection arr)
        {
            if (arr == null) return true;
            if (arr.Count == 0) return true;
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

        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> match) where T : class
        {
            var index = 0;
            foreach (var item in source)
            {
                if (match.Invoke(item)) return index;
                index++;
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
            //Unfortunately the entire IEnumerable array has to be evaluated because we have to act on the last item first.

            if (source is IList<T> list) 
            {
                var length = list.Count;
                for (int i = length - 1; i >= 0; i--)
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
                //We don't know the length, so we simulate List<T>() but auto-expand the array ourselves for efficiency.
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

                if (!FileEx.Exists(s[0]))
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
            Log.Write(Severity.Verbose, $"Exec: {fn} {si.Arguments??""}");

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

        private const int SW_RESTORE = 9;
        [DllImport("User32.dll")] private static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("User32.dll")] private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [DllImport("User32.dll")] private static extern bool IsIconic(IntPtr handle);

        /// <summary>
        /// Allow only one instance of this executable to run.
        /// The other instance window is popped open to the foreground and this instance terminates.
        /// This should be the first line of Program.cs:Program.Main().
        /// </summary>
        public static void AllowOnlyOneInstance()
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);
            if (processes.Length <= 1) return;  //should never be zero. 1 or 2 only.
            var currentPid = currentProcess.Id;
            var otherProcess = processes.FirstOrDefault(p => p.Id != currentPid);
            if (otherProcess == null) return; //should never occur.

            IntPtr handle = otherProcess.MainWindowHandle;
            if (handle == IntPtr.Zero) return; //should never occur.
            if (IsIconic(handle)) ShowWindow(handle, SW_RESTORE);
            SetForegroundWindow(handle);

            Environment.Exit(1);
        }
    }
}
