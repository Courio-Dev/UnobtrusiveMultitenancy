

#load "./parameters.cake"

public static class ConstantsPaths
{

    public static FilePath CodeCoverageResultFile => "coverage.xml";
    public static DirectoryPath CodeCoverageReportDirectory => "coverage";
    public static FilePath WebNuspecFile => "src/Web/Web.nuspec";
    public static FilePath WebProjectFile => "src/Web/Web.csproj";
}

public static FilePath Combine(DirectoryPath directory, FilePath file)
{
    return directory.CombineWithFilePath(file);
}

public DirectoryPath VS2017InstallDirectory(ICakeContext context)
{
    var programFilesX86 = context.Environment.GetSpecialPath(SpecialPath.ProgramFilesX86);
    string[] editions  = { "Enterprise", "Professional", "Community" };

    return editions
        .Select(edition => Directory($"{programFilesX86}/Microsoft Visual Studio/2017/{edition}"))
        .FirstOrDefault(path => context.DirectoryExists(path));
}



public class BuildPaths
{
    public BuildFiles Files { get; private set; }
    public BuildDirectories Directories { get; private set; }

    private static FilePath SolutionFile => "./PuzzleCMS.sln";

    public static FilePath ArtifactCodeCoverageResultFile => "coverage.xml";
    public static DirectoryPath ArtifactCodeCoverageReportDirectory => "coverage";


  public static BuildDirectories GetBuildDirectories(ICakeContext context)
    {
        var rootDir = (DirectoryPath)context.Directory("./");
        var artifacts = rootDir.Combine("./artifacts");
        var artifactsTestResults = artifacts.Combine("./Test-Results");
        DirectoryPath artifactCodeCoverageReportDirectory = artifacts.Combine("./Coverage");
        FilePath artifactCodeCoverageResultFile =Combine(artifactCodeCoverageReportDirectory,"coverage.xml") ;
        
/*
        var integrationTestsDir = rootDir.Combine(context.Directory("NETCoreCodeCoverage.Tests.Integration"));
        var unitTestsDir = rootDir.Combine(context.Directory("NETCoreCodeCoverage.Tests.Unit"));
        var mainProjectDir = rootDir.Combine(context.Directory("NETCoreCodeCoverage"));
*/
        /*var testDirs = new []{
                                unitTestsDir,
                                integrationTestsDir
                            };
                            */
        var toClean = new[] {
                                 artifactsTestResults,
                                 artifactCodeCoverageReportDirectory
                                 //integrationTestsDir.Combine("bin"),
                                 //integrationTestsDir.Combine("obj"),
                                 //unitTestsDir.Combine("bin"),
                                 //unitTestsDir.Combine("obj"),
                                 //mainProjectDir.Combine("bin"),
                                 //mainProjectDir.Combine("obj"),
                            };
        return new BuildDirectories(rootDir,
                                    artifacts,
                                    artifactsTestResults,
                                    artifactCodeCoverageResultFile,
                                    artifactCodeCoverageReportDirectory,
                                    //new DirectoryPath[],//testDirs, 
                                    toClean);
    }

    public static BuildPaths GetPaths(ICakeContext context, BuildParameters parameters)
    {
        FilePathCollection unitTestProjects = context.GetFiles("./test/**/*.csproj");
        FilePathCollection integrationTestProjects = context.GetFiles("./test/*IntegrationTests/**/*.csproj");

        var configuration =  parameters.Configuration;
        var buildDirectories = GetBuildDirectories(context);
        var testProjects =(new[]{unitTestProjects,integrationTestProjects}).SelectMany(p => p).ToList();

        var buildFiles = new BuildFiles(
            SolutionFile,
            buildDirectories.ArtifactCodeCoverageResultFile,
            buildDirectories.ArtifactCodeCoverageReportDirectory,
            testProjects);
        
        return new BuildPaths
        {
            Files = buildFiles,
            Directories = buildDirectories
        };
    }

  
}

public class BuildFiles
{
    public FilePath Solution { get; private set; }
    public FilePath ArtifactCodeCoverageResultFile{ get; private set; }
    public DirectoryPath ArtifactCodeCoverageReportDirectory{ get; private set; }
    public ICollection<FilePath> TestProjects { get; private set; }

    public BuildFiles(FilePath solution,
                      FilePath artifactCodeCoverageResultFile,
                      DirectoryPath artifactCodeCoverageReportDirectory,
                      ICollection<FilePath> testProjects)
    {
        Solution = solution;
        ArtifactCodeCoverageResultFile=artifactCodeCoverageResultFile;
        ArtifactCodeCoverageReportDirectory=artifactCodeCoverageReportDirectory;
        TestProjects = testProjects;
    }
}

public class BuildDirectories
{
    public DirectoryPath RootDir { get; private set; }
    public DirectoryPath Artifacts { get; private set; }
    public DirectoryPath ArtifactsTestResults { get; private set; }
    public FilePath ArtifactCodeCoverageResultFile{ get; private set; }
    public DirectoryPath ArtifactCodeCoverageReportDirectory{ get; private set; }
    //public ICollection<DirectoryPath> TestDirs { get; private set; }
    public ICollection<DirectoryPath> ToClean { get; private set; }

    public BuildDirectories(
        DirectoryPath rootDir,
        DirectoryPath artifacts,
        DirectoryPath artifactsTestResults,
        FilePath artifactCodeCoverageResultFile,
        DirectoryPath artifactCodeCoverageReportDirectory,
        //ICollection<DirectoryPath> testDirs,
        ICollection<DirectoryPath> toClean)
    {
        RootDir = rootDir;
        Artifacts = artifacts;
        ArtifactsTestResults = artifactsTestResults;
        ArtifactCodeCoverageResultFile=artifactCodeCoverageResultFile;
        ArtifactCodeCoverageReportDirectory=artifactCodeCoverageReportDirectory;
        //TestDirs = testDirs;
        ToClean = toClean;
    }
}