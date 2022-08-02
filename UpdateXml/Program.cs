using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrarian;

namespace UpdateXml
{
    public enum Command { Help, Update, Restore }

    public static class Program
    {
        private static readonly bool isDoubleClicked = (Console.CursorLeft == 0 && Console.CursorTop == 0); //started from WindowsExplorer?
        private static string[] MediaFolders; //folders to enumerate for *.url and associated video files.
        private static Command Command;

        static void Main(string[] args)
        {
#if !DEBUG
            //If starting directly from Windows Explorer (e.g. double-click), don't run without an explicit prompt.
            //This app may start things the caller is not ready to do. So just show help and then prompt to continue.
            if (isDoubleClicked)
            {
                AboutMsg();
                Console.WriteLine();
                Console.Write("UpdateXml was started from Windows Explorer. Do you want to continue? [Yes] ");
                var answer = (int)Console.ReadKey().Key;
                if (!(answer == 'Y' || answer == 'y' || answer == '\r')) Environment.Exit(1);
            }
#endif
            ProcessEx.AllowOnlyOneInstance();
            EmbeddedAssemblyResolver.SetResolver(); //Required for embedded assemblies in VideoLibrarian.exe assembly.
            Log.SeverityFilter = Severity.Verbose;
            Log.MessageCapture += (Severity sev, string msg) => Console.WriteLine($"{sev}: {msg}");
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

            if (Command == Command.Restore) DoRestore();
            else if (Command == Command.Update) DoUpdate();

            RegexCache.CompileToAssembly();
            Log.Dispose(); //closes the logger.
        }

        /// <summary>
        /// Update/refresh the movie properties xml with the latest changes on IMDB.
        /// Things like ratings and descriptions are the most likely to change.
        /// </summary>
        private static void DoUpdate()
        {
            foreach (var mf in MediaFolders) // Copied (mostly) from VideoLibrarian.FormMain.LoadMovieInfo()
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
                                //Find all titleId xml files in this directory. Hint, there should be only one.
                                var arr = DirectoryEx.EnumerateAllFiles(folder).Where(m => RegexCache.RegEx(@"\\tt[0-9]{6,9}\.xml$", RegexOptions.IgnoreCase).IsMatch(m)).ToList();
                                if (arr.Count == 0)  throw new InvalidDataException($"Missing movie property XML file for folder: {folder}");
                                if (arr.Count > 1) throw new InvalidDataException($"Multiple movie property XML files for folder: {folder}");
                                xmlFile = arr[0];
                                if (Path.GetFileNameWithoutExtension(xmlFile) == MovieProperties.EmptyTitleID) return; //Custom non-IMDB pseudo title ID for manually generated MovieProperties.

                                var p1 = new MovieProperties(folder); //load original properties

                                FileEx.Delete(xmlFile + ".backup");
                                FileEx.Move(xmlFile, xmlFile + ".backup"); //backup/save just in case

                                //create upon demand new movie properties file.
                                var p2 = new MovieProperties(folder); //with xml file moved out of the way, scrape new movie properties,

                                //Copy manual properties that are not web scraped.
                                if (!p1.MoviePosterUrl.IsNullOrEmpty())  //Note: Save the orginal poster image url, if it exists. It may have been manually updated by user.
                                    p2.MoviePosterUrl = p1.MoviePosterUrl;
                                p2.CustomGroups = p1.CustomGroups;
                                p2.Watched = p1.Watched;

                                //Because IMDB may change the title ID (rarely), the backup file name may no longer match the title ID of the of the new xml file so we have to get it the hard way.
                                var backup = DirectoryEx.EnumerateAllFiles(folder).FirstOrDefault(m=> RegexCache.RegEx(@"\\tt[0-9]{6,9}\.xml.backup$", RegexOptions.IgnoreCase).IsMatch(m));

                                //If the 2 movie properies objects are different, save the new one, delete the old one and delete the cached tile images.
                                if (!p1.Equals(p2))
                                {
                                    p2.Serialize();
                                    if (!backup.IsNullOrEmpty()) FileEx.Delete(backup);
                                    var re = RegexCache.RegEx(@"\\tt[0-9]{6,9}\.png$", RegexOptions.IgnoreCase); //ex. tt0000000S.png, tt0000000M.png, tt0000000L.png
                                    foreach (var f in DirectoryEx.EnumerateAllFiles(folder).Where(m => re.IsMatch(m))) FileEx.Delete(f);
                                    //We do not delete the poster image because it may have been manually modified. User can manually delete the poster image if they really want to re-download it.
                                }
                                else //They're not different, so ignore the (not saved) new one and restore the old one.
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
                                //Something bad happened, so restore the backup if it exists..

                                var backup = DirectoryEx.EnumerateAllFiles(folder).FirstOrDefault(m => RegexCache.RegEx(@"\\tt[0-9]{6,9}\.xml.backup$", RegexOptions.IgnoreCase).IsMatch(m));
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
        }

