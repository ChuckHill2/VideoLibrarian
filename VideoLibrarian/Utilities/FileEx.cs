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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace VideoLibrarian
{
    public static class FileEx
    {
        private static readonly Object GetUniqueFilename_Lock = new Object();  //used exclusively by FileEx.GetUniqueFilename()

        /// <summary>
        /// Make sure specified file does not exist. If it does, add or increment
        /// version. Then create an empty file placeholder so it won't get usurped
        /// by another thread calling this function. Versioned file format:
        /// d:\dir\name(00).ext where '00' is incremented until one is not found.
        /// </summary>
        /// <param name="srcFilename"></param>
        /// <returns></returns>
        private static string GetUniqueFilename(string srcFilename) //find an unused filename
        {
            string pathFormat = null;
            string newFilename = srcFilename;
            int index = 1;

            lock (GetUniqueFilename_Lock)
            {
                string dir = Path.GetDirectoryName(srcFilename);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                while (File.Exists(newFilename))
                {
                    if (pathFormat == null)
                    {
                        string path = Path.Combine(dir, Path.GetFileNameWithoutExtension(srcFilename));
                        if (path[path.Length - 1] == ')')
                        {
                            int i = path.LastIndexOf('(');
                            if (i > 0) path = path.Substring(0, i);
                        }
                        pathFormat = path + "({0:00})" + Path.GetExtension(srcFilename);
                    }
                    newFilename = string.Format(pathFormat, index++);
                }

                File.Create(newFilename).Dispose();  //create place-holder file.
            }

            return newFilename;
        }

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
            var dtMin = File.GetCreationTime(filename);

            var dt = File.GetLastAccessTime(filename);
            if (dt < dtMin) dtMin = dt;

            dt = File.GetLastWriteTime(filename);
            if (dt < dtMin) dtMin = dt;

            //Forget hi-precision and DateTimeKind. It just complicates comparisons. This is more than good enough.
            return new DateTime(dtMin.Year, dtMin.Month, dtMin.Day, dtMin.Hour, dtMin.Minute, 0);
        }

        /// <summary>
        /// Safe file delete. Will not throw exception. Instead will log it as a warning.
        /// </summary>
        /// <param name="fn">Name of file to delete.</param>
        /// <returns>True if successfully deleted</returns>
        public static bool FileDelete(string fn)
        {
            try
            {
                File.Delete(fn);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(Severity.Warning, $"Could not delete {fn}: {ex.Message}");
                return false;
            }
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
                results = reNoScript.Replace(results, string.Empty);
            }

            return results;
        }
        private static readonly Regex reNoScript = new Regex(@"(<script.+?</script>)|(<style.+?</style>)|(<svg.+?</svg>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Download a URL output into a local file.
        /// </summary>
        /// <param name="data">Job to download (url and suggested destination filename, plus more)</param>
        /// <returns>True if successfully downloaded</returns>
        /// <remarks>
        /// Thread-safe<br/>
        /// Will not throw an exception. Errors are written to Log.Write().<br/>
        /// Upon successful download:<br/>
        /// • File date is set to the object date on the server.<br/>
        /// • File extension is updated to reflect the server mime type.<br/>
        /// • If file already exists, file version is incremented. (e.g. fileame(ver).ext)<br/>
        /// • Job.Cookie is assigned if empty.<br/>
        /// • Job.Filename is always updated to the new filename.<br/>
        /// • Job.Url is always updated to the actual url used by the server (redirect?)
        /// </remarks>
        public static bool Download(Job data)
        {
            //This string is apparently not stored anywhere. It must be retrieved as a response from a web service via the browser of choice. It cannot be retrieved offline! Arrgh! Google: what is my useragent
            const string UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:96.0) Gecko/20100101 Firefox/96.0";

            Uri uri = null;
            int t1 = Environment.TickCount;

            try
            {
                uri = new Uri(data.Url);

                string ext = Path.GetExtension(data.Filename);
                string mimetype = null;
                DateTime lastModified;

                //Fix for exception: The request was aborted: Could not create SSL/TLS secure channel
                //https://stackoverflow.com/questions/10822509/the-request-was-aborted-could-not-create-ssl-tls-secure-channel
                //if (ServicePointManager.ServerCertificateValidationCallback==null)
                //    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; //Skip validation of SSL/TLS certificate

                using (var web = new MyWebClient())
                {
                    web.Headers[HttpRequestHeader.UserAgent] = UserAgent;
                    if (!data.Referer.IsNullOrEmpty()) web.Headers[HttpRequestHeader.Referer] = data.Referer;
                    if (!data.Cookie.IsNullOrEmpty()) web.Headers[HttpRequestHeader.Cookie] = data.Cookie;
                    web.Headers[HttpRequestHeader.AcceptLanguage] = "en-US,en;q=0.5"; //always set language to english so our web scraper only has to deal with only a single language.
                    data.Filename = FileEx.GetUniqueFilename(data.Filename); //creates empty file as placeholder

                    web.DownloadFile(data.Url, data.Filename); //This will throw an exception if the url cannot be downloaded.

                    data.Url = web.ResponseUrl; //update url to what the Web server thinks it is.
                    string cookie = web.ResponseHeaders[HttpResponseHeader.SetCookie];
                    if (!cookie.IsNullOrEmpty()) data.Cookie = cookie;
                    if (!DateTime.TryParse(web.ResponseHeaders[HttpResponseHeader.LastModified] ?? string.Empty, out lastModified)) lastModified = DateTime.Now;
                    mimetype = web.ResponseHeaders[HttpResponseHeader.ContentType];
                }
                if (!File.Exists(data.Filename)) throw new FileNotFoundException();

                if (new FileInfo(data.Filename).Length < 8) { File.Delete(data.Filename); throw new FileNotFoundException("File truncated."); }

                File.SetCreationTime(data.Filename, lastModified);
                File.SetLastAccessTime(data.Filename, lastModified);
                File.SetLastWriteTime(data.Filename, lastModified);

                //Adjust extension to reflect true filetype, BUT make sure that new filename does not exist.
                ext = GetDefaultExtension(mimetype, ext);
                if (!ext.EqualsI(Path.GetExtension(data.Filename)))
                {
                    var newfilename = Path.ChangeExtension(data.Filename, ext);
                    newfilename = FileEx.GetUniqueFilename(newfilename); //creates empty file as placeholder
                    File.Delete(newfilename); //delete the placeholder. Move will throw exception if it already exists
                    File.Move(data.Filename, newfilename);
                    data.Filename = newfilename; //return new filename to caller.
                }

                Log.Write(Severity.Verbose, $"Download {data.Url} duration={((Environment.TickCount - t1) / 1000f):F2} sec");
                return true;
            }
            catch (Exception ex)
            {
                File.Delete(data.Filename);

                HttpStatusCode responseStatus = (HttpStatusCode)0;
                WebExceptionStatus status = WebExceptionStatus.Success;
                if (ex is WebException)
                {
                    WebException we = (WebException)ex;
                    HttpWebResponse response = we.Response as System.Net.HttpWebResponse;
                    responseStatus = (response == null ? (HttpStatusCode)0 : response.StatusCode);
                    status = we.Status;
                }

                //Occurs when the web server is flooded with our requests due to multi-threading.
                if (status== WebExceptionStatus.RequestCanceled || status == WebExceptionStatus.Timeout || status == WebExceptionStatus.UnknownError)
                {
                    if (data.RetryCount < 6)
                    {
                        data.RetryCount++;
                        Log.Write(Severity.Warning, $"{status}: Download Retry {data.RetryCount} {data.Url} ==> {Path.GetDirectoryName(data.Filename)}");
                        Thread.Sleep(2000);
                        if (Download(data)) return true;
                    }
                }

                var eStatusMsg = DownloadStatus(status, responseStatus); //logically combines both web and http status's into a single string.

                //Occurs when the IMDB url has changed or is no longer valid. Requires user to manually update the IMDB shortcut.
                if (responseStatus == (HttpStatusCode)308) //The remote server returned an error: (308) Permanent Redirect.
                {
                    Log.Write(Severity.Error, $"{eStatusMsg} {data.Url} ==> {Path.GetDirectoryName(data.Filename)}: {ex.GetType().Name}:{ex.Message}\nShortcut may be corrupted!");
                    return false;
                }

                Log.Write(Severity.Error, $"{eStatusMsg} {data.Url} ==> {Path.GetDirectoryName(data.Filename)}: {ex.GetType().Name}:{ex.Message}");
                return false;
            }
        }

        private static string DownloadStatus(WebExceptionStatus status, HttpStatusCode responseStatus)
        {
            var sb = new StringBuilder();
            if (status != 0) sb.Append(status);
            if (responseStatus != 0)
            {
                if (sb.Length > 0) sb.Append("/");
                sb.Append($"{responseStatus}({(int)responseStatus})");
            }

            return sb.ToString();
        }

        private static string GetDefaultExtension(string mimeType, string defalt) //used exclusively by Download()
        {
            if (mimeType.IsNullOrEmpty()) return defalt;
            mimeType = mimeType.Split(';')[0].Trim();
            string ext;
            try { ext = Registry.GetValue(@"HKEY_CLASSES_ROOT\MIME\Database\Content Type\" + mimeType, "Extension", string.Empty).ToString(); }
            catch { ext = defalt; }

            if (ext == ".html") ext = ".htm";  //Override registry mimetypes. We like the legacy extensions.
            if (ext == ".jfif") ext = ".jpg";

            return ext;
        }

        private class MyWebClient : WebClient
        {
            public WebRequest Request { get; private set; }
            public WebResponse Response { get; private set; }
            public string ResponseUrl => this.Response?.ResponseUri?.AbsoluteUri; //gets the URI of the Internet resource that actually responded to the request.

            protected override WebResponse GetWebResponse(WebRequest request) //used internally
            {
                Request = request;
                Response = base.GetWebResponse(request);
                return Response;
            }

            protected override WebRequest GetWebRequest(Uri address) //used internally
            {
                Request = base.GetWebRequest(address);
                HttpWebRequest request = Request as HttpWebRequest; //there are others: e.g. FtpWebRequest (ftp://) and FileWebRequest (file://).
                
                if (request != null)
                {
                    request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;  //Allow this API to decompress http output.
                    request.AllowAutoRedirect = true;
                }

                return Request;
            }
        }

        /// <summary>
        /// Info to pass to FileEx.Download(). More properties may be added in the future.
        /// </summary>
        public class Job
        {
            /// <summary>
            /// For Internal use by FileEx.Download()
            /// </summary>
            public int RetryCount = 0;

            /// <summary>
            /// Previous job url. Now the referrer to this new job. 
            /// </summary>
            public string Referer { get; set; }

            /// <summary>
            /// Previously generated cookie. Now forwarded to this new job. 
            /// </summary>
            public string Cookie { get; set; }

            /// <summary>
            /// Absolute url path to download. Updated with the URI of the Internet resource that actually responded to the request.
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            ///   Full path name of file to write result to.
            ///   If file extension does not match the downloaded mimetype, the file extension is updated to match the mimetype.
            ///   If the file already exists, the file name is incremented (e.g 'name(nn).ext')
            ///   This property is updated with the new name.
            /// </summary>
            public string Filename { get; set; }

            /// <summary>
            /// Info to pass to FileEx.Download().
            /// </summary>
            /// <param name="job">Parent job info to use as the referrer. Null if no parent.</param>
            /// <param name="url">Url to download</param>
            /// <param name="filename">
            ///   Full path name of file to write result to.
            ///   If file extension does not match the downloaded mimetype, the file extension is updated to match the mimetype.
            ///   If the file already exists, the file name is incremented (e.g 'name(nn).ext')
            ///   This property is updated with the new name.
            /// </param>
            public Job(Job job, string url, string filename)
            {
                if (job != null)
                {
                    Cookie = job.Cookie;
                    Referer = job.Url;
                }
                else
                {
                    //We don't use previously cached cookies by host in VideoLibrarian.
                    //if (!url.IsNullOrEmpty()) Cookie = VideoLibrarian.Cookie.GetCached(new Uri(url).Host);
                }

                Url = url;
                Filename = filename;
            }

            /// <summary>
            /// Info to pass to FileEx.Download().
            /// </summary>
            /// <param name="url">Url to download</param>
            /// <param name="filename">
            ///   Full path name of file to write result to.
            ///   If file extension does not match the downloaded mimetype, the file extension is updated to match the mimetype.
            ///   If the file already exists, the file name is incremented (e.g 'name(nn).ext')
            ///   This property is updated with the new name.
            /// </param>
            /// <param name="referer">Optional download referer. Simulates reference to a previous call.</param>
            /// <param name="cookie">Optional download cookie. A previously generated cookie.</param>
            public Job(string url, string filename, string referer = null, string cookie = null)
            {
                Referer = referer;
                Cookie = cookie;
                Url = url;
                Filename = filename;
            }
        }
    }
}
