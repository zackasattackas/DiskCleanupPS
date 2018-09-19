using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DiskCleanup
{
    public static class RecycleBin
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static class NativeMethods
        {
            public const int SHERB_NOCONFIRMATION = 1;
            public const int SHERB_NOPROGRESSUI = 2;
            public const int SHERB_NOSOUND = 4;

            [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int SHEmptyRecycleBinW(IntPtr hwnd, string pszRootPath, uint dwFlags);
        }

        [Flags]
        public enum EmptyOptions : uint
        {
            Default = NoConfirmation | NoProgressUI | NoSound,
            NoConfirmation = NativeMethods.SHERB_NOCONFIRMATION,
            NoProgressUI = NativeMethods.SHERB_NOPROGRESSUI,
            NoSound = NativeMethods.SHERB_NOSOUND
        }

        public static void Empty(IntPtr parent = default, string rootPath = null, EmptyOptions options = EmptyOptions.Default)
        {
            var result = NativeMethods.SHEmptyRecycleBinW(parent, rootPath, (uint) options);
            //if (result != 0)
            //    throw Marshal.GetExceptionForHR(result);
        }
    }
}
