using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace DiskCleanup
{
    public class CCleaner
    {
        #region Delegates

        public delegate void DownloadProgressCallback(long size, long bytesProcessed);

        public delegate void UnzipProgressCallback(string name, double percentComplete);

        #endregion

        #region Fields

        private const string DownloadUrl = "https://www.ccleaner.com/ccleaner/download/portable/downloadfile";
        private Action<DownloadProgressCallback, UnzipProgressCallback> _downloadDelegate;
        private Action _invocationDelegate;
        private readonly FileInfo _zip;
        private readonly FileInfo _exe;
        private static int _syncPoint;

        #endregion

        #region Properties

        public bool DownloadNeeded
        {
            get
            {
                _exe.Refresh();
                return !_exe.Exists;
            }
        }

        public bool RunAsAdministrator { get; set; }

        #endregion

        #region Ctor

        public CCleaner()
        {
            _zip = new FileInfo(Path.Combine(Path.GetTempPath(), "CCleanerPortable.zip"));
            _exe = new FileInfo(Path.Combine(_zip.Directory.FullName, $"CCleanerPortable\\{(IntPtr.Size == 8 ? "CCleaner64.exe" : "CCleaner.exe")}"));
        }

        #endregion

        #region Methods

        public void Download(DownloadProgressCallback downloadProgressCallback = default, UnzipProgressCallback unzipProgressCallback = default)
        {
            if (Interlocked.CompareExchange(ref _syncPoint, 1, 0) != 0)
                throw new InvalidOperationException("A download or cleanup operation is already in progress.");

            var webRequest = (HttpWebRequest) WebRequest.Create(DownloadUrl);

            try
            {
                using (var response = (HttpWebResponse) webRequest.GetResponse())
                using (var responseStream = response.GetResponseStream() ?? throw new DiskCleanupException($"Error downloading CCleaner from {DownloadUrl}."))
                using (var fileStream = _zip.OpenWrite())
                {
                    var downloaded = 0L;
                    var buffer = new byte[8192];
                    int read;

                    while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, read);
                        downloaded += read;
                        downloadProgressCallback?.Invoke(response.ContentLength, downloaded);
                    }
                }

                UnzipFile(_zip, Path.Combine(_zip.Directory.FullName, "CCleanerPortable"), unzipProgressCallback);
            }
            finally
            {
                Interlocked.Decrement(ref _syncPoint);
            }            
        }

        private static void UnzipFile(FileInfo zipFIle, string outFolder, UnzipProgressCallback progressCallback)
        {
            var events = new FastZipEvents();
            events.Progress += UnzipProgressHandler;
            
            var fastZip = new FastZip(events);

            try
            {
                fastZip.ExtractZip(zipFIle.OpenRead(), outFolder, FastZip.Overwrite.Always, null, null, null, true, true);                
            }
            catch (Exception e)
            {
                throw new DiskCleanupException(e.Message, e);
            }

            zipFIle.Delete();

            void UnzipProgressHandler(object sender, ProgressEventArgs e)
            {
                progressCallback?.Invoke(e.Name, e.PercentComplete);
            }
        }       

        public void Run()
        {
            if (Interlocked.CompareExchange(ref _syncPoint, 1, 0) != 0)
                throw new InvalidOperationException("A download or cleanup operation is already in progress.");

            if (DownloadNeeded)
                throw new FileNotFoundException("CCleaner has not been downloaded.");

            var psi = new ProcessStartInfo(_exe.FullName, "/AUTO");

            if (RunAsAdministrator)
                psi.Verb = "runas";

            using (var proc = Process.Start(psi))
                proc?.WaitForExit();

            Interlocked.Decrement(ref _syncPoint);
        }

        #endregion

        #region Async methods

        public IAsyncResult BeginDownload(DownloadProgressCallback downloadProgressCallback, UnzipProgressCallback unzipProgressCallback, AsyncCallback callback, object state)
        {
            _downloadDelegate = Download;

            return _downloadDelegate.BeginInvoke(downloadProgressCallback, unzipProgressCallback, callback, state);
        }

        public void EndDownload(IAsyncResult asyncResult)
        {
            _downloadDelegate.EndInvoke(asyncResult);
        }

        public IAsyncResult BeginRun(AsyncCallback callback, object state)
        {
            _invocationDelegate = Run;

            return _invocationDelegate.BeginInvoke(callback, state);
        }

        public void EndRun(IAsyncResult asyncResult)
        {
            _invocationDelegate.EndInvoke(asyncResult);
        }

        #endregion
    }
}

