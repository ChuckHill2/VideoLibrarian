using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MovieGuide
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

            return dtMin;
        }

        /// <summary>
        /// Get html file content as string without unecessary whitespace,
        /// no newlines and replace all double-quotes with single-quotes.
        /// Sometimes html quoting is done with single-quotes and sometimes with double-quotes.
        /// These fixups are all legal html and also make it easier to parse with Regex.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string ReadHtml(string filename)
        {
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
                return sb.ToString();
            }
        }

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
            const string UserAgent = @"Mozilla/5.0 (Windows NT 6.1; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0"; //DO NOT include "User-Agent: " prefix!
            Func<WebClient, string> ResponseUrl = web => ((HttpWebResponse)typeof(WebClient).GetField("m_WebResponse", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(web)).ResponseUri.AbsoluteUri;
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

                using (var web = new WebClient())
                {
                    web.Headers[HttpRequestHeader.UserAgent] = UserAgent;
                    if (!data.Referer.IsNullOrEmpty()) web.Headers[HttpRequestHeader.Referer] = data.Referer;
                    if (!data.Cookie.IsNullOrEmpty()) web.Headers[HttpRequestHeader.Cookie] = data.Cookie;
                    data.Filename = FileEx.GetUniqueFilename(data.Filename); //creates empty file as placeholder
                    //Diagnostics.WriteLine("{0} ==> {1}\r\n", data.Url, Path.GetFileName(data.Filename));

                    web.DownloadFile(data.Url, data.Filename);

                    data.Url = ResponseUrl(web); //update url to what the Web server thinks it is.
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

                if (!ext.EqualsI(Path.GetExtension(data.Filename)))
                {
                    var newfilename = Path.ChangeExtension(data.Filename, ext);
                    newfilename = FileEx.GetUniqueFilename(newfilename); //creates empty file as placeholder
                    File.Delete(newfilename); //delete the placeholder. Move will throw exception if it already exists
                    File.Move(data.Filename, newfilename);
                    data.Filename = newfilename;
                }

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
                    Log.Write("Error: {0} ==> {1}: {2}", data.Url, Path.GetFileName(data.Filename), ex.Message);
                    return false;
                }

                if (status == WebExceptionStatus.NameResolutionFailure || status == WebExceptionStatus.ConnectFailure)
                {
                    if (MiniMessageBox.Show(null, "Network Connection Dropped.", "Name Resolution Failure", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                    {
                        return false;
                    }
                }

                Log.Write("Retry #{0}: {1} ==> {2}: {3}", data.Retries, data.Url, Path.GetFileName(data.Filename), ex.Message);
                return Download(data);
                #endregion Log Error and Maybe Retry Download
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
                    //We don't use previously cached cookies by host in MovieGuide.
                    //if (!url.IsNullOrEmpty()) Cookie = MovieGuide.Cookie.GetCached(new Uri(url).Host);
                }

                Url = url;
                Filename = filename;
            }
        }
    }
}
