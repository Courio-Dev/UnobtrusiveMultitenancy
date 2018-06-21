
#load ./paths.cake
#load ./urls.cake
#load ./parameters.cake
#load ./version.cake


public class BuildParameters
{
    public string Target { get; private set; }
    public string Configuration { get; private set; }

    public string PreReleaseSuffix { get; private set; }
    public int BuildNumber { get; private set; }

    public bool UseDotNetVsTest { get; set; }
    public bool UseDotNetTest { get; set; }
    public string TargetFramework { get; private set; }
    public string TargetFrameworkFull { get; private set; }

    public BuildPaths Paths { get; private set; }
    public BuildVersion Version { get; private set; }
    
    
    /*public void Initialize(ICakeContext context)
    {
        context.Information($"   {context}");
        var paths = BuildPaths.GetPaths(context, this);

        Paths = BuildPaths.GetPaths(context, this);
        Version = BuildVersion.Calculate(context,paths.Files.VersionFile,PreReleaseSuffix,BuildNumber);
    }*/

    /*private static void Initialize(ICakeContext context,BuildParameters buildParameters)
    {
        context.Information($"   {context}");
        var paths = BuildPaths.GetPaths(context, buildParameters);

        
        Version = BuildVersion.Calculate(context,paths.Files.VersionFile,PreReleaseSuffix,BuildNumber);
    }*/

    public static BuildParameters GetParameters(
        ICakeContext context,
        string target,
        string configuration,
        string preReleaseSuffix,
        int buildNumber)
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        //context.Information($"   {context.Argument<bool>("UseDotNetVsTest", true)}");
        var buildSystem = context.BuildSystem();
        var paths = BuildPaths.GetPaths(context);
        var version = BuildVersion.Calculate(context,paths.Files.VersionFile,preReleaseSuffix,buildNumber);
        var result = new BuildParameters(){
            Target = target,
            Configuration = configuration,
            PreReleaseSuffix = preReleaseSuffix,
            BuildNumber = buildNumber,
            Paths = paths,
            Version = version,
            UseDotNetVsTest = context.Argument<bool>("UseDotNetVsTest", true),
            UseDotNetTest = context.Argument<bool>("UseDotNetTest", false),
            TargetFramework = "netcoreapp2.1",
            TargetFrameworkFull = "netcoreapp2.1"
        };
        //Initialize( context,result);

        return result;
    }
}