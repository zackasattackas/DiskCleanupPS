using System;

namespace DiskCleanup
{
    public class DiskCleanupException : Exception
    {
        public DiskCleanupException(string message, Exception innerException = default) 
            : base(message, innerException)
        {
        }
    }
}