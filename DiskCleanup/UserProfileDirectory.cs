using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiskCleanup
{
    public static class UserProfileDirectory
    {
        public static DirectoryInfo DirectoryInfo => new DirectoryInfo(Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "users"));

        public static IEnumerable<DirectoryInfo> GetUserProfiles(bool includeHidden)
        {
            return from d in DirectoryInfo.GetDirectories()
                where includeHidden || (d.Attributes & FileAttributes.Hidden) == 0
                select d;
        }
    }
}