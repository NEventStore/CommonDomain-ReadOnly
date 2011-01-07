@ECHO OFF
SET FRAMEWORK_PATH=C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319
SET PATH=%PATH%;%FRAMEWORK_PATH%;

:target_config
SET TARGET_CONFIG=Release
IF x==%1x GOTO framework_version
SET TARGET_CONFIG=%1

:framework_version
SET FRAMEWORK_VERSION=v4.0
SET ILMERGE_VERSION=v4,%FRAMEWORK_PATH%
SET LIB_DIRECTORY=4.0
IF x==%2x GOTO build
SET FRAMEWORK_VERSION=%2
SET ILMERGE_VERSION=%3
SET LIB_DIRECTORY=3.5

:build
if exist output ( rmdir /s /q output )
if exist output ( rmdir /s /q output )
mkdir output
mkdir output\bin

echo Compiling / Target: %FRAMEWORK_VERSION% / Config: %TARGET_CONFIG%
msbuild /nologo /verbosity:quiet src/CommonDomain.sln /p:Configuration=%TARGET_CONFIG% /t:Clean
msbuild /nologo /verbosity:quiet src/CommonDomain.sln /p:Configuration=%TARGET_CONFIG% /property:TargetFrameworkVersion=%FRAMEWORK_VERSION%

echo Merging
SET FILES_TO_MERGE=
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain/bin/%TARGET_CONFIG%/CommonDomain.dll"
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain.Core/bin/%TARGET_CONFIG%/CommonDomain.Core.dll"
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain.Persistence/bin/%TARGET_CONFIG%/CommonDomain.Persistence.dll"
bin\ilmerge-bin\ILMerge.exe /keyfile:src/CommonDomain.snk /xmldocs /targetplatform:%ILMERGE_VERSION% /out:output/bin/CommonDomain.dll %FILES_TO_MERGE%
copy "lib\eventstore-bin\.NET %LIB_DIRECTORY%\*.*" "output\bin\"

echo Copying
mkdir output\doc
copy doc\*.* output\doc
copy "lib\eventstore-bin\doc\license.txt" "output\doc\EventStore license.txt"
copy "lib\eventstore-bin\doc\protobuf-net license.txt" "output\doc\protobuf-net license.txt"

echo Cleaning
msbuild /nologo /verbosity:quiet src/CommonDomain.sln /p:Configuration=%TARGET_CONFIG% /t:Clean

echo Done