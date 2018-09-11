using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management.Automation;

namespace DiskCleanup.Commands
{
    [Cmdlet("Out", "Warning")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class OutWarningCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public string Message { get; set; }

        [Parameter]
        [AllowNull]
        public string FilePath { get; set; }

        [Parameter]
        [ValidateSet("Unicode", "ASCII")]
        public string Encoding { get; set; } = "Unicode";

        [Parameter]
        public SwitchParameter Append { get; set; }

        [Parameter]
        public SwitchParameter NoNewLine { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteWarning(Message);

            if (string.IsNullOrEmpty(FilePath))
                return;

            var mode = Append ? FileMode.Append : FileMode.Create;
            var encoding = Encoding == "Unicode" ? System.Text.Encoding.Unicode : System.Text.Encoding.ASCII;

            using (var writer = new StreamWriter(File.Open(FilePath, mode, FileAccess.Write), encoding))
                if (NoNewLine)
                    writer.Write(Message);
                else
                    writer.WriteLine(Message);
        }
    }
}
