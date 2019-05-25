#!/usr/bin/pwsh

Function Test-PomMissing {
    if (-not $env:POMODORO_REPOS) {
        Write-Host "Please set the $env:POMODORO_REPOS to the location of this repo."
        return $true
    }   
}

Function Use-PomDirectory {
    if (Test-PomMissing) { RETURN }
    Set-Location "$env:POMODORO_REPOS/PersonalTracker.Api"
}

Function Start-PmsResourceManagement {
    param(
        # [Parameter(
        #     Mandatory=$true, 
        #     HelpMessage="Starts IdentiyManagement microservices.",
        #     ParameterSetName="Individual")]
        # [ValidateSet(
        #     "pomodoro-pgsql",
        #     "pomodoro-idserver",
        #     "pomodoro-identity", 
        #     "pomodoro-resource", 
        #     "pomodoro-privilege", 
        #     "pomodoro-reverse-proxy",
        #     "watch-pomo-rapi",
        #     "pomo-ping-rapi",
        #     "pomodoro-client"
        # )] 
        # [string]$Container,
        [Parameter(
            Mandatory=$false, 
            HelpMessage="Use 'dotnet run'")]
        [switch]$Runs
    )

    if (Test-PomMissing) { RETURN }

    Write-Host "Starting pms-resourcemanagement..."

    if ($Runs) {
        # Cannot attach a debugger, but can have the app auto reload during development.
        # https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/dotnet-docker-dev-in-container.md
        docker run `
            --name pms-resourcemanagement `
            --rm -it `
            -p 8080:8080 `
            --network pomodoro-net `
            --entrypoint "/bin/bash" `
            -v $env:POMODORO_REPOS/ResourceManagement/ResourceManagement/src/:/app/ResourceManagement/ResourceManagement/src/ `
            -v $env:POMODORO_REPOS/ResourceManagement/ResourceManagement/secrets/:/app/ResourceManagement/ResourceManagement/secrets/ `
            -v $env:POMODORO_REPOS/ResourceManagement/ResourceManagement.Domain/src/:/app/ResourceManagement/ResourceManagement.Domain/src/ `
            -v $env:POMODORO_REPOS/ResourceManagement/ResourceManagement.Domain.DAL/src/:/app/ResourceManagement/ResourceManagement.Domain.DAL/src/ `
            -v $env:POMODORO_REPOS/ResourceManagement/ResourceManagement.UnitTests/src/:/app/ResourceManagement/ResourceManagement.UnitTests/src/ `
            pms-resourcemanagement
#            pms-resourcemanagement "run" "--project" "ResourceManagement"
    } else {
        # Cannot attach a debugger, but can have the app auto reload during development.
        # https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/dotnet-docker-dev-in-container.md
        docker run `
        --name pms-resourcemanagement `
        --rm -it `
        -p 2005:8080 `
        --network pomodoro-net `
        -v $env:POMODORO_REPOS/ResourceManagement/ResourceManagement/src/:/app/ResourceManagement/ResourceManagement/src/ `
        -v $env:POMODORO_REPOS/ResourceManagement/ResourceManagement/secrets/:/app/ResourceManagement/ResourceManagement/secrets/ `
        -v $env:POMODORO_REPOS/ResourceManagement/ResourceManagement.Domain/src/:/app/ResourceManagement/ResourceManagement.Domain/src/ `
        -v $env:POMODORO_REPOS/ResourceManagement/ResourceManagement.Domain.DAL/src/:/app/ResourceManagement/ResourceManagement.Domain.DAL/src/ `
        -v $env:POMODORO_REPOS/ResourceManagement/ResourceManagement.UnitTests/src/:/app/ResourceManagement/ResourceManagement.UnitTests/src/ `
        pms-resourcemanagement

    }
}
Function Build-PmsResourceManagement {
    <#
    .SYNOPSIS
        Builds the docker container related to the pomodor project.
    .DESCRIPTION
        Builds the docker container related to the pomodor project.
    .PARAMETER Image
        One of the valid images for the pomodoro project
    .EXAMPLE
    .NOTES
        Author: Phillip Scott Givens
    #>    
    param(
        [Parameter(Mandatory=$false)]
        [ValidateSet(
            "docker", 
            "microk8s.docker",
            "azure"
            )] 
        [string]$Docker="docker"
    )

    if (Test-PomMissing) { RETURN }
    if ($Docker) {
        Set-Alias dkr $Docker -Option Private
    }

    $buildpath = "$env:POMODORO_REPOS/ResourceManagement"
    dkr build `
        -t pms-resourcemanagement `
        -f "$buildpath/watch.Dockerfile" `
        "$buildpath/.."
}

Function Update-PmsResourceManagement {
    if (Test-PomMissing) { RETURN }

    $MyPSModulePath = "{0}/.local/share/powershell/Modules" -f (ls -d ~)
    mkdir -p $MyPSModulePath/MsResourceManagement

    Write-Host ("Linking {0}/ResourceManagement/scripts/MsResourceManagement.psm1 to {1}/MsResourceManagement/" -f $env:POMODORO_REPOS,  $MyPSModulePath)
    ln -s $env:POMODORO_REPOS/ResourceManagement/scripts/MsResourceManagement.psm1  $MyPSModulePath/MsResourceManagement/MsResourceManagement.psm1

    Write-Host "Force import-module PomodorEnv"
    Import-Module -Force MsResourceManagement -Global

}



Export-ModuleMember -Function Build-PmsResourceManagement
Export-ModuleMember -Function Start-PmsResourceManagement
Export-ModuleMember -Function Update-PmsResourceManagement