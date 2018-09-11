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

        [Parameter]
        [ValidateSet("Include", "Exclude", "IncludeAny", "ExcludeAny")]
        public string FilterOption { get; set; } = "IncludeAny";

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            switch (ParameterSetName)
            {
                case "Path":
                    if (Path == null)
                        Path = new[] {SessionState.Path.CurrentFileSystemLocation.Path};

                    foreach (var path in Path)
                        if (System.IO.Path.HasExtension(path))
                            if (!File.Exists(path))
                                WriteError(new FileNotFoundException("The file does not exist.", path).ToErrorRecord());
                            else
                                WriteObject(new FileInfo(path).ToPSObject());
                        else if (!Directory.Exists(path))
                            WriteError(new DirectoryNotFoundException("The directory does not exist.").ToErrorRecord());
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

                        if (attributeFilter != default)
                            switch (FilterOption)
                            {
                                case "Include":
                                    if ((fileSystemInfo.Attributes & attributeFilter) != attributeFilter)
                                        continue;
                                    break;
                                case "Exclude":
                                    if ((fileSystemInfo.Attributes & attributeFilter) == attributeFilter)
                                        continue;
                                    break;
                                case "IncludeAny":
                                    if ((fileSystemInfo.Attributes & attributeFilter) == 0)
                                        continue;
                                    break;
                                case "ExcludeAny":
                                    if ((fileSystemInfo.Attributes & attributeFilter) != 0)
                                        continue;
                                    break;
                            }

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
