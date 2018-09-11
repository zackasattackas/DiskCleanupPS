using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using DiskCleanup.Internal;

namespace DiskCleanup
{
    internal static class Utilities
    {
        public static FileSize GetFileSize(FileInfo fileInfo)
        {
            try
            {
                if ((fileInfo.Attributes & FileAttributes.Compressed) == 0)
                    return new FileSize(fileInfo.Length, fileInfo.Length);

                var lowOrder = NativeMethods.GetCompressedFileSize(fileInfo.FullName, out var highOrder);

                if (highOrder == 0 && lowOrder == 0xffffffff)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return new FileSize(fileInfo.Length, (long)highOrder + lowOrder);
            }
            catch (Win32Exception e)
            {
                switch (e.NativeErrorCode)
                {
                    case 3:
                        throw new FileNotFoundException(e.Message, e);
                    case 5:
                        throw new UnauthorizedAccessException(e.Message, e);
                    case 32:
                        throw new IOException(e.Message, e);
                    default:
                        throw;
                }
            }
        }
        
        public static DirectorySize GetDirectorySize(DirectoryInfo directoryInfo, bool ignoreUnauthorizedAccessAndIOExceptions, ProgressCallback progressCallback = null)
        {
            if (!directoryInfo.Exists)
                throw new DirectoryNotFoundException("The directory does not exist.");

            var directorySize = new DirectorySize();

            foreach (var fileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                if ((fileSystemInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                    continue;

                try
                {
                    switch (fileSystemInfo)
                    {
                        case DirectoryInfo subDirectoryInfo:
                            directorySize += GetDirectorySize(subDirectoryInfo, ignoreUnauthorizedAccessAndIOExceptions);
                            directorySize.FolderCount++;
                            break;
                        case FileInfo fileInfo:
                            directorySize.Size += GetFileSize(fileInfo);
                            directorySize.FileCount++;
                            break;
                    }
                }
                catch (FileNotFoundException)
                {
                    // ignored
                }
                catch (Exception e) when (e is UnauthorizedAccessException || e is IOException)
                {
                    directorySize.Inaccessible++;

                    if (!ignoreUnauthorizedAccessAndIOExceptions)
                        throw;
                }
                finally
                {
                    progressCallback?.Invoke(fileSystemInfo.FullName, directorySize);
                }
            }

            return directorySize;
        }

        public static void DeleteDirectory(DirectoryInfo directory, bool recreate, out int inaccessible)
        {
            inaccessible = 0;
            try
            {
                foreach (var fileSystemInfo in directory.GetFileSystemInfos())
                {
                    if (fileSystemInfo is DirectoryInfo directoryInfo)
                    {
                        DeleteDirectory(directoryInfo, false, out var failed);
                        inaccessible += failed;
                    }

                    try
                    {
                        fileSystemInfo.Delete();
                    }
                    catch (Exception e) when (e is UnauthorizedAccessException || e is IOException)
                    {
                        inaccessible++;
                    }
                }
                
                if (recreate)
                    directory.Create();
            }
            catch (Exception e) when (e is UnauthorizedAccessException || e is IOException)
            {
                inaccessible++;
            }
        }
    }    
}