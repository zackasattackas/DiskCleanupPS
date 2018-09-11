using System;

namespace DiskCleanup.Commands
{
    [Flags]
    public enum TempFileScope
    {
        AllUsers = 1,
        System = 2,
        Default = AllUsers | System
    }
}