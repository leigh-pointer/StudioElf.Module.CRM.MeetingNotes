@echo off
REM Usage: release.cmd net10.0
REM Auto-detects the .nuspec file in the current directory.
set TargetFramework=%1
for %%f in (*.nuspec) do set NuspecFile=%%f
set ProjectName=%NuspecFile:.nuspec=%
del "*.nupkg"
"..\..\..\..\oqtane.framework\oqtane.package\FixProps.exe"
"..\..\..\..\oqtane.framework\oqtane.package\nuget.exe" pack %NuspecFile% -Properties targetframework=%TargetFramework%;projectname=%ProjectName%
XCOPY "*.nupkg" "..\..\..\..\oqtane.framework\Oqtane.Server\Packages\" /Y