        /// <summary>
        /// If the Update is '^C' aborted, a number of movie properties xml files may have been renamed to .backup
        /// with incomplete download of new unsaved scraped properties. This will restore the backup xml files.
        /// </summary>
        private static void DoRestore()
        {

            foreach (var mf in MediaFolders) // Copied (mostly) from VideoLibrarian.FormMain.LoadMovieInfo()
            {
                if (!Directory.Exists(mf))
                {
                    Log.Write(Severity.Warning, $"Media folder {mf} does not exist.");
                    continue;
                }

                Console.WriteLine($"[Enumerating Movie Folders in {mf}]");
                int added = 0;

                var options = new ParallelOptions(); //{ MaxDegreeOfParallelism = 50 };

                Parallel.ForEach(DirectoryEx.EnumerateAllFiles(mf, SearchOption.AllDirectories)
                        .Where(m => m.EndsWith(".url", StringComparison.OrdinalIgnoreCase))
                        .Select(m => Path.GetDirectoryName(m))
                        .Where(m => m != mf & !MovieProperties.IgnoreFolder(m))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(x => x),
                        options,
                        folder =>
                        {
                            var arr = DirectoryEx.EnumerateAllFiles(mf).Where(m => RegexCache.RegEx(@"\tt[0-9]{6,9}\.xml.backup$", RegexOptions.IgnoreCase).IsMatch(m)).ToList();
                            if (arr.Count != 1) return;
                            var backup = arr[0];
                            var xml = backup.Substring(0, backup.Length - 7);
                            FileEx.Delete(xml);
                            FileEx.Move(backup, xml);
                        });

                Log.Write(Severity.Info, $"{added} movie properties scanned from {mf}");
            }
        }

        private static void ParseCommandLine(string[] args)
        {
            var dirs = new List<string>();
            Command = Command.Update;

            foreach (var arg in args)
            {
                if (arg[0] == '-' || arg[0] == '/')
                {
                    var option = arg.Substring(1).ToLower();
                    if (option == "?" || option == "h")
                    {
                        Command = Command.Help;
                        AboutMsg();
                        Environment.Exit(1);
                    }
                    else if (option == "r")
                        Command = Command.Restore;
                    else
                    {
                        Console.WriteLine($"\r\nInvalid command-line switch \"{arg}\"");
                        AboutMsg();
                        Environment.Exit(1);
                    }
                }

                if (Directory.Exists(arg)) dirs.Add(arg);
            }

            if (dirs.Count > 0) MediaFolders = dirs.ToArray();
        }

        private static void AboutMsg()
        {
            Console.WriteLine(@"
Update/refresh the VideoLibrarian movie properties XML file with latest info from IMDB.
Movie poster images and Watched flags are not changed.

Usage: UpdateXml.exe [switches] [[mediafolder1] [mediafolder2] [mediafolderN]...]
         /? - this help
         /h - this help
         /r - restore tt*.xml.backup from previously aborted xml update.
mediafolder - A list of VideoLibrarian media folders. If undefined, reads the
              list of media folders from VideoLibrarian's saved state file.

This executable must reside in the same folder as VideoLibrarian.exe.

Results are displayed in both the console window and UpdateXml.log.

UpdateXml may take a very long time to complete depending how many video
files there are (6hrs or more for >2000 videos).
");
        }
    }
}
