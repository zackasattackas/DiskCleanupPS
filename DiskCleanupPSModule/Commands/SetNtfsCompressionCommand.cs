using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using DiskCleanup.Compression;
using DiskCleanup.Internal;

namespace DiskCleanup.Commands
{
    [Cmdlet(VerbsCommon.Set, "NtfsCompression")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SetNtfsCompressionCommand : PSCmdlet
    {
        #region Fields

        private static readonly object SyncObject = new object();
        private string _compactOutput;
        private List<NtfsCompressionThread> _compressionThreads;        

        #endregion

        #region Properties

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string[] Path { get; set; }

        [Parameter(Position = 1, Mandatory = true)] public NtfsCompressionOptions Options { get; set; }

        [Parameter]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(System.Threading.Timeout.Infinite);

        [Parameter]
        public SwitchParameter TerminateOnTimeout { get; set; } = true;

        [Parameter]
        public SwitchParameter RunAsAdministrator { get; set; }

        [Parameter]
        public SwitchParameter RedirectStandardError { get; set; }

        [Parameter]
        public SwitchParameter RedirectStandardOutput { get; set; }

        #endregion

        #region Cmdlet overrides

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (Path.Length > 1 && RedirectStandardError)
                throw new ArgumentException("The standard output and error streams cannot be redirected for multiple paths.");            
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            _compressionThreads = new List<NtfsCompressionThread>();

            foreach (var path in Path)
            {
                var fsi = System.IO.Path.HasExtension(path) ? (FileSystemInfo) new FileInfo(path) : new DirectoryInfo(path);

                if (!fsi.Exists)
                {
                    WriteError((fsi is FileInfo ? (Exception) new FileNotFoundException("The file was not found.", fsi.FullName) : new DirectoryNotFoundException($"The directory \"{fsi.FullName}\" was not found.")).ToErrorRecord());
                    continue;
                }

                if ((fsi.Attributes & FileAttributes.Compressed) != 0 && !Options.ForceRecompress)
                {
                    WriteWarning($"The file system object \"{fsi.FullName}\" already has NTFS compression enabled, and the ForceRecompress property on the NtfsCompressionOptions object was false.");
                    continue;
                }

                var ntfsCompress = new NtfsCompress(fsi, Options);
                
                if (Path.Length == 1)
                    ntfsCompress.OutputDataReceived += NtfsCompressOnOutputDataReceived;

                if (RedirectStandardError)
                    ntfsCompress.ErrorDataReceived += NtfsCompressOnErrorDataReceived;

                _compressionThreads.Add(new NtfsCompressionThread(ntfsCompress, ntfsCompress.BeginCompress(Timeout, TerminateOnTimeout, null, null)));
            }        

            ThreadPool.QueueUserWorkItem(ProgressThreadProc);

            try
            {
                while (_compressionThreads.Any())
                {
                    int index, i;
                    for (index = 0, i = 0; i < _compressionThreads.Count; i++)
                    {
                        if (_compressionThreads[i].AsyncResult.IsCompleted || _compressionThreads[i].AsyncResult.CompletedSynchronously)
                        {
                            index = i;
                            break;
                        }

                        if (index < _compressionThreads.Count - 1)
                            continue;

                        Thread.Sleep(250);
                        i = -1;
                    }

                    var thread = _compressionThreads[index];

                    try
                    {
                        var result = thread.NtfsCompress.EndCompress(false, thread.AsyncResult);
                        WriteObject(PSObject.AsPSObject(
                            new
                            {
                                Path = thread.NtfsCompress.FileSystemInfo.FullName,
                                result.PreCompressionSize,
                                result.PostCompressionSize
                            }));
                    }
                    catch (Exception e)
                    {
                        WriteError(new ErrorRecord(e, null, ErrorCategory.NotSpecified, null));
                    }
                    finally
                    {
                        _compressionThreads.RemoveAt(index);
                    }                
                }
            }
            finally
            {
                if (_compressionThreads.Any())
                    foreach (var compressionThread in _compressionThreads)
                        try
                        {
                            compressionThread.NtfsCompress.EndCompress(false, compressionThread.AsyncResult);
                        }
                        catch (Exception e) when (e is OperationCanceledException)
                        {
                            WriteWarning(e.Message);
                        }
            }

            void ProgressThreadProc(object state)
            {
                var timer = Stopwatch.StartNew();

                while (_compressionThreads.Any())
                {
                    lock (SyncObject)
                        Host.UI.WriteProgress(0, new ProgressRecord(0,
                            $"{(Path.Length == 1 ? (!RedirectStandardOutput && string.IsNullOrEmpty(_compactOutput) ? "Waiting for data from compact.exe output stream..." : RedirectStandardOutput ? "Running compact.exe (output redirected)..." : _compactOutput) : $"{(Options.EnableCompression ? "Compressing" : "Uncompressing")} file system objects...")}",
                            $"Paths to compress: {Path.Length}   Processes: {_compressionThreads.Count}   Time Elapsed: {timer.Elapsed:g}"));

                    Thread.Sleep(500);
                }

                timer.Stop();
            }
        }

        protected override void StopProcessing()
        {
            if (!_compressionThreads.Any())
                return;

            foreach (var compressionThread in _compressionThreads)
                try
                {
                    compressionThread.NtfsCompress.EndCompress(true, compressionThread.AsyncResult);
                }
                catch (Exception e) when (e is OperationCanceledException)
                {
                    Host.UI.WriteWarningLine(e.Message);
                }

            base.StopProcessing();
        }

        #endregion

        private void NtfsCompressOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            Host.UI.WriteWarningLine(e.Data.Trim());            
        }

        private void NtfsCompressOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            if (RedirectStandardOutput)
                Host.UI.WriteLine(e.Data);
            else
                lock (SyncObject)
                    _compactOutput = e.Data.Trim();
        }
    }
}
