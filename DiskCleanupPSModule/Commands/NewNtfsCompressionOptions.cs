using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using DiskCleanup.Compression;

namespace DiskCleanup.Commands
{
    [Cmdlet(VerbsCommon.New, "NtfsCompressionOptions")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class NewNtfsCompressionOptions : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public SwitchParameter EnableCompression { get; set; }

        [Parameter]
        public SwitchParameter Recurse { get; set; }

        [Parameter]
        public SwitchParameter ContinueOnError { get; set; }

        [Parameter]
        public SwitchParameter ForceRecompress { get; set; }

        [Parameter]
        public SwitchParameter ExeOptimized { get; set; }

        [Parameter]
        public SwitchParameter RunAsAdministrator { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteObject(new NtfsCompressionOptions
            {
                ContinueOnError = ContinueOnError,
                EnableCompression = EnableCompression,
                ExeOptimized = ExeOptimized,
                ForceRecompress = ForceRecompress,
                Recurse = Recurse,
                RunAsAdministrator = RunAsAdministrator
            });
        }
    }
}