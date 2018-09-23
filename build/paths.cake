
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Common.Tools.NuGet.Pack;
using System.Xml;
using System.Xml.Linq;

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
    private static FilePath VersionFile => "./directory.build/version.props";

    public static FilePath ArtifactCodeCoverageResultFile => "coverage.xml";
    public static DirectoryPath ArtifactCodeCoverageReportDirectory => "coverage";


    public static BuildDirectories GetBuildDirectories(ICakeContext context)
    {
        var rootDir = (DirectoryPath)context.Directory("./");
        var artifactsDir = rootDir.Combine("./artifacts");
        //var artifactsDir = (DirectoryPath)(context.Directory("./artifacts") + context.Directory("v" + semVersion));
        var artifactsTestResults = artifactsDir.Combine("./Test-Results");
        
        var artifactsBinDir = artifactsDir.Combine("bin");
        var artifactsBinFullFx = artifactsBinDir.Combine("net461");        
        var artifactsBinNetStandard20 = artifactsBinDir.Combine("netstandard2.0");        
        var artifactsBinNetCoreapp20 = artifactsBinDir.Combine("netcoreapp2.0");
        var artifactsBinNetCoreapp21 = artifactsBinDir.Combine("netcoreapp2.1");    

        DirectoryPath artifactsNugetsDirectory = artifactsDir.Combine("./Nugets");
        DirectoryPath artifactCodeCoverageReportDirectory = artifactsDir.Combine("./Coverage");
        FilePath artifactCodeCoverageResultFile =Combine(artifactCodeCoverageReportDirectory,"coverage.xml") ;
        
        var toClean = new[] {
                                 artifactsTestResults,
                                 artifactCodeCoverageReportDirectory,
                                 artifactsNugetsDirectory,
                                 artifactsBinNetStandard20,
                                 artifactsBinNetCoreapp20,
                                 artifactsBinNetCoreapp21,
                                 artifactsBinDir
                            };
        return new BuildDirectories(rootDir,
                                    artifactsDir,
                                    artifactsTestResults,
                                    artifactCodeCoverageResultFile,
                                    artifactCodeCoverageReportDirectory,
                                    artifactsNugetsDirectory,
                                    //new DirectoryPath[],//testDirs, 
                                    artifactsBinDir,
                                    artifactsBinNetStandard20,
                                    artifactsBinNetCoreapp20,
                                    artifactsBinNetCoreapp21,
                                    toClean);
    }

    public static BuildPaths GetPaths(ICakeContext context)
    {

        string allProjectsPatternFiles="./src/**/*.csproj";
        string allTestProjectsPatternFiles="./test/**/*.csproj";

        FilePathCollection allProjects = context.GetFiles(allProjectsPatternFiles);
        FilePathCollection unitTestProjects = context.GetFiles("./test/**/*.csproj");
        FilePathCollection integrationTestProjects = context.GetFiles("./test/*IntegrationTests/**/*.csproj");

        FilePathCollection allProjectsToPack =
        context.GetFiles(allProjectsPatternFiles) - context.GetFiles(allTestProjectsPatternFiles);

        // https://github.com/cake-build/cake/issues/1037
        var buildDirectories = GetBuildDirectories(context);
        var testProjects =(new[]{unitTestProjects,integrationTestProjects}).SelectMany(p => p).ToList();

        var buildFiles = new BuildFiles(
            VersionFile,
            SolutionFile,
            buildDirectories.ArtifactCodeCoverageResultFile,
            buildDirectories.ArtifactCodeCoverageReportDirectory,
            allProjectsPatternFiles,
            allProjects.ToList(),
            GetProjectFilesWithNuspecs(context,allProjectsToPack).ToList(),
            testProjects);
        
        return new BuildPaths
        {
            Files = buildFiles,
            Directories = buildDirectories
        };
    }

    private static FilePathCollection GetProjectFilesWithNuspecs(
        ICakeContext context,
        FilePathCollection allProjectsToPack, 
        string path = "**/*.nuspec")
    {
        bool ReadIsPackableFromProject(ICakeContext localContext,string projectFilePath)
        {
            XNamespace xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
            XDocument projDefinition = XDocument.Load(projectFilePath);           
            IEnumerable<XNode> assemblyResultsEnumerable = projDefinition
                ?.Element(xmlns  + "Project")
                ?.Elements(xmlns + "PropertyGroup")
                ?.Elements(xmlns + "IsPackable")
                ;

            var isPackableNode=assemblyResultsEnumerable?.FirstOrDefault();
            if(isPackableNode==null){ return false;}

            var valueIsPackableNode=((XElement) isPackableNode).Value;
            bool.TryParse(valueIsPackableNode,out bool result);
            return result;
        }

        var csprojs=allProjectsToPack;
        var nuspecs = csprojs
            .Where(csproj =>csproj!=null &&  ReadIsPackableFromProject(context,csproj.FullPath)) 
            .Select(csproj =>csproj.ChangeExtension(".nuspec") )
            .Where(filePath =>{ 
                return filePath!=null && System.IO.File.Exists(filePath?.FullPath);
            })
            .Select(x => x)
            .ToList();

        return new FilePathCollection(nuspecs, PathComparer.Default);
    }
}

