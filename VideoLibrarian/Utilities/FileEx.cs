//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="FileEx.cs" company="Chuck Hill">
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
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace VideoLibrarian
{
    public static class FileEx
    {
        /// <summary>
        /// Compute unique MD5 hash of file contents.
        /// DO NOT USE for security encryption.
        /// </summary>
        /// <param name="filename">File content to generate hash from.</param>
        /// <returns>Guid hash. Upon error (null, file not found, file locked, invalid permissions, etc) returns empty guid.</returns>
        public static Guid GetHash(string filename)
        {
            if (filename.IsNullOrEmpty()) return Guid.Empty;
            try
            {
                using (var fs = new FileStream(filename, FileMode.Open, System.Security.AccessControl.FileSystemRights.Read, FileShare.ReadWrite, 1024 * 1024, FileOptions.SequentialScan))
                {
                    var md5 = new MD5CryptoServiceProvider(); //to be multi-threaded compliant, this must not be a static variable.
                    var result = new Guid(md5.ComputeHash(fs));
                    md5.Dispose();
                    return result;
                }
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Get file extension (with leading '.') from url.
        /// If none found, assumes ".htm"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetUrlExtension(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                string ext = Path.GetExtension(uri.AbsolutePath).ToLower();
                if (ext.IsNullOrEmpty()) ext = ".htm";
                else if (ext == ".html") ext = ".htm";
                else if (ext == ".jpe") ext = ".jpg";
                else if (ext == ".jpeg") ext = ".jpg";
                else if (ext == ".jfif") ext = ".jpg";
                return ext;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Make absolute url from baseUrl + relativeUrl.
        /// If relativeUrl contains an absolute Url, returns that url unmodified.
        /// If any errors occur during combination of the two parts, string.Empty is returned.
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="relativeUrl"></param>
        /// <returns>absolute Url</returns>
        public static string GetAbsoluteUrl(string baseUrl, string relativeUrl)
        {
            try
            {
                return new Uri(new Uri(baseUrl), relativeUrl).AbsoluteUri;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Get earliest file or directory datetime.
        /// Empirically, it appears that the LastAccess or LastWrite times can be 
        /// earlier than the Creation time! For consistency, this method just 
        /// returns the earliest of these three file datetimes.
        /// </summary>
        /// <param name="filename">Full directory or filepath</param>
        /// <returns>DateTime</returns>
        public static DateTime GetCreationDate(string filename)
        {
            GetFileTime(filename, out long creationTime, out long lastAccessTime, out long lastWriteTime);
            long timeMin = creationTime;
            if (lastAccessTime < timeMin) timeMin = lastAccessTime;
            if (lastWriteTime < timeMin) timeMin = lastWriteTime;
            var dtMin = DateTime.FromFileTime(timeMin);

            //Forget hi-precision and DateTimeKind. It just complicates comparisons. This is more than good enough.
            return new DateTime(dtMin.Year, dtMin.Month, dtMin.Day, dtMin.Hour, dtMin.Minute, 0);
        }

        /// <summary>
        /// Get html file content as string without unecessary whitespace,
        /// no newlines and replace all double-quotes with single-quotes.
        /// Sometimes html quoting is done with single-quotes and sometimes with double-quotes. Note this breaks javascript and json.
        /// These fixups are all legal html and also make it easier to parse with Regex.
        /// </summary>
        /// <param name="filename">Name of HTML file to reformat.</param>
        /// <param name="noScript">True to remove everything between script, style, and svg tags.</param>
        /// <returns></returns>
        public static string ReadHtml(string filename, bool noScript = false)
        {
            string results = string.Empty;
            using (var reader = File.OpenText(filename))
            {
                StringBuilder sb = new StringBuilder((int)reader.BaseStream.Length);
                char prev_c = '\0';
                while (true)
                {
                    int i = reader.Read();
                    if (i == -1) break;
                    char c = (char)i;

                    if (c == '\t' || c == '\r' || c == '\n' || c == '\xA0' || c == '\x90' || c == '\x9D' || c == '\x9E') c = ' ';
                    if (c == '"') c = '\'';
                    if (c == '\x96' || c == '\x97' || c == '\xAD' || c == '\x2013') c = '-'; //replace various dash chars with standard ansi dash

                    if (c == ' ' && prev_c == ' ') continue;     //remove duplicate whitespace
                    if (c == '>' && prev_c == ' ') sb.Length--;  //remove whitespace before '>'
                    if (c == ' ' && prev_c == '>') continue;     //remove whitespace after '>'
                    if (c == '<' && prev_c == ' ') sb.Length--;  //remove whitespace before '<'
                    if (c == ' ' && prev_c == '<') continue;     //remove whitespace after '<'
                    if (c == '>' && prev_c == '/' && sb[sb.Length - 2] == ' ') { sb.Length -= 2; sb.Append('/'); } //remove whitespace before '/>'

                    sb.Append(c);
                    prev_c = c;
                }
                if (prev_c == ' ') sb.Length--;
                results = sb.ToString();
            }

            if (noScript)
            {
                results = RegexCache.RegEx(@"(<script.+?</script>)|(<style.+?</style>)|(<svg.+?</svg>)", RegexOptions.IgnoreCase).Replace(results, string.Empty);
            }

            return results;
        }

        #region Win32
        /// <summary>
        /// This is a low-level alternative to:
        ///    • System.IO.File.GetCreationTime()
        ///    • System.IO.File.GetLastWriteTime()
        ///    • System.IO.File.GetLastAccessTime()
        ///    and
        ///    • System.IO.File.SetCreationTime()
        ///    • System.IO.File.SetLastWriteTime()
        ///    • System.IO.File.SetLastAccessTime()
        /// The reason is sometimes some fields do not get set properly. File open/close 3 times in rapid succession?
        /// </summary>
        [DllImport("kernel32.dll")] private static extern bool SetFileTime(IntPtr hFile, ref long creationTime, ref long lastAccessTime, ref long lastWriteTime);
        [DllImport("kernel32.dll")] private static extern bool SetFileTime(IntPtr hFile, IntPtr creationTime, ref long lastAccessTime, ref long lastWriteTime);
        [DllImport("kernel32.dll")] private static extern bool SetFileTime(IntPtr hFile, ref long creationTime, IntPtr lastAccessTime, ref long lastWriteTime);
        [DllImport("kernel32.dll")] private static extern bool SetFileTime(IntPtr hFile, ref long creationTime, ref long lastAccessTime, IntPtr lastWriteTime);
        [DllImport("kernel32.dll")] private static extern bool SetFileTime(IntPtr hFile, IntPtr creationTime, IntPtr lastAccessTime, ref long lastWriteTime);
        [DllImport("kernel32.dll")] private static extern bool SetFileTime(IntPtr hFile, ref long creationTime, IntPtr lastAccessTime, IntPtr lastWriteTime);
        [DllImport("kernel32.dll")] private static extern bool SetFileTime(IntPtr hFile, IntPtr creationTime, ref long lastAccessTime, IntPtr lastWriteTime);

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false)]
        private static extern bool GetFileTime(IntPtr hFile, out long creationTime, out long lastAccessTime, out long lastWriteTime);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false)]
        private static extern bool CloseHandle(IntPtr hFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        private static extern bool DeleteFile(string path);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        private static extern bool CopyFile(string srcfile, string dstfile, bool failIfExists);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        private static extern bool MoveFileEx(string src, string dst, int dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false)]
        private static extern bool GetFileSizeEx(IntPtr hFile, out long lpFileSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        private static extern int GetFileAttributes(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        private static extern bool GetFileAttributesEx(string lpFileName, int flags, out WIN32_FILE_ATTRIBUTE_DATA fileData);

        [StructLayout(LayoutKind.Sequential)]
        private struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public FileAttributes dwFileAttributes;
            public long ftCreationTime;
            public long ftLastAccessTime;
            public long ftLastWriteTime;
            public long nFileSize;
        }
        #endregion

        /// <summary>
        /// Delete the specified file.
        /// </summary>
        /// <param name="filename">Full name of file to delete.</param>
        /// <returns>True if successfully deleted</returns>
        /// <remarks>
        /// Does not throw exceptions.
        /// </remarks>
        public static bool Delete(string filename) => DeleteFile(filename);

        /// <summary>
        ///  Copy a file to a new filename.
        /// </summary>
        /// <param name="srcfile">File name of source file</param>
        /// <param name="dstFile">File name of destination file</param>
        /// <param name="failIfExists"></param>
        /// <returns>True if successful</returns>
        /// <remarks>
        /// Does not throw exceptions.
        /// </remarks>
        public static bool Copy(string srcfile, string dstFile, bool failIfExists = false) => CopyFile(srcfile, dstFile, failIfExists);

        /// <summary>
        /// Move a file to a new destination.
        /// </summary>
        /// <param name="srcfile">File name of source file</param>
        /// <param name="dstFile">File name of destination file</param>
        /// <returns>True if successful</returns>
        /// <remarks>
        /// Does not throw exceptions.
        /// A pre-existing destination file is overwritten.
        /// May move files across drives.
        /// </remarks>
        public static bool Move(string srcfile, string dstFile) => MoveFileEx(srcfile, dstFile, 3);

        /// <summary>
        /// Get length of specified file 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>File length or -1 upon error.</returns>
        /// <remarks>
        /// Does not throw exceptions.
        /// </remarks>
        public static long Length(string filename)
        {
            bool success = GetFileAttributesEx(filename, 0, out WIN32_FILE_ATTRIBUTE_DATA fileData);
            if (!success) return -1L;
            return fileData.nFileSize;
        }

        /// <summary>
        /// Check if a file exists.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>True if file exists.</returns>
        /// <remarks>
        /// Does not throw exceptions.
        /// </remarks>
        public static bool Exists(string filename) => GetFileAttributes(filename) != -1;

        /// <summary>
        /// Get all 3 datetime fields for a given file in FileTime (64-bit) format.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="creationTime"></param>
        /// <param name="lastAccessTime"></param>
        /// <param name="lastWriteTime"></param>
        /// <returns>True if successful</returns>
        public static bool GetFileTime(string filename, out long creationTime, out long lastAccessTime, out long lastWriteTime)
        {
            creationTime = lastAccessTime = lastWriteTime = 0;

            //bool success = GetFileAttributesEx(filename, 0, out WIN32_FILE_ATTRIBUTE_DATA fileData);
            //if (!success) return false;
            //creationTime = fileData.ftCreationTime;
            //lastAccessTime = fileData.ftLastAccessTime;
            //lastWriteTime = fileData.ftLastWriteTime;

            var hFile = CreateFile(filename, 0x0080, 0x00000003, IntPtr.Zero, 3, 0x80, IntPtr.Zero);
            if (hFile == INVALID_HANDLE_VALUE) return false;
            bool success = GetFileTime(hFile, out creationTime, out lastAccessTime, out lastWriteTime);
            CloseHandle(hFile);
            return success;
        }

        /// <summary>
        /// Set datetime fields for a given file in FileTime (64-bit) format. Time field value 0 == not modified.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="creationTime"></param>
        /// <param name="lastAccessTime"></param>
        /// <param name="lastWriteTime"></param>
        /// <returns>True if successful</returns>
        public static bool SetFileTime(string filename, long creationTime, long lastAccessTime, long lastWriteTime)
        {
            bool success;
            var hFile = CreateFile(filename, 0x0100, 0x00000003, IntPtr.Zero, 3, 0x80, IntPtr.Zero);
            if (hFile == INVALID_HANDLE_VALUE) return false;

            var fields = (creationTime == 0 ? 0 : 1) | (lastAccessTime == 0 ? 0 : 2) | (lastWriteTime == 0 ? 0 : 4);

            switch (fields)
            {
                case 0x01: success = SetFileTime(hFile, ref creationTime, IntPtr.Zero, IntPtr.Zero); break;
                case 0x02: success = SetFileTime(hFile, IntPtr.Zero, ref lastAccessTime, IntPtr.Zero); break;
                case 0x03: success = SetFileTime(hFile, ref creationTime, ref lastAccessTime, IntPtr.Zero); break;
                case 0x04: success = SetFileTime(hFile, IntPtr.Zero, IntPtr.Zero, ref lastWriteTime); break;
                case 0x05: success = SetFileTime(hFile, ref creationTime, IntPtr.Zero, ref lastWriteTime); break;
                case 0x06: success = SetFileTime(hFile, IntPtr.Zero, ref lastAccessTime, ref lastWriteTime); break;
                case 0x07: success = SetFileTime(hFile, ref creationTime, ref lastAccessTime, ref lastWriteTime); break;
                default: success = false; break;
            }

            CloseHandle(hFile);
            return success;
        }
    }
}
