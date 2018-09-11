using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management.Automation;
using DiskCleanup.Internal;

namespace DiskCleanup.Commands
{
    [Cmdlet(VerbsCommon.Get, "UserProfile")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class GetUserProfileCommand : PSCmdlet, IDynamicParameters
    {
        [Parameter]
        public SwitchParameter ShowHidden { get; set; }

        public object GetDynamicParameters()
        {            
            var profileNames = UserProfileDirectory.GetUserProfiles(ShowHidden).Select(p => p.Name).ToArray();

            var attributeCollection = new Collection<Attribute>
            {
                new ParameterAttribute {Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true},
                new ValidateSetAttribute(profileNames)
            };

            var runtimeDictionary = new RuntimeDefinedParameterDictionary
            {
                {"UserName", new RuntimeDefinedParameter("UserName", typeof(string), attributeCollection)}
            };

            return runtimeDictionary;
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            MyInvocation.BoundParameters.TryGetValue("UserName", out var boundParameter);
            var userNames = boundParameter is string str ? new[] {str} : (string[]) boundParameter;

            if (userNames == null)
            {
                var profiles = UserProfileDirectory.GetUserProfiles(ShowHidden);

                WriteObject(profiles.ToArray().ToPSObject());
            }
            else
                WriteObject(userNames
                    .Select(u => new DirectoryInfo(Path.Combine(UserProfileDirectory.DirectoryInfo.FullName, u)))
                    .ToArray()
                    .ToPSObject());
        }
    }
}
