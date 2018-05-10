
#tool nuget:?package=xunit.runner.console&version=2.3.1
#tool nuget:?package=xunit.runner.visualstudio&version=2.3.1
#tool nuget:?package=OpenCover&version=4.6.519
#tool nuget:?package=ReportGenerator&version=3.1.2
#tool nuget:?package=GitVersion.CommandLine&version=3.6.5

#load build/paths.cake
#load build/urls.cake
#load build/parameters.cake
#load build/version.cake

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////
// Target - The task you want to start. Runs the Default task if not specified.
var target = Argument<string>("Target", "Default");
var configuration =
    HasArgument("Configuration") ? Argument<string>("Configuration") :
    EnvironmentVariable("Configuration") != null ? EnvironmentVariable("Configuration") : "Release";

// The build number to use in the version number of the built NuGet packages.
// There are multiple ways this value can be passed, this is a common pattern.
// 1. If command line parameter parameter passed, use that.
// 2. Otherwise if running on AppVeyor, get it's build number.
// 3. Otherwise if running on Travis CI, get it's build number.
// 4. Otherwise if an Environment variable exists, use that.
// 5. Otherwise default the build number to 0.
var preReleaseSuffix =
    HasArgument("PreReleaseSuffix") ? Argument<string>("PreReleaseSuffix") :
    (AppVeyor.IsRunningOnAppVeyor && AppVeyor.Environment.Repository.Tag.IsTag) ? null :
    EnvironmentVariable("PreReleaseSuffix") != null ? EnvironmentVariable("PreReleaseSuffix") :
    "beta";
var buildNumber = HasArgument("BuildNumber") ?
    Argument<int>("BuildNumber") :
    AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Number :
    EnvironmentVariable("BuildNumber") != null ? int.Parse(EnvironmentVariable("BuildNumber")) :
    0;

var parameters = BuildParameters.GetParameters(Context,target,configuration);
var paths = BuildPaths.GetPaths(Context, parameters);

BuildVersion buildVersion=ReadVersionNumberFromProject(Context,paths.Files.VersionFile);
 var revision = buildNumber.ToString("D4");
var versionElement = string.IsNullOrEmpty(preReleaseSuffix) ? null : $"-{preReleaseSuffix}-{revision}";

var finalVersionPrefix = buildVersion.VersionPrefix;
var finalVersionSuffix = buildVersion.VersionSuffix.Replace("-*", versionElement);

var finalVersion= $"{finalVersionPrefix}{finalVersionSuffix}";

 var msBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("Version", finalVersion)
            .WithProperty("AssemblyVersion",finalVersionPrefix)
            .WithProperty("FileVersion", finalVersionPrefix)
            ;
var dotNetCoreBuildSettings = new DotNetCoreBuildSettings {
        Configuration = configuration,
        NoRestore = true,
        VersionSuffix =  finalVersionSuffix,
        ArgumentCustomization = args => args
                .Append("--no-restore")
                .AppendSwitch("/p:DebugType","=","Full")
                ,
        MSBuildSettings=msBuildSettings
};

static DirectoryPath GetOutputArtifactFromProjectFile(DirectoryPath artifactsBinFolder,FilePath csprojFile)
{
    var fileWithoutExtension=System.IO.Path.GetFileNameWithoutExtension(csprojFile.FullPath);
    return artifactsBinFolder.Combine(fileWithoutExtension);
}

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
	Information("Running tasks...");  
    Information($"finalVersion {finalVersion}");  
    EnsureDirectoryExists(paths.Directories.Artifacts);
    EnsureDirectoryExists(paths.Directories.ArtifactsTestResults);
    EnsureDirectoryExists(paths.Directories.ArtifactCodeCoverageReportDirectory);
    EnsureDirectoryExists(paths.Directories.ArtifactNugetsDirectory);

    EnsureDirectoryExists(paths.Directories.ArtifactsBinDir);
    EnsureDirectoryExists(paths.Directories.ArtifactsBinNetStandard20);
    EnsureDirectoryExists(paths.Directories.ArtifactsBinNetCoreapp20);
    EnsureDirectoryExists(paths.Directories.ArtifactsBinNetCoreapp21);
});

