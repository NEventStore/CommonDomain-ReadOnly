@echo off
SET PATH=%PATH%;C:\WINDOWS\Microsoft.NET\Framework\V3.5;

if not exist output ( mkdir output )

echo Compiling
msbuild /nologo /verbosity:quiet src/CommonDomain.sln /p:Configuration=Release /t:Clean
msbuild /nologo /verbosity:quiet src/CommonDomain.sln /p:Configuration=Release

echo Merging

SET FILES_TO_MERGE=
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain/bin/Release/CommonDomain.dll"
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain.Core/bin/Release/CommonDomain.Core.dll"
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain.Persistence/bin/Release/CommonDomain.Persistence.dll"
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain.Persistence.EventStore/bin/Release/CommonDomain.Persistence.EventStore.dll"
SET FILES_TO_MERGE=%FILES_TO_MERGE% "src/proj/CommonDomain.Persistence.EventStore/bin/Release/EventStore.dll"

bin\ilmerge-bin\ILMerge.exe /keyfile:src/CommonDomain.snk /v2 /xmldocs /out:output/CommonDomain.dll %FILES_TO_MERGE%

echo Cleaning
msbuild /nologo /verbosity:quiet src/CommonDomain.sln /p:Configuration=Release /t:Clean

echo Done