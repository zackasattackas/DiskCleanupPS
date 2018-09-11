# DiskCleanupPS
A binary PowerShell module containing cmdlets for performing various disk cleanup tasks.

The target framework is .NET 3.5, so the module should work on systems with .NET 3.5 and PowerShell 2.0 or higher.

## Tested Operating Systems
* Windows 10
* Windows Server 2008 R2 (SP1)

## Exported Cmdlets

```PowerShell
Get-ChildFileSystemItem
Get-FileSystemItemSize
Get-UserProfile
Invoke-CCleaner
New-NtfsCompressionOptions
Remove-TempFiles
Set-NtfsCompression
```

## Examples
I have yet to write the PowerShell help documentation for each cmdlet, but the following examples and the source code should give you a good idea of how each cmdlet works and what they return.


This example enables NTFS compression on a single directory and all subdirectories. The standard error stream of `compact.exe` will be redirected to the current PS host.
```PowerShell
$options = New-NtfsCompressionOptions -EnableCompression -Recurse -ContinueOnError
Set-NtfsCompression -Path C:\Windows\SoftwareDistribution -Options $options -RedirectStandardError
```

This example outputs the directory size of each user profile on the system (including hidden profiles).
```PowerShell
Get-UserProfile -ShowHidden | Get-FileSystemItemSize
```

This example downloads, unzips and executes CCleaner Portable with the `/Auto` command line option.
```PowerShell
Invoke-CCleaner -AllowDownload
```

This example outputs the `System.IO.FileSystemInfo` objects in the current directory that have the `System.IO.FileAttributes.Hidden` or `System.IO.FileAttributes.Archive` bit flags set.
```PowerShell
Get-ChildFileSystemItem -AttributeFilter 'Hidden','System'
```

## Installation

### Temporary install (current session)
1. Clone and build the solution
2. Run `Import-Module "<your path here>\DiskCleanupPS.psd1"`

### Permanent install (always loaded)
1. Create a new folder in one of your `$env:PSModulePath` directories (e.g. %USERPROFILE%\Documents\WindowsPowerShell\Modules or %PROGRAMFILES%\WindowsPowerShell\Modules) named DiskCleanupPS.
2. Clone and build the solution.
3. Copy the output files to the new module folder.
4. Open a new PowerShell console or ISE host and run `Get-Command -Module DiskCleanupPS`, you should see the cmdlets listed above.

## Remarks

For those already comfortable with PowerShell scripting concepts (i.e. Functions, Pipeline processing), writing a compiled cmdlet can be a great intro to the C# language, and can provide deeper insight into the PowerShell runtime. The [Windows PowerShell SDK](https://docs.microsoft.com/en-us/powershell/developer/windows-powershell) documentation has several examples to get you started, and pretty good documentation to help you along the way.
