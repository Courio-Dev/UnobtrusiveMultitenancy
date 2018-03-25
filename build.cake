
#tool nuget:?package=xunit.runner.console&version=2.3.1
#tool nuget:?package=xunit.runner.visualstudio&version=2.3.1
#tool nuget:?package=OpenCover&version=4.6.519
#tool nuget:?package=ReportGenerator&version=3.1.2
#tool nuget:?package=GitVersion.CommandLine&version=3.6.5

#load build/paths.cake
#load build/urls.cake
#load build/parameters.cake


///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////




// Target - The task you want to start. Runs the Default task if not specified.
var target = Argument<string>("Target", "Default");
// Configuration - The build configuration (Debug/Release) to use.
// 1. If command line parameter parameter passed, use that.
// 2. Otherwise if an Environment variable exists, use that.
var configuration =
    HasArgument("Configuration") ? Argument<string>("Configuration") :
    EnvironmentVariable("Configuration") != null ? EnvironmentVariable("Configuration") : "Release";

// A directory path to an Artifacts directory.
var artifactsDirectory = MakeAbsolute(Directory("./artifacts"));

var codeCoverageReportPath = Argument<FilePath>("CodeCoverageReportPath", "coverage.zip");
var packageOutputPath = Argument<DirectoryPath>("PackageOutputPath", "packages");


var parameters = BuildParameters.GetParameters(Context,target,configuration);
var paths = BuildPaths.GetPaths(Context, parameters);

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    // Executed BEFORE the first task.
	Information("Running tasks...");  

    if (!DirectoryExists(paths.Directories.Artifacts))
    {
        CreateDirectory(paths.Directories.Artifacts);
    }

    if (!DirectoryExists(paths.Directories.ArtifactsTestResults))
    {
        CreateDirectory(paths.Directories.ArtifactsTestResults);
    }

    if (!DirectoryExists(paths.Directories.ArtifactCodeCoverageReportDirectory))
    {
        CreateDirectory(paths.Directories.ArtifactCodeCoverageReportDirectory);
    }
 
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////


Task("Clean")
    .Does(() => 
{
    CleanDirectory(paths.Directories.ArtifactsTestResults);
    CleanDirectory(paths.Directories.ArtifactCodeCoverageReportDirectory);
    CleanDirectory(paths.Directories.Artifacts);
    CleanDirectories("./**/obj/*.*");
    CleanDirectories($"./**/bin/{configuration}/*.*");
    CleanDirectories(paths.Directories.ToClean);
});


// NuGet restore packages for .NET Framework projects (and .NET Core projects)
Task("NuGet-Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore(paths.Files.Solution.ToString());
    });

// NuGet restore packages for .NET Core projects only
Task("DotNet-Core-Package-Restore")
    .IsDependentOn("NuGet-Restore")
    .Does(() =>
{
    DotNetCoreBuild(paths.Files.Solution.ToString(), new DotNetCoreBuildSettings
    {
        Configuration = parameters.Configuration,
        ArgumentCustomization = arg => arg.AppendSwitch("/p:DebugType","=","Full")
    });
});
/*
Task("Create-Version-Info")
    .Does(() =>
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
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings {
        Configuration = configuration,
        NoRestore = true,
        ArgumentCustomization = args => args
                .Append("--no-restore")
                .AppendSwitch("/p:DebugType","=","Full")
                ,
    };
    DotNetCoreBuild(paths.Files.Solution.ToString(), settings);
});


// Look under a 'Tests' folder and run dotnet test against all of those projects.
// Then drop the XML test results file in the Artifacts folder at the root.
Task("Run-Unit-tests")  
    .Does(() =>
    {
        foreach(var project in paths.Files.TestProjects)
        {
            Information("Testing project " + project);
            DotNetCoreTest(
                project.ToString(),
                new DotNetCoreTestSettings()
                {
                    Configuration = configuration,
                    NoBuild = true
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
    .WithFilter("+[*]* -[*.UnitsTests]*");
 
    foreach(var project in paths.Files.TestProjects)
    {
        try 
        {

             Information("Testing project " + MakeAbsolute(project).ToString());
            var projectFile = MakeAbsolute(project).ToString();
            var dotNetTestSettings = new DotNetCoreTestSettings
            {
               /* Configuration = "Debug",
                NoBuild = true,
                ArgumentCustomization = args => args
                        .Append("--no-restore")
                        .AppendSwitch("/p:DebugType","=","Full")
                        ,
                        */
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
    /*
    ReportGenerator(
        paths.Directories.ArtifactCodeCoverageResultFile,
        Paths.Directories.ArtifactCodeCoverageReportDirectory,
        new ReportGeneratorSettings
        {
            ReportTypes = new[] { ReportGeneratorReportType.Html }
        }
    );

    Zip(Paths.CodeCoverageReportDirectory,MakeAbsolute(codeCoverageReportPath));
    */
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
    });


Task("Default")  
    .IsDependentOn("BuildAndTest")
    ;

RunTarget(target);