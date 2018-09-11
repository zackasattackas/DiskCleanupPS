namespace DiskCleanup
{
    public struct DirectorySize
    {
        public int FileCount { get; internal set; }
        public int FolderCount { get; internal set; }
        public int Inaccessible { get; internal set; }
        public FileSize Size { get; internal set; }

        public static DirectorySize operator +(DirectorySize left, DirectorySize right)
        {
            left.FileCount += right.FileCount;
            left.FolderCount += right.FolderCount;
            left.Inaccessible += right.Inaccessible;
            left.Size += right.Size;

            return left;
        }

        //internal void Add(DirectorySize other)
        //{
        //    FileCount += other.FileCount;
        //    FolderCount += other.FolderCount;
        //    Inaccessible += other.Inaccessible;
        //    Size += other.Size;
        //}

        public override string ToString()
        {
            return Size.ToString();
        }

        public string ToString(string format)
        {
            return Size.ToString(format);
        }
    }
}
