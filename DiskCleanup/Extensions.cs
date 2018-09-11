using System.IO;

namespace DiskCleanup
{
    public static class Extensions
    { 
        public static FileSize GetSize(this FileSystemInfo fileSystemInfo)
        {
            return fileSystemInfo is FileInfo fileInfo
                ? Utilities.GetFileSize(fileInfo)
                : Utilities.GetDirectorySize(fileSystemInfo as DirectoryInfo, false).Size;
        }

        public static DirectorySize GetSize(this DirectoryInfo directoryInfo, bool ignoreUnauthorizedAccessAndIOExceptions, ProgressCallback progressCallback = default)
        {
            return Utilities.GetDirectorySize(directoryInfo, ignoreUnauthorizedAccessAndIOExceptions, progressCallback);
        }

        public static void TryRecursiveDelete(this DirectoryInfo directoryInfo, bool recreate, out int inaccessible)
        {
            Utilities.DeleteDirectory(directoryInfo, recreate, out inaccessible);
        }
    }
}