public class BuildFiles
{
    public FilePath VersionFile { get; private set; }
    public FilePath Solution { get; private set; }
    public FilePath ArtifactCodeCoverageResultFile{ get; private set; }
    public DirectoryPath ArtifactCodeCoverageReportDirectory{ get; private set; }

    public string AllProjectsPatternFiles { get; private set; }
    public ICollection<FilePath> AllProjects { get; private set; }
    public ICollection<FilePath> AllNuspecsProjects { get; private set; }
    public ICollection<FilePath> TestProjects { get; private set; }

    public BuildFiles(FilePath versionFile,
                      FilePath solution,
                      FilePath artifactCodeCoverageResultFile,
                      DirectoryPath artifactCodeCoverageReportDirectory,
                      string allProjectsPatternFiles,
                      ICollection<FilePath> allProjects,
                      ICollection<FilePath> allNuspecsProjects,
                      ICollection<FilePath> testProjects)
    {
        VersionFile=versionFile;
        Solution = solution;
        ArtifactCodeCoverageResultFile=artifactCodeCoverageResultFile;
        ArtifactCodeCoverageReportDirectory=artifactCodeCoverageReportDirectory;
        AllProjectsPatternFiles=allProjectsPatternFiles;
        AllProjects=allProjects;
        AllNuspecsProjects=allNuspecsProjects;
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
    public DirectoryPath ArtifactNugetsDirectory{ get; private set; }
    //public ICollection<DirectoryPath> TestDirs { get; private set; }

    public    DirectoryPath ArtifactsBinDir{ get; private set; }
    public    DirectoryPath ArtifactsBinNetStandard20{ get; private set; }
    public    DirectoryPath ArtifactsBinNetCoreapp20{ get; private set; }
    public    DirectoryPath ArtifactsBinNetCoreapp21{ get; private set; }
    public ICollection<DirectoryPath> ToClean { get; private set; }

    public BuildDirectories(
        DirectoryPath rootDir,
        DirectoryPath artifacts,
        DirectoryPath artifactsTestResults,
        FilePath artifactCodeCoverageResultFile,
        DirectoryPath artifactCodeCoverageReportDirectory,
        DirectoryPath artifactNugetsDirectory,
        DirectoryPath artifactsBinDir,
        DirectoryPath artifactsBinNetStandard20,
        DirectoryPath artifactsBinNetCoreapp20,
        DirectoryPath artifactsBinNetCoreapp21,
        ICollection<DirectoryPath> toClean)
    {
        RootDir = rootDir;
        Artifacts = artifacts;
        ArtifactsTestResults = artifactsTestResults;
        ArtifactCodeCoverageResultFile=artifactCodeCoverageResultFile;
        ArtifactCodeCoverageReportDirectory=artifactCodeCoverageReportDirectory;
        //TestDirs = testDirs;
        ArtifactNugetsDirectory=artifactNugetsDirectory;
        ArtifactsBinDir=artifactsBinDir;
        ArtifactsBinNetStandard20=artifactsBinNetStandard20;
        ArtifactsBinNetCoreapp20=artifactsBinNetCoreapp20;
        ArtifactsBinNetCoreapp21=artifactsBinNetCoreapp21;
        ToClean = toClean;
    }
}