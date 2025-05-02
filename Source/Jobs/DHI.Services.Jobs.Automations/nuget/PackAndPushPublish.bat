@echo off

if /I "%1"=="yes" goto :CONTINUE
if /I "%GITHUB_ACTIONS%"=="true" goto :CONTINUE

set /P sure=Are you sure you want to push packages to nuget.org (yes/[n])?
if /I not "%sure%"=="yes" (
  echo Aborting.
  goto :EOF
)

:CONTINUE
echo.
echo ===== Building & Pushing DHI.Services.Jobs.Automations NuGet Package =====
echo.

echo on
set configuration=Release

echo Cleaning old packages…
del ..\bin\%configuration%\*.nupkg 2>nul
del ..\bin\%configuration%\*.snupkg 2>nul
echo.

echo [PACK] dotnet pack ..\DHI.Services.Jobs.Automations.csproj --configuration %configuration%
dotnet pack ..\DHI.Services.Jobs.Automations.csproj --configuration %configuration%
echo.

echo [PUSH] dotnet nuget push ..\bin\%configuration%\DHI.Services.Jobs.Automations.*.nupkg -k %NUGET_API_KEY_PUBLISH% -s https://api.nuget.org/v3/index.json
dotnet nuget push ..\bin\%configuration%\DHI.Services.Jobs.Automations.*.nupkg ^
  -k %NUGET_API_KEY_PUBLISH% ^
  -s https://api.nuget.org/v3/index.json
echo.

echo ===== Done =====