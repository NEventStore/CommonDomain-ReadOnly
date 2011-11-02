@ECHO OFF
SET FRAMEWORK_PATH=C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319
SET PATH=%PATH%;%FRAMEWORK_PATH%;

:target_config
SET TARGET_CONFIG=Release
IF x==%1x GOTO framework_version
SET TARGET_CONFIG=%1

:framework_version
SET FRAMEWORK_VERSION=v4.0
SET OUTPUT=output-net40
SET ILMERGE_VERSION=v4,%FRAMEWORK_PATH%
SET LIB_DIRECTORY=4.0

:build
if exist %OUTPUT% ( rmdir /s /q %OUTPUT% )
if exist %OUTPUT% ( rmdir /s /q %OUTPUT% )
mkdir %OUTPUT%
mkdir %OUTPUT%\bin

echo Compiling / Target: %FRAMEWORK_VERSION% / Config: %TARGET_CONFIG%
msbuild /nologo /verbosity:quiet src/CommonDomain.sln /p:Configuration=%TARGET_CONFIG% /t:Clean
msbuild /nologo /verbosity:quiet src/CommonDomain.sln /p:Configuration=%TARGET_CONFIG% /property:TargetFrameworkVersion=%FRAMEWORK_VERSION%

echo Merging
SET FILES_TO_MERGE=
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain/bin/%TARGET_CONFIG%/CommonDomain.dll"
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain.Core/bin/%TARGET_CONFIG%/CommonDomain.Core.dll"
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain.Persistence/bin/%TARGET_CONFIG%/CommonDomain.Persistence.dll"
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain.Persistence.EventStore/bin/%TARGET_CONFIG%/CommonDomain.Persistence.EventStore.dll"
bin\ilmerge-bin\ILMerge.exe /keyfile:src/CommonDomain.snk /xmldocs /targetplatform:%ILMERGE_VERSION% /out:%OUTPUT%/bin/CommonDomain.dll %FILES_TO_MERGE%

echo Copying
mkdir %OUTPUT%\doc
copy doc\*.* %OUTPUT%\doc

echo Cleaning
msbuild /nologo /verbosity:quiet src/CommonDomain.sln /p:Configuration=%TARGET_CONFIG% /t:Clean

echo Done