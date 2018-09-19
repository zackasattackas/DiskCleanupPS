using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using DiskCleanup.Internal;

namespace DiskCleanup.Commands
{
    [Cmdlet(VerbsCommon.Remove, "RecycleBinContent")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class RemoveRecycleBinContentCommand : Cmdlet
    {
        [Parameter(ValueFromPipeline = true)] public string[] PathRoot { get; set; }
        [Parameter] public IntPtr Container { get; set; } = default;
        [Parameter] public SwitchParameter ShowConfirmation { get; set; }
        [Parameter] public SwitchParameter ShowProgressUI { get; set; }
        [Parameter] public SwitchParameter PlaySound { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var options = RecycleBin.EmptyOptions.Default;

            if (ShowConfirmation)
                options ^= RecycleBin.EmptyOptions.NoConfirmation;
            if (ShowProgressUI)
                options ^= RecycleBin.EmptyOptions.NoProgressUI;
            if (PlaySound)
                options ^= RecycleBin.EmptyOptions.NoSound;

            if (PathRoot == null)
            {
                try
                {
                    RecycleBin.Empty(Container, options: options);
                }
                catch (Exception e)
                {
                    WriteError(e.ToErrorRecord());
                }

                return;
            }

            foreach (var path in PathRoot)
            {
                try
                {
                    RecycleBin.Empty(Container, path, options);
                }
                catch (Exception e)
                {
                    WriteError(e.ToErrorRecord());
                }
            }
        }
    }
}
