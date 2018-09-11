using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management.Automation;
using DiskCleanup.Internal;

namespace DiskCleanup.Commands
{
    [Cmdlet(VerbsCommon.Get, "FileSystemInfo")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class GetFileSystemInfoCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "Path")]
        public string[] Path { get; set; }

        [Parameter(ValueFromPipeline = true)]
        [Parameter(ParameterSetName = "Pipeling")]
        public DirectoryInfo[] InputObject { get; set; }

        [Parameter]
        public SwitchParameter Recurse { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            switch (ParameterSetName)
            {
                case "Path":
                    foreach (var path in Path)
                        if (System.IO.Path.HasExtension(path))
                            WriteObject(new FileInfo(path).ToPSObject());
                        else
                            EnumerateFileSystemInfos(new DirectoryInfo(path), Recurse);
                    break;
                case "Pipeline":
                    foreach (var directoryInfo in InputObject)
                        if (!Recurse)
                            WriteObject(directoryInfo.ToPSObject());
                        else
                            EnumerateFileSystemInfos(directoryInfo, Recurse);
                    break;                    
            }


            void EnumerateFileSystemInfos(DirectoryInfo directoryInfo, bool recurse)
            {
                try
                {
                    foreach (var fileSystemInfo in directoryInfo.GetFileSystemInfos())
                    {
                        if (recurse && fileSystemInfo is DirectoryInfo dir)
                            EnumerateFileSystemInfos(dir, recurse);

                        WriteObject(fileSystemInfo.ToPSObject());
                    }
                }
                catch (Exception e) when(e is UnauthorizedAccessException || e is IOException)
                {
                    // ignore
                }
            }
        }

        private static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(DirectoryInfo directoryInfo, bool recurse)
        {
            foreach (var fileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                if (recurse && fileSystemInfo is DirectoryInfo dir)
                    foreach (var item in EnumerateFileSystemInfos(dir, recurse))
                        yield return item;

                yield return fileSystemInfo ;
            }
        }
    }
}
