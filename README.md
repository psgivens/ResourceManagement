# ResourceManagement
Identity Management for infrastructure

# Environment setup 

* git PersonalTracker.Api, Common.FSharp, and ResourceManagement
* link PersonalTracker.Api/scripts/PomodoroEnv.psm1 into pwsh modules directory
* link ResourceManagement/scripts/MsIdentityManagment.psm1 into pwsh modules directory

# Build

1. Build pomdooro-dotnet-stage -- Use PersonalTracker.Api/scripts/PomodoroEnv.pms1:Build-Image 
2. Build pms-resourcemanagement -- MsResourceManagement.psm1

# Build performance

https://stackoverflow.com/questions/45763755/dotnet-core-2-long-build-time-because-of-long-restore-time