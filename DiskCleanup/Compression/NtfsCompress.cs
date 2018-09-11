using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace DiskCleanup.Compression
{
    public class NtfsCompress
    {
        #region Fields

        private delegate NtfsCompressionResult CompressionProcedure(TimeSpan timeout, bool terminateOnTimeout);
        private CompressionProcedure _asyncDelegate;
        private int _syncPoint;
        private bool _cancel;

        #endregion

        #region Properties

        public FileSystemInfo FileSystemInfo { get; set; }
        public NtfsCompressionOptions Options { get; set; }

        #endregion

        #region Events

        public event EventHandler<DataReceivedEventArgs> OutputDataReceived;
        public event EventHandler<DataReceivedEventArgs> ErrorDataReceived;

        #endregion

        #region Ctor

        public NtfsCompress(FileSystemInfo fileSystemInfo, NtfsCompressionOptions options)
        {
            FileSystemInfo = fileSystemInfo;
            Options = options;
        }

        #endregion

        #region Methods

        public NtfsCompressionResult Compress(TimeSpan timeout, bool terminateOnTimeout)
        {
            if (Interlocked.CompareExchange(ref _syncPoint, 1, 0)  != 0) 
                throw new DiskCleanupException("Another compression operation is currently in progress. The NtfsCompress object is not reentrant.");

            var compactExe = Path.Combine(Environment.SystemDirectory, "compact.exe");
            var arguments  = new StringBuilder(Options.EnableCompression ? "/C " : "/U ");            
            var preCompressionSize = FileSystemInfo.GetSize();

            if (Options.ForceRecompress)
                arguments.Append("/F ");
            if (Options.ContinueOnError)
                arguments.Append("/I ");
            if (Options.ExeOptimized)
                arguments.Append("/EXE ");
            if (Options.Recurse)
                arguments.Append("/S:");

            arguments.Append($"\"{FileSystemInfo.FullName}\"");

            var psi = new ProcessStartInfo(compactExe, arguments.ToString())
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = ErrorDataReceived != null,
                RedirectStandardOutput = OutputDataReceived != null,
                Verb = Options.RunAsAdministrator ? "runas" : null
            };

            var stopwatch = timeout >= TimeSpan.Zero ? Stopwatch.StartNew() : null;
            try
            {
                using (var proc = Process.Start(psi) ?? throw new DiskCleanupException("An error occurred invoking compact.exe."))
                {
                    if (psi.RedirectStandardOutput)
                    {
                        proc.BeginOutputReadLine();
                        proc.OutputDataReceived += OnOutputDataReceived;
                    }

                    if (psi.RedirectStandardError)
                    {
                        proc.BeginErrorReadLine();
                        proc.ErrorDataReceived += OnErrorDataReceived;
                    }
               
                    // Limited timeout, wait for timeout to elapse, process to exit or user to cancel, whichever comes first.
                    if (stopwatch != null)
                    {
                        while (stopwatch.Elapsed < timeout && !proc.HasExited && !_cancel)
                            Thread.Sleep(250);

                        stopwatch.Stop();

                        if (proc.HasExited)
                            return new NtfsCompressionResult(preCompressionSize, FileSystemInfo.GetSize());

                        if (terminateOnTimeout)
                            proc.Kill();

                        throw stopwatch.Elapsed >= timeout
                            ? new TimeoutException("The timeout limit was reached while waiting for compact.exe.")
                            : (Exception)new OperationCanceledException("The compress/uncompress operation was cancelled by the user.");
                    }

                    // Infinite timeout, wait for process to exit or for user cancellation
                    while (!proc.HasExited && !_cancel)
                        Thread.Sleep(250);

                    if (proc.HasExited)
                        return new NtfsCompressionResult(preCompressionSize, FileSystemInfo.GetSize());

                    proc.Kill();

                    throw new OperationCanceledException("The compress/uncompress operation was cancelled by the user.");
                }
            }
            finally
            {
                Interlocked.Decrement(ref _syncPoint);
            }          
        }

        public IAsyncResult BeginCompress(TimeSpan timeout, bool terminateOnTimeout, AsyncCallback callback, object state)
        {
            _asyncDelegate = Compress;

            return _asyncDelegate.BeginInvoke(timeout, terminateOnTimeout, callback, state);
        }

        public NtfsCompressionResult EndCompress(bool cancel, IAsyncResult asyncResult)
        {
            _cancel = cancel;
            return _asyncDelegate.EndInvoke(asyncResult);
        }

        #endregion

        #region Event handlers

        protected void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            OutputDataReceived?.Invoke(sender, args);
        }

        protected void OnErrorDataReceived(object sender, DataReceivedEventArgs args)
        {
            ErrorDataReceived?.Invoke(sender, args);
        }

        #endregion
    }
}
 