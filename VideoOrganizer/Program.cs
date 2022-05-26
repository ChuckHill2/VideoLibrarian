//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Program.cs" company="Chuck Hill">
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
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using VideoLibrarian;

namespace VideoOrganizer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ProcessEx.AllowOnlyOneInstance();
            EmbeddedAssemblyResolver.SetResolver(); //Required for embedded assemblies in VideoLibrarian.exe assembly.
            Log.SeverityFilter = Severity.Verbose;
            Downloader.LogWriter = (severity, msg) => Log.Write(severity, msg);

            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());

            RegexCache.CompileToAssembly();
            Log.Dispose(); //closes the logger.
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Log.Write(Severity.Error, "Unhandled Exception: {0}", e.ExceptionObject.ToString());
                MessageBox.Show(e.ExceptionObject.ToString(), "VideoOrganizer - Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //EventLog.WriteEntry("VideoLibrarian", "VideoOrganizer - Unhandled Exception\r\n" + e.ExceptionObject.ToString(), EventLogEntryType.Error);
            }
            catch { }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            try
            {
                Log.Write(Severity.Error, "Unhandled Thread Exception: {0}", e.Exception.ToString());
                MessageBox.Show(e.Exception.ToString(), "VideoOrganizer - Unhandled Thread Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //EventLog.WriteEntry("VideoLibrarian", "VideoOrganizer - Unhandled Thread Exception\r\n" + e.Exception.ToString(), EventLogEntryType.Error);
            }
            catch { }
        }
    }
}
