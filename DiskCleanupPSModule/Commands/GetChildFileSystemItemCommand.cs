using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management.Automation;
using DiskCleanup.Internal;

namespace DiskCleanup.Commands
{
    [Cmdlet(VerbsCommon.Get, "ChildFileSystemItem", DefaultParameterSetName = "Path")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class GetChildFileSystemItemCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "Path")]
        public string[] Path { get; set; }

        [Parameter(ValueFromPipeline = true)]
        [Parameter(ParameterSetName = "Pipeline")]
        public DirectoryInfo[] InputObject { get; set; }

        [Parameter]
        public SwitchParameter Recurse { get; set; }

        [Parameter]
        public FileAttributes AttributeFilter { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            switch (ParameterSetName)
            {
                case "Path":
                    if (Path == null)
                        Path = new []{ Directory.GetCurrentDirectory() };

                    foreach (var path in Path)
                        if (System.IO.Path.HasExtension(path))
                            WriteObject(new FileInfo(path).ToPSObject());
                        else
                            EnumerateFileSystemInfos(new DirectoryInfo(path), Recurse, AttributeFilter);
                    break;
                case "Pipeline":
                    foreach (var directoryInfo in InputObject)
                        if (!Recurse)
                            WriteObject(directoryInfo.ToPSObject());
                        else
                            EnumerateFileSystemInfos(directoryInfo, Recurse, AttributeFilter);
                    break;                    
            }


            void EnumerateFileSystemInfos(DirectoryInfo directoryInfo, bool recurse, FileAttributes attributeFilter = default)
            {
                try
                {
                    foreach (var fileSystemInfo in directoryInfo.GetFileSystemInfos())
                    {
                        if (recurse && fileSystemInfo is DirectoryInfo dir)
                            EnumerateFileSystemInfos(dir, true, attributeFilter);

                        if (attributeFilter != default && (fileSystemInfo.Attributes & attributeFilter) == 0)
                            continue;

                        WriteObject(fileSystemInfo.ToPSObject());
                    }
                }
                catch (Exception e) when(e is UnauthorizedAccessException || e is IOException)
                {
                    WriteError(e.ToErrorRecord());
                }
            }
        }
    }
}
