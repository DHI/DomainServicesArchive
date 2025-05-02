@echo OFF
echo Pushing to local package repo at "%HOME%/.nuget/packages"
echo ON

@set configuration=release
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /format:list') do set datetime=%%I
set logfile=%datetime:~0,8%-%datetime:~8,6%.log

del ..\bin\%configuration%\*.nupkg 2>nul
del ..\bin\%configuration%\*.snupkg 2>nul
dotnet pack ..\DHI.Services.csproj --configuration %configuration% > %logfile% 2>&1
dotnet nuget push ..\bin\%configuration%\DHI.Services.*.nupkg -s %HOME%/.nuget/packages >> %logfile% 2>&1

notepad %logfile%

:END
