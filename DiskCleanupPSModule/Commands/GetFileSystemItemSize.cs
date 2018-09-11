using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management.Automation;
using DiskCleanup.Internal;

namespace DiskCleanup.Commands
{
    [Cmdlet(VerbsCommon.Get, "FileSystemItemSize", DefaultParameterSetName = "Path")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class GetFileSystemItemSize : PSCmdlet
    {
        [Parameter(ParameterSetName = "Path")] public string[] Path { get; set; }

        [Parameter(ParameterSetName = "FileSystemInfo", ValueFromPipeline = true)]
        public FileSystemInfo[] Item { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            switch (ParameterSetName)
            {
                case "Path":
                    foreach (var path in Path)
                        if (System.IO.Path.HasExtension(path))
                        {
                            var fileInfo = new FileInfo(path);
                            try
                            {
                                WriteObject(new { Path = path, Size = fileInfo.GetSize() }.ToPSObject());
                            }
                            catch (Exception e)
                            {
                                WriteError(e.ToErrorRecord());
                            }
                        }
                        else
                        {
                            var directoryInfo = new DirectoryInfo(path);
                            try
                            {
                                var directorySize = directoryInfo.GetSize(true, WriteScanProgress);

                                if (directorySize.Inaccessible > 0)
                                    WriteWarning($"{directorySize.Inaccessible} subdirector{(directorySize.Inaccessible > 0 ? "ies" : "y")} of \"{path}\" could not be accessed.");

                                WriteObject(new
                                {
                                    Path = path,
                                    Files = directorySize.FileCount,
                                    Folders = directorySize.FolderCount,
                                    directorySize.Size
                                }.ToPSObject());
                            }
                            catch (Exception e)
                            {
                                WriteError(e.ToErrorRecord());
                            }
                        }

                    break;
                case "FileSystemInfo":
                    foreach (var fileSystemInfo in Item)
                        try
                        {
                            if (fileSystemInfo is DirectoryInfo directoryInfo)
                            {
                                var directorySize = directoryInfo.GetSize(true, WriteScanProgress);

                                if (directorySize.Inaccessible > 0)
                                    WriteWarning($"{directorySize.Inaccessible} subdirector{(directorySize.Inaccessible > 0 ? "ies" : "y")} of \"{fileSystemInfo.FullName}\" could not be accessed.");

                                WriteObject(new
                                {
                                    Path = fileSystemInfo.FullName,
                                    Files = directorySize.FileCount,
                                    Folders = directorySize.FolderCount,
                                    directorySize.Size
                                }.ToPSObject());
                            }
                            else
                            {
                                WriteObject(new { Path = fileSystemInfo.FullName, Size = fileSystemInfo.GetSize() }.ToPSObject());
                            }
                        }
                        catch (Exception e)
                        {
                            WriteError(e.ToErrorRecord());
                        }
                    break;
            }
        }

        private void WriteScanProgress(string directoryName, DirectorySize directorySize)
        {
            Host.UI.WriteProgress(0, new ProgressRecord(0, $"Item: {directoryName}",
                $"Files: {directorySize.FileCount,-7:N0} " +
                $"Folders: {directorySize.FolderCount,-7:N0} " +
                $"Inaccessible: {directorySize.Inaccessible,-4:N0} " +
                $"Size on disk: {directorySize.ToString()}"));
        }
    }
}
