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
        private static HashSet<string> ResolvedHosts = null;                   //used exclusively by FileEx.Download()
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

        private static MD5CryptoServiceProvider __MD5CryptoServiceProvider = null;
        private static MD5CryptoServiceProvider MD5CryptoServiceProvider
        {
            get
            {
                if (__MD5CryptoServiceProvider == null)
                {
                    if (FIPSCompliance) FIPSCompliance = false;
                    __MD5CryptoServiceProvider = new MD5CryptoServiceProvider();
                }
                return __MD5CryptoServiceProvider;
            }
        }

        /// <summary>
        /// Compute unique MD5 hash of file contents.
        /// DO NOT USE for security encryption.
        /// </summary>
        /// <param name="filename">File content to generate hash from.</param>
        /// <returns>Guid</returns>
        public static Guid GetHash(string filename)
        {
            if (filename.IsNullOrEmpty()) return Guid.Empty;
            try
            {
                using (var fs = new FileStream(filename, FileMode.Open, System.Security.AccessControl.FileSystemRights.Read, FileShare.ReadWrite, 1024 * 1024, FileOptions.SequentialScan))
                {
                    return new Guid(MD5CryptoServiceProvider.ComputeHash(fs));
                }
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Get or set FIPS compliance flag.
        /// A hacky way to allow non-FIPS compliant algorthms to run.
        /// Non-FIPS compliant algorthims are:
        ///     MD5CryptoServiceProvider,
        ///     RC2CryptoServiceProvider,
        ///     RijndaelManaged,
        ///     RIPEMD160Managed,
        ///     SHA1Managed,
        ///     SHA256Managed,
        ///     SHA384Managed,
        ///     SHA512Managed,
        ///     AesManaged,
        ///     MD5Cng. 
        /// In particular, enables use of fast MD5 hash to create unique identifiers for internal use.
        /// </summary>
        private static bool FIPSCompliance
        {
            get { return CryptoConfig.AllowOnlyFipsAlgorithms; }
            set
            {
                FieldInfo fi;
                fi = typeof(CryptoConfig).GetField("s_fipsAlgorithmPolicy", BindingFlags.Static | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(null, value);
                fi = typeof(CryptoConfig).GetField("s_haveFipsAlgorithmPolicy", BindingFlags.Static | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(null, true);
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
        /// Get html file content as string without unecessary whitespace,
        /// no newlines and replace all double-quotes with single-quotes.
        /// Sometimes html quoting is done with single-quotes and sometimes with double-quotes.
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
        /// Due to network or server glitches or delays, this will try 3 times before giving up.
        /// Will not throw an exception. Errors are written to Log.Write().
        /// </summary>
        /// <param name="data">Job to download (url and suggested destination filename)</param>
        /// <returns>True if successfully downloaded</returns>
        public static bool Download(Job data)
        {
            #region Initialize Static Variables
            const string UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0"; //DO NOT include "User-Agent: " prefix!
            Func<string, string, string> GetDefaultExtension = (mimeType, defalt) =>
            {
                if (mimeType.IsNullOrEmpty()) return defalt;
                mimeType = mimeType.Split(';')[0].Trim();
                try { return Registry.GetValue(@"HKEY_CLASSES_ROOT\MIME\Database\Content Type\" + mimeType, "Extension", string.Empty).ToString(); }
                catch { }
                return defalt;
            };

            if (ResolvedHosts == null)
            {
                ResolvedHosts = new HashSet<string>(); //for name resolution or connection failure to determine if we should retry the download
                //Fix for exception: The request was aborted: Could not create SSL/TLS secure channel
                //https://stackoverflow.com/questions/10822509/the-request-was-aborted-could-not-create-ssl-tls-secure-channel
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; //Skip validation of SSL/TLS certificate
            }
            #endregion

            Uri uri = new Uri(data.Url);
            data.Retries++;
            int t1 = Environment.TickCount;

            try
            {
                string ext = Path.GetExtension(data.Filename);
                string mimetype = null;
                DateTime lastModified;

                //HACK: Empirically required for httpś://m.media-amazon.com/images/ poster images.
                if (uri.Host.EndsWith("amazon.com", StringComparison.OrdinalIgnoreCase))
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                else
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

                using (var web = new MyWebClient())
                {
                    web.Headers[HttpRequestHeader.UserAgent] = UserAgent;
                    if (!data.Referer.IsNullOrEmpty()) web.Headers[HttpRequestHeader.Referer] = data.Referer;
                    if (!data.Cookie.IsNullOrEmpty()) web.Headers[HttpRequestHeader.Cookie] = data.Cookie;
                    data.Filename = FileEx.GetUniqueFilename(data.Filename); //creates empty file as placeholder
                    //Diagnostics.WriteLine($"{data.Url} ==> {Path.GetFileName(data.Filename)}\r\n");

                    web.DownloadFile(data.Url, data.Filename);

                    data.Url = web.ResponseUrl; //update url to what the Web server thinks it is.
                    string cookie = web.ResponseHeaders[HttpResponseHeader.SetCookie];
                    if (!cookie.IsNullOrEmpty()) data.Cookie = cookie;
                    if (!DateTime.TryParse(web.ResponseHeaders[HttpResponseHeader.LastModified] ?? string.Empty, out lastModified)) lastModified = DateTime.Now;
                    mimetype = web.ResponseHeaders[HttpResponseHeader.ContentType];
                }
                ResolvedHosts.Add(uri.Host);  //for NameResolutionFailure handler statement.
                if (!File.Exists(data.Filename)) throw new FileNotFoundException("File missing or truncated.");

                //if (data.Retries < 1) return true; //do not validate. we want this file, always.

                if (new FileInfo(data.Filename).Length < 8) { File.Delete(data.Filename); return false; }

                //Interlocked.Increment(ref mediaDownloaded);  //mediaDownloaded++;
                File.SetCreationTime(data.Filename, lastModified);
                File.SetLastAccessTime(data.Filename, lastModified);
                File.SetLastWriteTime(data.Filename, lastModified);

                ext = GetDefaultExtension(mimetype, ext);
                if (ext == ".html") ext = ".htm";
                if (ext == ".jfif") ext = ".jpg";

                if (!ext.EqualsI(Path.GetExtension(data.Filename)))
                {
                    var newfilename = Path.ChangeExtension(data.Filename, ext);
                    newfilename = FileEx.GetUniqueFilename(newfilename); //creates empty file as placeholder
                    File.Delete(newfilename); //delete the placeholder. Move will throw exception if it already exists
                    File.Move(data.Filename, newfilename);
                    data.Filename = newfilename;
                }

                Log.Write(Severity.Verbose, $"Download {data.Url} duration={((Environment.TickCount - t1) / 1000f):F2} sec");
                return true;
            }
            catch (Exception ex)
            {
                File.Delete(data.Filename);
                if (ex is ThreadAbortException) return false;

                #region Log Error and Maybe Retry Download
                HttpStatusCode responseStatus = (HttpStatusCode)0;
                WebExceptionStatus status = WebExceptionStatus.Success;
                if (ex is WebException)
                {
                    WebException we = (WebException)ex;
                    HttpWebResponse response = we.Response as System.Net.HttpWebResponse;
                    responseStatus = (response == null ? (HttpStatusCode)0 : response.StatusCode);
                    status = we.Status;
                }

                if ((data.Retries < 1 || data.Retries > 3) ||
                    responseStatus == HttpStatusCode.Forbidden || //403
                    responseStatus == HttpStatusCode.NotFound || //404
                    responseStatus == HttpStatusCode.Gone || //410
                    //responseStatus == HttpStatusCode.InternalServerError || //500
                    ((status == WebExceptionStatus.NameResolutionFailure || status == WebExceptionStatus.ConnectFailure) && !ResolvedHosts.Contains(uri.Host)) ||
                    ex.Message.Contains("URI formats are not supported"))
                {
                    Log.Write(Severity.Error, $"duration={((Environment.TickCount - t1) / 1000f):F2} sec, {data.Url} ==> {Path.GetFileName(data.Filename)}: {ex.Message}");
                    return false;
                }

                if (status == WebExceptionStatus.NameResolutionFailure || status == WebExceptionStatus.ConnectFailure)
                {
                    if (MiniMessageBox.Show(null, "Network Connection Dropped.", "Name Resolution Failure", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                    {
                        return false;
                    }
                }

                Log.Write(Severity.Warning, $"Retry #{data.Retries}: duration={((Environment.TickCount-t1)/1000f):F2} sec, {data.Url} ==> {Path.GetFileName(data.Filename)}: {ex.Message}");
                return Download(data);
                #endregion Log Error and Maybe Retry Download
            }
        }

        private class MyWebClient : WebClient
        {
            public WebRequest Request { get; private set; }
            public WebResponse Response { get; private set; }
            public string ResponseUrl => this.Response?.ResponseUri?.AbsoluteUri;

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                Request = request;
                Response = base.GetWebResponse(request);
                return Response;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                Request = base.GetWebRequest(address);
                HttpWebRequest request = Request as HttpWebRequest;
                //Allow this API to decompress output.
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }

        /// <summary>
        /// Info to pass to FileEx.Downloader.
        /// </summary>
        public class Job
        {
            /// <summary>
            /// Download retry count (min value=1, max value=3). 
            /// Do not modify. For internal use only by FileEx.Downloader().
            /// </summary>
            public int Retries = -1;

            /// <summary>
            /// Previous job url. Now the referrer to this new job. 
            /// Do not modify. For internal use only by FileEx.Downloader().
            /// </summary>
            public string Referer { get; set; }

            /// <summary>
            /// Previous job generated cookie. Now forwarded to this new job. 
            /// Do not modify. For internal use only by FileEx.Downloader().
            /// </summary>
            public string Cookie { get; set; }

            /// <summary>
            /// Absolute url path to download
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            ///   Full path name of file to write result to.
            ///   If file extension does not match the downloaded mimetype, the file extension is updated to match the mimetype.
            ///   If the file previously exists, the file name is incremented (e.g 'name(nn).ext')
            ///   This field is updated with the new name.
            /// </summary>
            public string Filename { get; set; }

            /// <summary>
            /// Info to pass to FileEx.Downloader.
            /// </summary>
            /// <param name="job">Parent job info to use as the referrer. Null if no parent.</param>
            /// <param name="url">Url to download</param>
            /// <param name="filename">
            ///   Full path name of file to write result to.
            ///   If file extension does not match the downloaded mimetype, the file extension is updated to match the mimetype.
            ///   If the file exists, the file name is incremented (e.g 'name(nn).ext')
            ///   This field is updated with the new name.
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
        }
    }
}
