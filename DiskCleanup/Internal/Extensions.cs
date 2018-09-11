namespace DiskCleanup.Internal
{
    internal static class Extensions
    {
        public static double Megabytes(this FileSize fileSize)
        {
            return (double) fileSize.Bytes / FileSize.OneMegabyte;
        }

        public static double MegabytesOnDisk(this FileSize fileSize)
        {
            return (double) fileSize.BytesOnDisk / FileSize.OneMegabyte;
        }

        public static double Gigabytes(this FileSize fileSize)
        {
            return (double) fileSize.Bytes / FileSize.OneGigabyte;
        }

        public static double GigabytesOnDisk(this FileSize fileSize)
        {
            return (double) fileSize.BytesOnDisk / FileSize.OneGigabyte;
        }

        public static double Terabytes(this FileSize fileSize)
        {
            return (double) fileSize.Bytes / FileSize.OneTerabyte;
        }

        public static double TerabytesOnDisk(this FileSize fileSize)
        {
            return (double) fileSize.BytesOnDisk / FileSize.OneTerabyte;
        }
    }
}