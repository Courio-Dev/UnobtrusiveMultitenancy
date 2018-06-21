public class BuildVersion
{
    public string Version { get; private set; }
    public string VersionPrefix { get; private set; }
    public string VersionSuffix { get; private set; }

    public string Milestone { get; private set; }
    public string CakeVersion { get; private set; }

    public static BuildVersion Calculate(
        ICakeContext context,
        FilePath versionFile,
        string preReleaseSuffixParam,
        int buildNumberParam
        /*, BuildParameters parameters*/)
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        var cakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();
        (string versionPrefixValue,string versionSuffixValue) = ReadVersionNumberFromProject(context,versionFile);

        var tempVersionSuffix = versionSuffixValue;
        var preReleaseSuffix =string.IsNullOrEmpty(preReleaseSuffixParam) ? null : versionSuffixValue;

        var finalVersionPrefix = versionPrefixValue;
        var finalVersionSuffix = string.IsNullOrEmpty(preReleaseSuffix) ? null :$"-{preReleaseSuffix}-{buildNumberParam.ToString("D4")}";

        var finalVersion= $"{finalVersionPrefix}{finalVersionSuffix}";

        return new BuildVersion()
        {
            Version = finalVersion,
            VersionPrefix = finalVersionPrefix,
            VersionSuffix = finalVersionSuffix,
            CakeVersion = cakeVersion
        };
    }
    private static (string versionPrefixValue, string versionSuffixValue) ReadVersionNumberFromProject(ICakeContext context,FilePath versionFile)
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

        return (versionPrefixValue:versionPrefixValue, versionSuffixValue:versionSuffixValue); 
        /*BuildVersion buildVersion=BuildVersion.Calculate(
            context,
            versionPrefixValue,
            versionSuffixValue);
        context.Information(buildVersion.VersionPrefix);
        context.Information(buildVersion.VersionSuffix);
        
        return buildVersion;*/
    }
}