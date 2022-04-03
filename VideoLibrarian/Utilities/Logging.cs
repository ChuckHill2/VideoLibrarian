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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace VideoLibrarian
{
    /// <summary>
    /// Logging severity levels
    /// </summary>
    public enum Severity
    {
        None,    //Special: No severity level written, but message always written.
        Success, //Action successful, Color.ForestGreen
        Error,   //Action failed, Color.Red
        Warning, //Action failed but recovered, Color.Gold
        Info,    //Action status, Color.MediumBlue
        Verbose  //Detailed action status, Color.LightBlue
    }

    /// <summary>
    /// Official event logging.
    /// </summary>
    public static class Log
    {
        private static readonly string _logName = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".log");
        public static string LogName { get { return _logName; } }
        private static StreamWriter LogStream = null;
        private static readonly object lockObj = new object();

        /// <summary>
        /// Event handler for cloning messages to an alternate destination.
        /// </summary>
        public static event Action<Severity, string> MessageCapture;

        /// <summary>
        /// Only allow messages this severe to be logged. Less severe messages are ignored.
        /// </summary>
        public static Severity SeverityFilter { get; set; } = Severity.Info; //default == all except verbose messages

        /// <summary>
        /// Write log message with severity level.
        /// </summary>
        /// <param name="severity">Severity level</param>
        /// <param name="fmt">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks>
        /// An interpolated format string may be used, however evaluation may be postponed when using a composite format
        /// string.  This is more efficient when some severity levels are ignored.In addition, passing exception objects get
        /// trimmed to just the message part for all messages except Verbose messages or when Verbose severity filter is set.
        /// </remarks>
        public static void Write(Severity severity, string fmt, params object[] args)
        {
            if (severity > SeverityFilter) return;

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

            if (LogStream != null)
            {
                if (args != null && args.Length > 0)
                {
                    if (severity == Severity.Verbose || SeverityFilter == Severity.Verbose)
                        fmt = string.Format(fmt, args);
                    else
                    {
                        //When not in verbose mode, do not include an exception call stack.
                        for (int i = 0; i < args.Length; i++)
                        {
                            if (args[i] is Exception) args[i] = ((Exception)args[i]).Message;
                        }
                        fmt = string.Format(fmt, args);
                    }
                }

                if (fmt.Contains('\n'))
                    fmt = fmt.Beautify(false, "    ").TrimStart(); //indent succeeding lines of message.

                lock (lockObj)
                {
                    if (severity != Severity.None) LogStream.Write(severity.ToString() + ": ");
                    LogStream.WriteLine(fmt);
                    LogStream.BaseStream.Flush();
                    MessageCapture?.Invoke(severity, fmt);
                }
            }
        }

        /// <summary>
        /// Close the log file. Equivalant to Log.Write(0,null). Log will automatically be reopened if writing a message again.
        /// </summary>
        public static void Dispose() => Write(0, null);
    }

    /// <summary>
    /// Alternate debugging log. For developer use only.
    /// </summary>
    public static class DebugLog
    {
        private static readonly string _logName = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Debug.log");
        public static string LogName { get { return _logName; } }
        private static StreamWriter LogStream = null;
        private static readonly object lockObj = new object();

        [Conditional("DEBUG")]
        public static void Write(string fmt, params object[] args)
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

            if (LogStream != null)
            {
                lock (lockObj)
                {
                    if (args != null && args.Length > 0)
                        fmt = string.Format(fmt, args);

                    LogStream.WriteLine(fmt);
                    LogStream.BaseStream.Flush();
                }
            }
        }

        [Conditional("DEBUG")]
        public static void Dispose() => Write(null);
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
        [DllImport("Kernel32.dll")] private static extern void OutputDebugString(string errmsg);

        [Conditional("DEBUG")]
        public static void WriteLine(string msg, params object[] args)
        {
            if (args != null && args.Length > 0) msg = string.Format(msg, args);
            if (msg[msg.Length - 1] != '\n') msg += Environment.NewLine;
            //Prefix diagnostic message with something unique that can be filtered upon by DebugView.exe
            _rawWrite("DEBUG: " + msg);
        }
    }
}
