using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using DiskCleanup.Internal;

namespace DiskCleanup.Commands
{
    [Cmdlet("Invoke", "CCleaner")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class InvokeCCleanerCommand : PSCmdlet
    {
        private Stopwatch _downloadTimer;

        [Parameter]
        public SwitchParameter AllowDownload { get; set; }
        [Parameter]
        public SwitchParameter RunAsAdministrator { get; set; }
        protected override void ProcessRecord()
        {
            base.ProcessRecord(); 
            
            var ccleaner = new CCleaner {RunAsAdministrator = RunAsAdministrator};

            try
            {
                if (ccleaner.DownloadNeeded)
                {
                    if (!AllowDownload)
                        throw new DiskCleanupException("CCleaner was not found. Run this command again with the -AllowDownload parameter.");

                    _downloadTimer = Stopwatch.StartNew();
                    ccleaner.Download(WriteDownloadProgress, WriteUnzipProgress);

                    if (ccleaner.DownloadNeeded)
                        throw new FileNotFoundException("CCleaner.exe could not be found after downloading. Its possible an error occurred while unzipping the archive, or a file name has been changed.");
                }
          
                var asyncResult = ccleaner.BeginRun(null, null);

                while (!asyncResult.IsCompleted && !asyncResult.CompletedSynchronously)
                {
                    var driveInfo = DriveInfo.GetDrives().Single(d => d.Name == Path.GetPathRoot(Environment.SystemDirectory));
                    WriteProgress(new ProgressRecord(0, "Running CCleaner...",
                        $"Drive: {driveInfo.Name}{(string.IsNullOrEmpty(driveInfo.VolumeLabel) ? null : $" ({driveInfo.VolumeLabel})")}   " +
                        $"Total Size: {new FileSize(driveInfo.TotalSize, driveInfo.TotalSize)} ({driveInfo.TotalSize:N0}) bytes   " +
                        $"Free Space: {new FileSize(driveInfo.TotalFreeSpace, driveInfo.TotalFreeSpace)} ({driveInfo.TotalFreeSpace:N0}) bytes"));

                    Thread.Sleep(250);
                }

                ccleaner.EndRun(asyncResult);
            }
            catch (Exception e)
            {
                WriteError(e.ToErrorRecord());
            }
        }

        private void WriteDownloadProgress(long streamSize, long bytesDownloaded)
        {
            Host.UI.WriteProgress(0, new ProgressRecord(0, 
                "Downloading CCleaner Portable...",
                $"Size: {new FileSize(streamSize, streamSize)}   " +
                $"Downloaded: {new FileSize(bytesDownloaded, bytesDownloaded)}   " +
                $"Speed: {(double )bytesDownloaded / 131072 / _downloadTimer.Elapsed.TotalSeconds:N2} Mb/s")
            {
                PercentComplete = (int) ((double) bytesDownloaded / streamSize * 100)
            });
        }

        private void WriteUnzipProgress(string fileName, double percentComplete)
        {
            Host.UI.WriteProgress(0, new ProgressRecord(0, "Unzipping CCleaner Portable...", $"File Name: {fileName}")
            {
                PercentComplete = (int) percentComplete
            });
        }
    }
}
  