using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DiskCleanup.Compression;

namespace diskcleaner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var cancel = 0;
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                Interlocked.Increment(ref cancel);
            };

            var ntfsCompress = new NtfsCompress(
                new DirectoryInfo("C:\\windows\\SoftwareDistribution"),
                new CompressionOptions {EnableCompression = true, ContinueOnError = true, Recurse = true});

            //ntfsCompress.Compress(ref cancel);

            Debugger.Break();
        }
    }
}