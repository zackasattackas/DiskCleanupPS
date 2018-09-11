namespace DiskCleanup.Compression
{
    public struct CompressionResult
    {
        public FileSize PreCompressionSize { get; set; }
        public FileSize PostCompressionSize { get; set; }

        public CompressionResult(FileSize preCompressionSize, FileSize postCompressionSize) : this()
        {
            PreCompressionSize = preCompressionSize;
            PostCompressionSize = postCompressionSize;
        }
    }
}