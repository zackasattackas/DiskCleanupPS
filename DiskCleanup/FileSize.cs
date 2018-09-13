using System;

namespace DiskCleanup
{
    public struct FileSize
    {
        public const long OneKilobyte = 1024L;
        public const long OneMegabyte = 1048576L;
        public const long OneGigabyte = 1073741824L;
        public const long OneTerabyte = 1099511627776L;

        public long Bytes { get; private set; }
        public long BytesOnDisk { get; private set; }

        public static FileSize operator + (FileSize left, FileSize right)
        {
            left.Bytes += right.Bytes;
            left.BytesOnDisk += right.BytesOnDisk;

            return left;
        }

        public static FileSize operator -(FileSize left, FileSize right)
        {
            left.Bytes -= right.Bytes;
            left.BytesOnDisk -= right.BytesOnDisk;

            return left;
        }

        public FileSize(long bytes, long bytesOnDisk) : this()
        {
            Bytes = bytes;
            BytesOnDisk = bytesOnDisk;
        }

        public override string ToString()
        {
            return ToString("d");
        }

        public string ToString(string format)
        {
            switch (format)
            {
                case "s":
                    return ToString(Bytes);
                case "d":
                    return ToString(BytesOnDisk);
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), "Invalid format specifier.");
            }
        }

        private static string ToString(long size)
        {
            double value;
            string unit;
            var absolute = Math.Abs(size);

            if (absolute >= OneTerabyte)
            {
                value = (double)size / OneTerabyte;
                unit = "TB";
            }
            else if(absolute >= OneGigabyte)
            {
                value = (double)size / OneGigabyte;
                unit = "GB";
            }
            else if (absolute >= OneMegabyte)
            {
                value = (double)size / OneMegabyte;
                unit = "MB";
            }
            else if (absolute >= OneKilobyte)
            {
                value = (double)size / OneKilobyte;
                unit = "KB";
            }
            else
            {
                value = size;
                unit = "B";
            }

            return $"{value:N2} {unit}";
        }
    }
}