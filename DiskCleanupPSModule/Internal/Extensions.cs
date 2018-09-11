using System;
using System.Management.Automation;

namespace DiskCleanup.Internal
{
    internal static class Extensions
    {
        public static ErrorRecord ToErrorRecord(this Exception e)
        {
            return new ErrorRecord(e, null, ErrorCategory.NotSpecified, null);
        }

        public static PSObject ToPSObject(this object o)
        {
            return PSObject.AsPSObject(o);
        }
    }
}
