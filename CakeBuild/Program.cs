using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Json;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public const string ProjectName = "PantheonWars";
    public string BuildConfiguration { get; set; }
    public string Version { get; set; }
    public string ModId { get; set; }
    public string Name { get; }

    public bool SkipJsonValidation { get; set; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        BuildConfiguration = context.Argument("configuration", "Release");
        SkipJsonValidation = context.Argument("skipJsonValidation", false);
        var modInfo = context.DeserializeJsonFromFile<ModInfo>($"../{ProjectName}/{ProjectName}/modinfo.json");
        Version = modInfo.Version;
        Name = modInfo.ModID;
    }
}

[TaskName("ValidateJson")]
public sealed class ValidateJsonTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Information("Validating JSON files...");

        var jsonFiles = context.GetFiles("./PantheonWars/assets/**/*.json");
        var hasErrors = false;

        foreach (var file in jsonFiles)
        {
            try
            {
                var content = System.IO.File.ReadAllText(file.FullPath);
                JToken.Parse(content);
                context.Information($"✓ Valid: {file.GetFilename()}");
            }
            catch (Exception ex)
            {
                context.Error($"✗ Invalid JSON in {file.GetFilename()}: {ex.Message}");
                hasErrors = true;
            }
        }

        if (hasErrors)
        {
            throw new Exception("JSON validation failed!");
        }

        context.Information("All JSON files are valid!");
    }
}

[TaskName("Build")]
[IsDependentOn(typeof(ValidateJsonTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Information($"Building PantheonWars in {context.BuildConfiguration} mode...");

        context.DotNetBuild("./PantheonWars/PantheonWars.csproj", new DotNetBuildSettings
        {
            Configuration = context.BuildConfiguration
        });
    }
}

[TaskName("Package")]
[IsDependentOn(typeof(BuildTask))]
public sealed class PackageTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.EnsureDirectoryExists("../Releases");
        context.CleanDirectory("../Releases");
        context.EnsureDirectoryExists($"../Releases/{context.Name}");
        context.CopyFiles($"../{BuildContext.ProjectName}/bin/{context.BuildConfiguration}/Mods/mod/publish/*",
            $"../Releases/{context.Name}");
        context.CopyDirectory($"../{BuildContext.ProjectName}/{BuildContext.ProjectName}/assets", $"../Releases/{context.Name}/assets");
        context.CopyFile($"../{BuildContext.ProjectName}/{BuildContext.ProjectName}/modinfo.json", $"../Releases/{context.Name}/modinfo.json");
        context.Zip($"../Releases/{context.Name}", $"../Releases/{context.Name}_{context.Version}.zip");
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(PackageTask))]
public class DefaultTask : FrostingTask
{
}