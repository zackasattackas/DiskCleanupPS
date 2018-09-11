using System;

namespace DiskCleanup.Compression
{
    public class NtfsCompressionThread
    {
        public NtfsCompress NtfsCompress { get; set; }
        public IAsyncResult AsyncResult { get; set; }
        public NtfsCompressionThread(NtfsCompress ntfsCompress, IAsyncResult asyncResult)
        {
            NtfsCompress = ntfsCompress;
            AsyncResult = asyncResult;
        }
    }
}