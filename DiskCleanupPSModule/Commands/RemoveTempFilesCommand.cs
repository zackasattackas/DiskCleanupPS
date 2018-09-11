using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management.Automation;
using DiskCleanup.Internal;

namespace DiskCleanup.Commands
{
    [Cmdlet(VerbsCommon.Remove, "TempFiles", DefaultParameterSetName = "Scope")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class RemoveTempFilesCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "Pipeline", ValueFromPipeline = true)]
        public DirectoryInfo[] InputObject { get; set; }

        [Parameter(ParameterSetName = "Scope")]
        [ValidateSet("Default", "AllUsers", "System")]
        public TempFileScope Scope { get; set; } =  TempFileScope.Default;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            switch (ParameterSetName)
            {
                case "Scope":
                    if ((Scope & TempFileScope.AllUsers) > 0)
                        foreach (var userProfile in UserProfileDirectory.GetUserProfiles(false))
                        {
                            var userTemp = new DirectoryInfo(Path.Combine(userProfile.FullName, "AppData\\Local\\Temp"));
                            if (!userTemp.Exists)
                                continue;
                            try
                            {
                                userTemp.TryRecursiveDelete(false, out var inaccessible);

                                if (inaccessible > 0)
                                    WriteWarning($"{inaccessible} file system objects could not be deleted because they were in use or could not be accessed.");
                            }
                            catch (Exception e)
                            {
                                WriteError(e.ToErrorRecord());
                            }
                        }
                    else if ((Scope & TempFileScope.System) > 0)
                    {
                        var windowsPath = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.System));
                        var windowsTemp = new DirectoryInfo(Path.Combine(windowsPath, "Temp"));

                        try
                        {
                            windowsTemp.TryRecursiveDelete(false, out var inaccessible);

                            if (inaccessible > 0)
                                WriteWarning($"{inaccessible} file system objects could not be deleted because they were in use or could not be accessed.");
                        }
                        catch (Exception e)
                        {
                            WriteError(e.ToErrorRecord());
                        }
                    }
                    break;
                case "Pipeline":

                    foreach (var item in InputObject)
                    {
                        if (item.Parent == null || !item.Parent.Name.Equals("users", StringComparison.CurrentCultureIgnoreCase))
                            throw new ArgumentException("The input value is invalid because the parent directory does not match the user profiles directory name.");

                        var usertemp = new DirectoryInfo(Path.Combine(item.FullName, "AppData\\Local\\Temp"));
                        if (!usertemp.Exists)
                            continue;
                        try
                        {
                            usertemp.TryRecursiveDelete(false, out var inaccessible);

                            WriteVerbose($"Deleted all files not in use from \"{usertemp.FullName}\".");

                            if (inaccessible > 0)
                                WriteWarning($"{inaccessible} file system objects could not be deleted because they were in use or could not be accessed.");
                        }
                        catch (Exception e)
                        {
                            WriteError(e.ToErrorRecord());
                        }
                    }
                    break;
            }
        }
    }
}
