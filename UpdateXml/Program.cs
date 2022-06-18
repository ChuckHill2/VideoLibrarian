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

namespace UpdateXml
{
    class Program
    {
        private static string[] MediaFolders; //folders to enumerate for *.url and associated video files.

        static void Main(string[] args)
        {
            ProcessEx.AllowOnlyOneInstance();
            EmbeddedAssemblyResolver.SetResolver(); //Required for embedded assemblies in VideoLibrarian.exe assembly.
            Log.SeverityFilter = Severity.Verbose;
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

                    Log.SeverityFilter = data.Settings.LogSeverity;
                    MediaFolders = data.Settings.MediaFolders;
                }
                catch (Exception ex)
                {
                    Log.Write(Severity.Error, $"VideoLibrarian.SavedState.xml failed to load: {ex.Message}");
                    Log.Dispose();
                    Environment.Exit(1);
                }
            }

            if (MediaFolders == null || MediaFolders.Length == 0)
            {
                Log.Write(Severity.Error, "No media folders were defined.");
                Log.Dispose();
                Environment.Exit(1);
            }

            Console.WriteLine($"Verifying {MediaFolders.Length} media folder{(MediaFolders.Length != 1 ? "s" : "")}.");

            // Copied (mostly) from VideoLibrarian.FormMain.LoadMovieInfo()
            foreach (var mf in MediaFolders)
            {
                if (!Directory.Exists(mf))
                {
                    Log.Write(Severity.Warning, $"Media folder {mf} does not exist.");
                    continue;
                }

                Console.WriteLine($"[Enumerating Movie Folders in {mf}]");
                int added = 0;

                //Need to restrict max threads if there are too many downloads needed (aka new
                //MovieProperty xml files) because flooding the IMDB webserver will drop downloads.
                //Testing showed that the web server started failing after 500 concurrent downloads.
                var options = new ParallelOptions() { MaxDegreeOfParallelism = 50 };

                Parallel.ForEach(DirectoryEx.EnumerateAllFiles(mf, SearchOption.AllDirectories)
                        .Where(m => m.EndsWith(".url", StringComparison.OrdinalIgnoreCase))
                        .Select(m => Path.GetDirectoryName(m))
                        .Where(m => m != mf & !MovieProperties.IgnoreFolder(m))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(x => x),
                        options,
                        folder =>
                {
                    string xmlFile;
                    try
                    {
                        Console.Write($"{added + 1}\b\b\b\b\b\b"); //Counter to show that we are actually busy working.

                        var xmlArray = Directory.GetFiles(folder, "tt*.xml");
                        if (xmlArray.Length != 1) throw new InvalidDataException($"Missing or multiple movie property XML file(s) for folder: {folder}");
                        xmlFile = xmlArray[0];
                        if (Path.GetFileNameWithoutExtension(xmlFile) == MovieProperties.EmptyTitleID) return; //custom non-IMDB MovieProperties.

                        var p1 = new MovieProperties(folder);

                        FileEx.Delete(xmlFile + ".backup");
                        FileEx.Move(xmlFile, xmlFile + ".backup"); //save just in case

                        //create upon demand new movie properties file.
                        var p2 = new MovieProperties(folder);

                        //Save the orginal poster image url, if it exists. It may have been manually updated by user.
                        if (!p1.MoviePosterUrl.IsNullOrEmpty())
                        {
                            p2.MoviePosterUrl = p1.MoviePosterUrl;
                        }

                        if (p2.Watched != p1.Watched)
                        {
                            p2.Watched = p1.Watched;
                        }

                        //backup file name may have changed so get it the hard way.
                        var backup = Directory.EnumerateFiles(folder, "tt*.xml.backup").FirstOrDefault();

                        //if (!p1.Equals(p2)) //remove image cache if MovieProperties has changed
                        if (!p1.Equals(p2))
                        {
                            p2.Serialize();
                            if (!backup.IsNullOrEmpty()) FileEx.Delete(backup);
                            var re = RegexCache.RegEx(@"\tt[0-9]+(?:S|M|L).png$", RegexOptions.IgnoreCase); //ex. tt0000000S.png, tt0000000M.png, tt0000000L.png
                            foreach (var f in DirectoryEx.EnumerateAllFiles(folder).Where(m => re.IsMatch(m))) FileEx.Delete(f);
                            //We do not delete the poster image because it may have been manually modified.
                        }
                        else
                        {
                            if (!backup.IsNullOrEmpty())
                            {
                                var xml = backup.Substring(0, backup.Length - 7);
                                FileEx.Delete(xml);
                                FileEx.Move(backup, xml);
                            }
                        }

                        Interlocked.Increment(ref added);
                    }
                    catch (Exception ex)
                    {
                        var backup = Directory.EnumerateFiles(folder, "tt*.xml.backup").FirstOrDefault();
                        if (!backup.IsNullOrEmpty())
                        {
                            var xml = backup.Substring(0, backup.Length - 7);
                            FileEx.Delete(xml);
                            FileEx.Move(backup, xml);
                        }

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
            if (args.Length > 0)
            {
                if (args[0][0] == '-' || args[0][0] == '/')
                {
                    //we have no other command-line switches...
                    //var arg = args[0].Substring(1).ToLower();
                    //if (arg == "?" || arg == "h" || arg == "help") ExitMsg();
                    ExitMsg();
                }

                var dirs = new List<string>();
                foreach (var arg in args)
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
Update/refresh the VideoLibrarian movie properties XML file with latest info from IMDB.
Movie poster images and Watched flags are not changed.

Usage: UpdateXml.exe [/?] [[mediafolder1] [mediafolder2] [mediafolderN]...]
         /? - this help
mediafolder - A list of VideoLibrarian media folders. If undefined, reads the
              list of media folders from VideoLibrarian's saved state file.

This executable must reside in the same folder as VideoLibrarian.exe.

Results are displayed in both the console window and UpdateXml.log.

UpdateXml may take a very long time to complete depending how many video
files there are (6hrs or more for >2000 videos).
");
            Environment.Exit(1);
        }
    }
}
