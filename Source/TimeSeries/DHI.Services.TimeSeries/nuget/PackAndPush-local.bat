@@echo OFF
echo Pushing to local package repo at "%HOME%/.nuget/packages"
echo ON

del ..\bin\%configuration%\*.nupkg 2>nul
del ..\bin\%configuration%\*.snupkg 2>nul
dotnet pack ..\DHI.Services.TimeSeries.csproj --configuration %configuration% > %logfile% 2>&1
dotnet nuget push ..\bin\%configuration%\DHI.Services.TimeSeries.*.nupkg --source "%HOME%/.nuget/packages" >> %logfile% 2>&1

notepad %logfile%
