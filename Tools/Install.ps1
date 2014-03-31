#requires -version 3.0

function Install-Module {

  param (
    [Parameter(Mandatory = $false)]
    [string] $Build        = 'Debug',
    [Parameter(Mandatory = $true)]
    [string] $ProjectFolder,
    [Parameter(Mandatory = $false)]
    [string] $TargetFolder = "$HOME\Documents\WindowsPowerShell\Modules\Awagat.OneDrive"
  )

  $assemblyName = 'Awagat.OneDrive.dll'
  $assemblyPath = "$projectFolder\Awagat.OneDrive\bin\$Build\$assemblyName"

  New-Item -Type Directory -Path $TargetFolder -Force |
    Out-Null

  Copy-Item "$projectFolder\Awagat.OneDrive\PowerShell\Scripts\*" $TargetFolder
  Copy-Item $assemblyPath $TargetFolder

} # End of Install-Module

$projectFolder = Split-Path (Split-Path $MyInvocation.MyCommand.Path)

Install-Module -ProjectFolder $projectFolder
