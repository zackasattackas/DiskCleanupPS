using System;

namespace DiskCleanup.Compression
{
    public class CompressionThread
    {
        public NtfsCompress NtfsCompress { get; set; }
        public IAsyncResult AsyncResult { get; set; }
        public CompressionThread(NtfsCompress ntfsCompress, IAsyncResult asyncResult)
        {
            NtfsCompress = ntfsCompress;
            AsyncResult = asyncResult;
        }
    }
}