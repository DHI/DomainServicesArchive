@set version=4.0.2
@set configuration=release
@set vswherePath="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

@setlocal enabledelayedexpansion
@for /f "usebackq tokens=*" %%i in (`!vswherePath! -latest -prerelease -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  @set vsInstallDir=%%i
)

"%vsInstallDir%"\Common7\IDE\devenv.exe ..\..\DHI.Services.Jobs.Web.sln /clean %configuration%  /out Build.log
"%vsInstallDir%"\Common7\IDE\devenv.exe ..\..\DHI.Services.Jobs.Web.sln /build %configuration%  /out Build.log

..\..\..\nuget.exe pack DHI.Services.Jobs.Web.nuspec -properties version=%version% > Push.log 2>&1
..\..\..\nuget.exe push DHI.Services.Jobs.Web.%version%.nupkg dhi-nuget-admin -Source http://dhi-nuget-server.azurewebsites.net/nuget >> Push.log 2>&1
..\..\..\nuget.exe push -Source https://dhigroup.pkgs.visualstudio.com/_packaging/MFL/nuget/v3/index.json -ApiKey VSTS DHI.Services.Jobs.Web.%version%.nupkg >> Push.log 2>&1
