# Usage

## Standalone

### Installation

Copy the `wt` executable file to a location on your PATH.

### Comamnds

Run the executable to display the latest help and options information:

    > wt 

## Git Extension

### Installation

Either

1. Place a copy of the `git-wt` executable in your PATH
2. Run the installation option to add as a Git Alias, running from the current location of `git-wt` executable

    wt install

for example

    Add alias wta to /Users/mminns/projects/bitbucket.org/mminns/git-worktree-ext/src/wt/bin/Debug/net9.0/osx-arm64/git-wt? [y/N]:y
    Alias wta:/Users/mminns/projects/bitbucket.org/mminns/git-worktree-ext/src/wt/bin/Debug/net9.0/osx-arm64/git-wt added.

### Commands

Run the git command to display the latest help and options information:

    > git wt 


For example

    Description:
    Git Worktree Extension

    Usage:
    git-wt [command] [options]

    Options:
    --version       Show version information
    -?, -h, --help  Show help and usage information

    Commands:
    open       Open a new of existing worktree
    close      Close a worktree branch
    home       Get the home worktree folder
    list       List open worktrees
    status     View the worktree status, configuration and active worktrees
    set        [Config] Set the parent worktree path
    unset      [Config] Unset the parent worktree path
    install    [Config] Install Git-Wt
    uninstall  [Config] Uninstall Git-Wt
    new        [Plumbing] Create a new worktree branch
    remove     [Plumbing] Remove a worktree branch


# Development

## Pre-requsites

1. DotNet 9+


## Build

From the Solution folder

    dotnet build


## Dependency Resolution Problems

If https://packages.atlassian.com is configured as a NuGet source, there seems to be an issue with downloading nuget dependencies, resulting in an error like this:

    > dotnet add package Microsoft.NETCore.App.Host.osx-arm64

    ...

    info :   GET https://packages.atlassian.com/ui/FindPackagesById()?id='Microsoft.NETCore.App.Host.osx-arm64'&semVerLevel=2.0.0
    info :   OK https://api.nuget.org/v3/registration5-gz-semver2/microsoft.netcore.app.host.osx-arm64/index.json 337ms
    info :   Gone https://packages.atlassian.com/ui/FindPackagesById()?id='Microsoft.NETCore.App.Host.osx-arm64'&semVerLevel=2.0.0 405ms
    error: Failed to fetch results from V2 feed at 'https://packages.atlassian.com/ui/FindPackagesById()?id='Microsoft.NETCore.App.Host.osx-arm64'&semVerLevel=2.0.0' with following message : Response status code does not indicate success: 410 (Gone).
    error:   Response status code does not indicate success: 410 (Gone).

### Dependency Resolution Workaround

Force the dependency resolution by being explicit about the package version, e.g.

    dotnet add package Microsoft.NETCore.App.Host.win-x64 --version 9.0.2

# Publishing

    dotnet publish

This relies on the the configuration in [wt.csproj](./src/scproj) to generate a self contained executable.


    <!-- START Single File Publishing options -->
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <PublishTrimmed>true</PublishTrimmed>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
    <!-- END Single File Publishing options -->


[Single-file deployment on Microsoft.com](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview?tabs=cli#other-considerations)

## To build for each architecture

Publish for the host workstation architecture

    dotnet publish

Publich for a specific architecture

    dotnet publish -r win-x64

The supported runtimes can be found in the [.NET RID Catalog](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)

### Troubleshooting Publishing

NB. Due to the [dependency resolution issue](#Dependency-Resultion-Problems) when publishing to a new architecture for the first time it may fail with an error similar to this 

    > dotnet publish -r osx-arm64

    .../git-worktree-ext/src/wt/wt.csproj : error NU1301:
      Failed to retrieve information about 'Microsoft.AspNetCore.App.Runtime.osx-arm64' from remote source 'https://packages.atlassian.com/FindPackagesById()?id='Microsoft.AspNetCore.App.Runtime.osx-arm64'&semVerLevel=2.0.0'.
        Response status code does not indicate success: 404.
    ...
    
    Restore failed with 4 error(s) in 6.0s

It can be resolved with the same [workaround](#Dependency-Resolution-Workaround)




# Misc

https://gitbetter.substack.com/p/automate-repetitive-tasks-with-custom 

Clipboard code https://github.com/soheilpro/Clipboard 