Teardown(ctx =>Information("Finished running tasks."));
///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////
Task("Clean")
    .Does(() => 
{
    CleanDirectory(paths.Directories.ArtifactCodeCoverageReportDirectory);
    CleanDirectory(paths.Directories.ArtifactNugetsDirectory);
    CleanDirectory(paths.Directories.ArtifactsTestResults);

    CleanDirectory(paths.Directories.ArtifactsBinNetStandard20);
    CleanDirectory(paths.Directories.ArtifactsBinNetCoreapp20);
    CleanDirectory(paths.Directories.ArtifactsBinNetCoreapp21);
    CleanDirectory(paths.Directories.ArtifactsBinDir);
    CleanDirectories(GetDirectories("**/bin"));
    CleanDirectories(GetDirectories("**/obj"));
    CleanDirectories(paths.Directories.ToClean);
});


// NuGet restore packages for .NET Framework projects (and .NET Core projects)
Task("NuGet-Restore")
    .IsDependentOn("Clean")
    .Does(() =>NuGetRestore(paths.Files.Solution.ToString()));
// NuGet restore packages for .NET Core projects only
Task("DotNet-Core-Package-Restore")
    .IsDependentOn("NuGet-Restore")
    .Does(() =>
    { 
        DotNetCoreRestore(paths.Files.Solution.ToString(), new DotNetCoreRestoreSettings
        {
            Verbosity = DotNetCoreVerbosity.Minimal,            
        });
    });
/*
Task("Create-Version-Info").Does(() =>
{
    CreateAssemblyInfo(File("AssemblyVersionInfo.cs"), new AssemblyInfoSettings
    {
        Version = parameters.AssemblyVersion,
        FileVersion = parameters.Version,
        InformationalVersion = parameters.FullVersion
    });
});
*/
Task("Build")
    .Does(() =>DotNetCoreBuild(paths.Files.Solution.ToString(), dotNetCoreBuildSettings));
// Look under a 'Tests' folder and run dotnet test against all of those projects.
// Then drop the XML test results file in the Artifacts folder at the root.
Task("Run-Unit-tests")  
    .Does(() =>
    {
        foreach(var project in paths.Files.TestProjects)
        {
            Information("Testing project " + project);
            DotNetCoreTest(project.ToString(),new DotNetCoreTestSettings()
            {
                Configuration = configuration,
                NoBuild = true,
                NoRestore = true
            });
        }
    });

Task("Test-OpenCover")
    .IsDependentOn("Build")
    //.IsDependentOn("Run-Unit-tests")
    .WithCriteria(() => BuildSystem.IsLocalBuild || BuildSystem.IsRunningOnAppVeyor)
    .Does(() =>
{
    var success = true;
    var openCoverSettings = new OpenCoverSettings
    {
        OldStyle = true,
        MergeOutput = true,
        Register = "user",
        SkipAutoProps = true
    }
    .WithFilter("+[*]* -[*.UnitsTests]*")
    .WithFilter("-[xunit.*]*")
    .WithFilter("-[*.*Tests]*")
    //.ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
    ;
 
    foreach(var project in paths.Files.TestProjects)
    {
        try 
        {

             Information("Testing project " + MakeAbsolute(project).ToString());
            var projectFile = MakeAbsolute(project).ToString();
            var dotNetTestSettings = new DotNetCoreTestSettings
            {
               /* Configuration = "Debug",*/
               Configuration = configuration,
                NoBuild = true,
                NoRestore = true,
                ArgumentCustomization = args => args
                        .Append("--no-restore")
                        .AppendSwitch("/p:DebugType","=","Full") 
            };
 
            OpenCover(context => context.DotNetCoreTest(projectFile, dotNetTestSettings), 
                                paths.Directories.ArtifactCodeCoverageResultFile, 
                                openCoverSettings
            );
        }
        catch(Exception ex)
        {
            success = false;
            Error("There was an error while running Test-OpenCover", ex);
            throw;
        }
    }
 
    try 
    {
        ReportGenerator(paths.Directories.ArtifactCodeCoverageResultFile, paths.Directories.ArtifactsTestResults);
    }
    catch(Exception ex)
    {
        success = false;
        Error("There was an error while running the Test-OpenCover", ex);
        throw;
    }
    if(success == false)
    {
        throw new CakeException("There was an error while running the Test-OpenCover");
    }
    
});

Task("Report-Coverage")
    .IsDependentOn("Test-OpenCover")
    .WithCriteria(() => BuildSystem.IsLocalBuild)
    .Does(() =>
{
});

