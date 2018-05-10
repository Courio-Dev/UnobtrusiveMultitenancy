public static BuildVersion ReadVersionNumberFromProject(ICakeContext context,
 FilePath versionFile)
{
    var xmlPeekSettings = new XmlPeekSettings
    {
        Namespaces = new Dictionary<string, string>{
            { "msbuild", "http://schemas.microsoft.com/developer/msbuild/2003" }
        }
    };
    var versionNode = "/msbuild:Project/msbuild:PropertyGroup/msbuild:Version";
    var versionPrefixNode = "/msbuild:Project/msbuild:PropertyGroup/msbuild:VersionPrefix";
    var versionSuffixNode = "/msbuild:Project/msbuild:PropertyGroup/msbuild:VersionSuffix";
    string versionValue=context.XmlPeek(versionFile, versionNode,xmlPeekSettings);
    string versionPrefixValue=context.XmlPeek(versionFile, versionPrefixNode,xmlPeekSettings);
    versionPrefixValue=versionPrefixValue.Replace("$(Version)",versionValue);
    string versionSuffixValue=context.XmlPeek(versionFile, versionSuffixNode,xmlPeekSettings);

    BuildVersion buildVersion=BuildVersion.Calculate(
        context,
        versionPrefixValue,
        versionSuffixValue);
    context.Information(buildVersion.VersionPrefix);
    context.Information(buildVersion.VersionSuffix);
    
    return buildVersion;
}


public class BuildVersion
{
    public string VersionPrefix { get; private set; }

    public string VersionSuffix { get; private set; }
    public string Milestone { get; private set; }
    public string CakeVersion { get; private set; }

    public static BuildVersion Calculate(
        ICakeContext context,
        string versionPrefix,
        string versionSuffix)
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        string milestone = null;

        var cakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();

        return new BuildVersion
        {
            VersionPrefix = versionPrefix,
            VersionSuffix = versionSuffix,
            Milestone = milestone,
            CakeVersion = cakeVersion
        };
    }

    public static string ReadSolutionInfoVersion(ICakeContext context)
    {
        var solutionInfo = context.ParseAssemblyInfo("./src/SolutionInfo.cs");
        if (!string.IsNullOrEmpty(solutionInfo.AssemblyVersion))
        {
            return solutionInfo.AssemblyVersion;
        }
        throw new CakeException("Could not parse version.");
    }
}