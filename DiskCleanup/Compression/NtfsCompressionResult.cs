namespace DiskCleanup.Compression
{
    public struct NtfsCompressionResult
    {
        public FileSize PreCompressionSize { get; set; }
        public FileSize PostCompressionSize { get; set; }

        public NtfsCompressionResult(FileSize preCompressionSize, FileSize postCompressionSize) : this()
        {
            PreCompressionSize = preCompressionSize;
            PostCompressionSize = postCompressionSize;
        }
    }
}