Task("Test-VSTest")
    .IsDependentOn("Build")
    .WithCriteria(() => BuildSystem.IsRunningOnVSTS)
    .Does(() =>
{
    VSTest(
        GetFiles($"**/bin/{configuration}/*Tests.dll"),
        new VSTestSettings
        {
            Logger = "trx",
            EnableCodeCoverage = true,
            InIsolation = true,
            TestAdapterPath = $"tools/xunit.runner.visualstudio/build/_common",
            ToolPath = $"{VS2017InstallDirectory(Context)}/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe"
        });
});

Task("Test")
    .IsDependentOn("Run-Unit-tests")
    .IsDependentOn("Test-OpenCover")
    .IsDependentOn("Test-VSTest");

// A meta-task that runs all the steps to Build and Test the app
Task("BuildAndTest")  
    .IsDependentOn("Clean")
    .IsDependentOn("DotNet-Core-Package-Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .Does(() =>
    {
        Information($"Builded and tested successfully");
    })
    .OnError(ex => {
        Error("Build Failed, throwing exception...");
        throw ex;
    });

//////////////////////////////////////////////////////////////////////
// Versionning + Packaging
//////////////////////////////////////////////////////////////////////
Task("Version").Does(() =>{});
Task("Remove-Packages").Does(() =>CleanDirectory(paths.Directories.ArtifactNugetsDirectory));

Task("Copy-Files")
    .DoesForEach(paths.Files.AllNuspecsProjects,projectNuSpecToPack => 
    {
        var nuspecFile = projectNuSpecToPack.FullPath;
        var csprojFile = projectNuSpecToPack.ChangeExtension(".csproj");

        var outputDirectory21 = GetOutputArtifactFromProjectFile(paths.Directories.ArtifactsBinNetCoreapp21,csprojFile);

        // .NET Core
        DotNetCorePublish(csprojFile.FullPath, new DotNetCorePublishSettings
        {
            Framework = "netcoreapp2.1",
            NoRestore = true,
            Configuration = configuration,
            OutputDirectory = outputDirectory21,
            MSBuildSettings = msBuildSettings
        });

        // Copy license
        //CopyFileToDirectory("./LICENSE", outputDirectory21); 

    });
// Run dotnet pack to produce NuGet packages from our projects. Versions the package
// using the build number argument on the script which is used as the revision number 
// (Last number in 1.0.0.0). The packages are dropped in the Artifacts directory.
Task("Package-NuGet")
    .IsDependentOn("BuildAndTest")
    .IsDependentOn("Remove-Packages")
    .IsDependentOn("Version")
    .IsDependentOn("Copy-Files")
    .DoesForEach(paths.Files.AllNuspecsProjects,projectNuSpecToPack => 
    {
        /*
        var nuspecFile = projectNuSpecToPack.FullPath;
        var nuspecContent = System.IO.File.ReadAllText(nuspecFile);
        var nuspecDocument = XDocument.Parse(nuspecContent);
        var ns = XNamespace.Get("http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
        var versionElement = nuspecDocument.Element(ns + "package").Element(ns + "metadata").Element(ns + "version");
        var finalVersion= versionElement.Value.Replace("-*", finalVersionSuffix);
        versionElement.Value = finalVersion;
        System.IO.File.WriteAllText(nuspecFile, nuspecDocument.ToString());
        */

        // .NET Core
        var nuspecFile=projectNuSpecToPack.FullPath;
        var fullBasePath = GetOutputArtifactFromProjectFile(paths.Directories.ArtifactsBinNetCoreapp21,nuspecFile);

        /*DotNetCorePack(projectNuSpecToPack.ChangeExtension(".csproj").FullPath,new DotNetCorePackSettings()
        {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,
            OutputDirectory = paths.Directories.ArtifactNugetsDirectory,
            VersionSuffix = finalVersionSuffix,//finalVersion,
            MSBuildSettings= msBuildSettings,
            ArgumentCustomization = args => args
                        .AppendSwitch("/p:NuspecFile","=",$"{nuspecFile}") 
        });*/
        
        // normal
        NuGetPack(nuspecFile, new NuGetPackSettings {
            Version = finalVersion,
            BasePath = fullBasePath,
            OutputDirectory = paths.Directories.ArtifactNugetsDirectory,
            Symbols = false,
            NoPackageAnalysis = true
        });
        
        
        //System.IO.File.WriteAllText(nuspecFile, nuspecContent);
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")  
    .IsDependentOn("Package-NuGet")
    ;

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);