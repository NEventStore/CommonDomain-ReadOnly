properties {
    $base_directory = Resolve-Path .. 
	$publish_directory = "$base_directory\publish-net40"
	$build_directory = "$base_directory\build"
	$src_directory = "$base_directory\src"
	$output_directory = "$base_directory\output"
	$packages_directory = "$src_directory\packages"

	$sln_file = "$src_directory\CommonDomain.sln"
	$keyfile = "$src_directory/CommonDomain.snk"
	$target_config = "Release"
	$framework_version = "v4.0"
	$version = "0.0.0.0"

	$ilMergeModule.ilMergePath = "$base_directory\bin\ilmerge-bin\ILMerge.exe"
	$nuget_dir = "$src_directory\.nuget"
}

task default -depends Build

task Build -depends Clean, UpdateVersion, Compile

task UpdateVersion {
	$versionAssemblyInfoFile = "$src_directory/proj/VersionAssemblyInfo.cs"
	"using System.Reflection;" > $versionAssemblyInfoFile
	"" >> $versionAssemblyInfoFile
	"[assembly: AssemblyVersion(""$version"")]" >> $versionAssemblyInfoFile
	"[assembly: AssemblyFileVersion(""$version"")]" >> $versionAssemblyInfoFile
}

task Compile {
	exec { msbuild /nologo /verbosity:quiet $sln_file /p:Configuration=$target_config /t:Clean }

	exec { msbuild /nologo /verbosity:quiet $sln_file /p:Configuration=$target_config /p:TargetFrameworkVersion=v4.0 }
}

task Package -depends Build, PackageCommonDomain, PackageDocs {
	move $output_directory $publish_directory
}

task PackageCommonDomain -depends Clean, Compile {
	mkdir "$output_directory\bin" | out-null
	Merge-Assemblies -outputFile "$output_directory\bin\CommonDomain.dll" -keyfile $keyFile -exclude "CommonDomain.*" -files @(
		"$src_directory\proj\CommonDomain\bin\$target_config\CommonDomain.dll"
		"$src_directory\proj\CommonDomain.Core\bin\$target_config\CommonDomain.Core.dll"
		"$src_directory\proj\CommonDomain.Persistence\bin\$target_config\CommonDomain.Persistence.dll"
		"$src_directory\proj\CommonDomain.Persistence.EventStore\bin\$target_config\CommonDomain.Persistence.EventStore.dll"
	)
}

task PackageDocs {
	mkdir "$output_directory\doc"
	copy "$base_directory\doc\*.*" "$output_directory\doc"
}

task Clean {
	Clean-Item $publish_directory -ea SilentlyContinue
    Clean-Item $output_directory -ea SilentlyContinue
}

task NuGetPack -depends Package {
	gci -r -i *.nuspec "$nuget_dir" |% { .$nuget_dir\nuget.exe pack $_ -basepath $base_directory -o $publish_directory -version $version }
}