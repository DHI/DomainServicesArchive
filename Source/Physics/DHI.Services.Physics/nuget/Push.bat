@set version=3.0.0
@set configuration=debug
..\..\..\nuget.exe pack DHI.Services.Physics.nuspec -version %version% -properties configuration=%configuration% > Push.log 2>&1
..\..\..\nuget.exe push DHI.Services.Physics.%version%.nupkg dhi-nuget-admin -Source http://dhi-nuget-server.azurewebsites.net/nuget >> Push.log 2>&1
..\..\..\nuget.exe push -Source https://dhigroup.pkgs.visualstudio.com/_packaging/MFL/nuget/v3/index.json -ApiKey VSTS DHI.Services.Physics.%version%.nupkg >> Push.log 2>&1
