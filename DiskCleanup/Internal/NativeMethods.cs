using System.Runtime.InteropServices;

namespace DiskCleanup.Internal
{
    internal class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint GetCompressedFileSize(string fileName, out ulong fileSizeHigh);
    }
}