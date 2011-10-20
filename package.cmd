@echo off

:: Major.Minor
set /p VERSION=Enter version (e.g. 2.0): 
:: YYdayOfYear.BuildNumber
set /p BUILD=Enter a build (e.g. 11234): 
set /p MATURITY=Enter maturity (e.g. Alpha, Beta, RC, Release, etc.): 

if exist packages ( rmdir /s /q "output\packages" )
mkdir "output\packages"

:: for some reason nuget doesn't like adding files located in directories underneath it.  v1.4 bug?
::move "publish-net40" "bin\nuget"
::move "publish-net35" "bin\nuget"

"src/.nuget/nuget.exe" Pack "nuget/CommonDomain.nuspec" -Version "%VERSION%.%BUILD%" -OutputDirectory "output/packages" -BasePath "."

::rmdir /s /q bin\nuget\publish-net40