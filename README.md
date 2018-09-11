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

This example enables NTFS compression on a single directory and all subdirectories. The standard error stream of compact.exe will be redirected to the current PS host.
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
