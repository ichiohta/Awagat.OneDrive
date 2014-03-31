# PsOneDriveProvider

## Introduction

PsOneDriveProvider is a sample implementation of OneDrive provider for PowerShell, intended to showcase:
* How to implement a PowerShell provider
* How to use RESTful API to access OneDrive

THIS MODULE IS NOT INTENDED FOR PRODUCTION USE.

## How to Install

1. Download the source code (somehow)
2. Open the solution file and build the solution
3. Run Tools\Install.ps1 to install the OneDrive module to to $HOME\Documents\WindowsPowerShell\Modules

## Regsiter Your Application

To execute this sample code, you need to obtain a set of Live SDK client Id and secret by registreing a new application at Live Connect Develoepr Center:

http://dev.live.com

You need to register your application as a mobile client app / desktop client app.

## Configure Your Client Profile

Once you obtain your client Id and secret, configure OneDrive module to use them. 

1. Import-Module Awagat.OneDrive
2. Set-OdClientProfile -Id your-client-id -Secret your-client-secret

## How to Use Provider

1. Import-Module Awagat.OneDrive
2. New-PSDrive -Name drive-name -Provider OneDrive -Root \

## Expected FAQs

### Why can I not copy/move files between the file system and OneDrive providers?

In PowerShell, Copy-Item / Move-Item only works within the same provider. Use Get-Content and Set-Content to copy files between file system and OneDrive providers.

### Why can I not copy files with new names?

In this implementation, you can only specify a destination folder path for Copy-Item and Move-Item, and you cannot include the destination file name in it. This restriction comes from some limitation on how MOVE and COPY method behave in Live SDK REST API.
