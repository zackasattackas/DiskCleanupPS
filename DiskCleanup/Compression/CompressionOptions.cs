namespace DiskCleanup.Compression
{
    public class CompressionOptions
    {
        public bool EnableCompression { get; set; }
        public bool Recurse { get; set; }
        public bool ContinueOnError { get; set; }
        public bool ForceRecompress { get; set; }
        public bool ExeOptimized { get; set; }
        public bool RunAsAdministrator { get; set; }
    }
}