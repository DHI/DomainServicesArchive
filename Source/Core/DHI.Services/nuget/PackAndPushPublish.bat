@echo off

:: Skip prompt when run on CI or passed “yes”
if /I "%1"=="yes" goto :CONTINUE
if /I "%GITHUB_ACTIONS%"=="true" goto :CONTINUE

set /P sure=Are you sure you want to push packages to nuget.org (yes/[n])?
if /I not "%sure%"=="yes" (
  echo Aborting.
  goto :EOF
)

:CONTINUE
echo.
echo ===== Building & Pushing DHI.Services NuGet Package =====
echo.

echo on
set configuration=Release

echo Deleting old *.nupkg and *.snupkg in ..\bin\%configuration%\
del ..\bin\%configuration%\*.nupkg 2>nul
del ..\bin\%configuration%\*.snupkg 2>nul
echo.

echo [PACK] dotnet pack ..\DHI.Services.csproj --configuration %configuration%
dotnet pack ..\DHI.Services.csproj --configuration %configuration%
echo.

echo [PUSH] dotnet nuget push ..\bin\%configuration%\DHI.Services.*.nupkg -k %NUGET_API_KEY_PUBLISH% -s https://api.nuget.org/v3/index.json
dotnet nuget push ..\bin\%configuration%\DHI.Services.*.nupkg ^
  -k %NUGET_API_KEY_PUBLISH% ^
  -s https://api.nuget.org/v3/index.json
echo.

echo ===== Done =====