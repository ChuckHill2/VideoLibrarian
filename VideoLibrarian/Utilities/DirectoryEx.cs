//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="DirectoryEx.cs" company="Chuck Hill">
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
// <repository>https://github.com/ChuckHill2/ChuckHill2.Utilities</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace VideoLibrarian
{
    /// <summary>
    /// Directory Management Utilities
    /// </summary>
    public static class DirectoryEx
    {
        #region Win32
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public ulong ftCreationTime;
            public ulong ftLastAccessTime;
            public ulong ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);
        private const int ERROR_NO_MORE_FILES = 18;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FindClose(IntPtr hFindFile);
        #endregion

        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// Same as System.IO.Directory.EnumerateFiles(), except instead of simple wildcard search patterns, this allows complex regular expressions.
        /// </summary>
        /// <param name="rootFolder">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="regexFilenamePattern">The regex search string to match against the names of files in path. Not case-sensitive. Does not match folder names.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories. The default value is System.IO.SearchOption.TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by path and that match the specified search pattern.</returns>
        public static IEnumerable<string> EnumerateFiles(string rootFolder, string regexFilenamePattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var matcher = new Regex(regexFilenamePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return EnumerateFiles(rootFolder, matcher, searchOption);
        }
        private static IEnumerable<string> EnumerateFiles(string folder, Regex matcher, SearchOption searchOption)
        {
            int lastErr = 0;
            WIN32_FIND_DATA fd = new WIN32_FIND_DATA();
            IntPtr hFind = FindFirstFile(Path.Combine(folder, "*"), out fd);
            if (hFind == INVALID_HANDLE_VALUE)
            {
                lastErr = Marshal.GetLastWin32Error();
                if (lastErr != 0) throw new Win32Exception(lastErr);
                yield break;
            }

            do
            {
                if (fd.cFileName == "." || fd.cFileName == "..") continue;   //pseudo-directory
                string path = Path.Combine(folder, fd.cFileName);
                if ((fd.dwFileAttributes & FileAttributes.Directory) != 0)
                {
                    if (searchOption == SearchOption.AllDirectories)
                    {
                        foreach (var x in EnumerateFiles(path, matcher, searchOption))
                        {
                            yield return x;
                        }
                    }

                    continue;
                }

                if (matcher != null && !matcher.IsMatch(fd.cFileName)) continue;

                yield return path;

            } while (FindNextFile(hFind, out fd));
            lastErr = Marshal.GetLastWin32Error();
            if (lastErr != 0 && lastErr != ERROR_NO_MORE_FILES) throw new Win32Exception(lastErr);

            if (!FindClose(hFind)) throw new Win32Exception();

            yield break;
        }

        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// Same as System.IO.Directory.EnumerateFiles(), except 42% faster on a SSD.
        /// To filter list, use this.Where(m=>m.something(m)) linq clause.
        /// </summary>
        /// <param name="folder">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories. The default value is System.IO.SearchOption.TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of all the full names (including paths) for the files in the root directory specified by 'folder'.</returns>
        public static IEnumerable<string> EnumerateAllFiles(string folder, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // All of the following +more are all handled by Win32Exception().
            // if (folder==null) throw new ArgumentNullException(nameof(folder));
            // if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException("Cannot be empty.", nameof(folder));
            // if (folder.IndexOfAny(Path.GetInvalidPathChars())!=-1) throw new ArgumentException("Must be a directory.", nameof(folder));
            // if (!Directory.Exists(folder)) throw new DirectoryNotFoundException($"Directory {folder} not found.");

            int lastErr = 0;
            WIN32_FIND_DATA fd = new WIN32_FIND_DATA();
            IntPtr hFind = FindFirstFile(Path.Combine(folder, "*"), out fd);
            if (hFind == INVALID_HANDLE_VALUE)
            {
                lastErr = Marshal.GetLastWin32Error();
                if (lastErr != 0) throw new Win32Exception(lastErr);
                yield break;
            }

            do
            {
                if (fd.cFileName == "." || fd.cFileName == "..") continue;   //pseudo-directory
                string path = Path.Combine(folder, fd.cFileName);
                if ((fd.dwFileAttributes & FileAttributes.Directory) != 0)
                {
                    if (searchOption == SearchOption.AllDirectories)
                    {
                        foreach (var x in EnumerateAllFiles(path, searchOption))
                        {
                            yield return x;
                        }
                    }

                    continue;
                }

                yield return path;

            } while (FindNextFile(hFind, out fd));
            lastErr = Marshal.GetLastWin32Error();
            if (lastErr != 0 && lastErr != ERROR_NO_MORE_FILES) throw new Win32Exception(lastErr);

            if (!FindClose(hFind)) throw new Win32Exception();

            yield break;
        }

        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// Same as System.IO.Directory.EnumerateFiles(), except 42% faster on a SSD.
        /// To filter list, use this.Where(m=>m.something(m)) linq clause.
        /// </summary>
        /// <param name="folder">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories. The default value is System.IO.SearchOption.TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of all the full directory paths in the root directory specified by 'folder'.</returns>
        public static IEnumerable<string> EnumerateAllFolders(string folder, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            int lastErr = 0;
            WIN32_FIND_DATA fd = new WIN32_FIND_DATA();
            IntPtr hFind = FindFirstFile(Path.Combine(folder, "*"), out fd);
            if (hFind == INVALID_HANDLE_VALUE)
            {
                lastErr = Marshal.GetLastWin32Error();
                if (lastErr != 0) throw new Win32Exception(lastErr);
                yield break;
            }

            do
            {
                if (fd.cFileName == "." || fd.cFileName == "..") continue;   //pseudo-directory
                string path = Path.Combine(folder, fd.cFileName);
                if ((fd.dwFileAttributes & FileAttributes.Directory) != 0)
                {
                    yield return path;

                    if (searchOption == SearchOption.AllDirectories)
                    {
                        foreach (var x in EnumerateAllFolders(path, searchOption))
                        {
                            yield return x;
                        }
                    }

                    continue;
                }

                yield return path;

            } while (FindNextFile(hFind, out fd));
            lastErr = Marshal.GetLastWin32Error();
            if (lastErr != 0 && lastErr != ERROR_NO_MORE_FILES) throw new Win32Exception(lastErr);

            if (!FindClose(hFind)) throw new Win32Exception();

            yield break;
        }
    }
}
