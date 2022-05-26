//--------------------------------------------------------------------------
// <summary>
//   Entrypoint for console application.
//   Validate/verify the integrity of all the movie/videos under the VideoLibrarian.
//   Enumerate all the movies referenced by the VideoLibrarian and check for any posible file corruption.
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrarian;

namespace VideoValidator
{
    class Program
    {
        private static string[] MediaFolders; //folders to enumerate for *.url and associated video files.

        static void Main(string[] args)
        {
            ProcessEx.AllowOnlyOneInstance();
            EmbeddedAssemblyResolver.SetResolver(); //Required for embedded assemblies in VideoLibrarian.exe assembly.
            Log.MessageCapture += (Severity sev, string msg) => Console.WriteLine($"{sev}: {msg}");
            Log.SeverityFilter = Severity.Verbose;
            Downloader.LogWriter = (severity, msg) => Log.Write(severity, msg);
            ParseCommandLine(args);  //populate MediaFolders from the command-line


            if (MediaFolders == null) //try getting MediaFolders from matching VideoLibrarian.exe/VideoLibrarian.SavedState.xml
            {
                try
                {
                    string filename = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "VideoLibrarian.SavedState.xml");
                    if (!FileEx.Exists(filename))
                    {
                        //Hack: Visual Studio places this exe in a different folder from the official VideoLibrarian folder, so we use a shortcut to the official state file.
                        if (FileEx.Exists(filename + ".lnk")) //dereference the .lnk file
                        {
                            IWshRuntimeLibrary.WshShell wshShell = new IWshRuntimeLibrary.WshShell();
                            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(filename + ".lnk");
                            filename = shortcut.TargetPath;
                        }

                        if (!FileEx.Exists(filename)) throw new FileNotFoundException($"File {filename} not found.", filename);
                    }

                    FormMainProperties data = XmlIO.Deserialize<FormMainProperties>(filename);

                    MediaFolders = data.Settings.MediaFolders;
                }
                catch(Exception ex)
                {
                    Log.Write(Severity.Error, $"VideoLibrarian.SavedState.xml failed to load: {ex.Message}");
                    Log.Dispose();
                    Environment.Exit(1);
                }
            }

            if (MediaFolders==null || MediaFolders.Length==0)
            {
                Log.Write(Severity.Error, "No media folders were defined.");
                Log.Dispose();
                Environment.Exit(1);
            }

            Console.WriteLine($"Verifying {MediaFolders.Length} media folder{(MediaFolders.Length!=1?"s":"")}.");

            // Copied (mostly) from VideoLibrarian.FormMain.LoadMovieInfo()
            foreach (var mf in MediaFolders)
            {
                if (!Directory.Exists(mf))
                {
                    Log.Write(Severity.Error, $"Media folder {mf} does not exist.");
                    continue;
                }

                Console.WriteLine($"[Begin {mf} Verification]");
                int added = 0;
                Parallel.ForEach(DirectoryEx.EnumerateAllFiles(mf, SearchOption.AllDirectories)
                        .Where(m => m.EndsWith(".url", StringComparison.OrdinalIgnoreCase))
                        .Select(m => Path.GetDirectoryName(m))
                        .Where(m => m != mf & !MovieProperties.IgnoreFolder(m))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(x => x),
                        folder =>
                        {
                            try
                            {
                                Console.Write($"{added + 1}\b\b\b\b\b\b"); //Counter to show that we are actually busy working.

                                var p = new MovieProperties(folder, false);
                                if (p.ToString() == "UNKNOWN") //Incomplete/corrupted movie property. See log file.
                                    throw new InvalidDataException($"Incomplete/corrupted movie property for folder: {folder}");

                                p.VerifyVideoFile(); //don't need to do anything with the return value as VerifyVideoFile() already writes the messages to Log.

                                Interlocked.Increment(ref added);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(Severity.Error, $"Movie property failed to load from {folder}: {ex.Message}");
                            }
                        });

                Log.Write(Severity.Info, $"{added} movie properties scanned from {mf}");
            }

            RegexCache.CompileToAssembly();
            Log.Dispose(); //closes the logger.
            Console.WriteLine("[Verification Complete.]");
        }

        private static void ParseCommandLine(string[] args)
        {
            if (args.Length>0)
            {
                if (args[0][0]=='-' || args[0][0] == '/')
                {
                    //we have no other command-line switches...
                    //var arg = args[0].Substring(1).ToLower();
                    //if (arg == "?" || arg == "h" || arg == "help") ExitMsg();
                    ExitMsg();
                }

                var dirs = new List<string>();
                foreach(var arg in args)
                {
                    if (!Directory.Exists(arg)) continue;
                    dirs.Add(arg);
                }
                if (dirs.Count > 0) MediaFolders = dirs.ToArray();
            }
        }

        private static void ExitMsg()
        {
            Console.WriteLine(@"
Validate/verify that all the movie files in the specified folders are NOT corrupted.
Reason: Video corruption may occur if the external drive is disconnected
unexpectedly, windows crashed, or the drive itself is just slowly dying.

See low-level system console command: chkdsk.exe drive-letter: /F /R /X
This may need to be run first in order to verify/fix/repair the drive itself.

Usage: VideoValidator.exe [/?] [[mediafolder1] [mediafolder2] [mediafolderN]...]
         /? - this help
mediafolder - A list of VideoLibrarian media folders. If undefined, reads the
              list of media folders from VideoLibrarian's saved state file.

This executable must reside in the same folder as VideoLibrarian.exe.

Results are displayed in both the console window and VideoValidator.log.

VideoValidator may take a very long time to complete depending how many video
files there are (6hrs or more for >2000 videos). This is due to the necessity
of having to read the entire content of all the large video files.
");
            Environment.Exit(1);
        }
    }
}
