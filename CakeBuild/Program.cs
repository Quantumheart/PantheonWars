using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Newtonsoft.Json.Linq;

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
    public string BuildConfiguration { get; set; }
    public string Version { get; set; }
    public string ModId { get; set; }
    
    public BuildContext(ICakeContext context)
        : base(context)
    {
        BuildConfiguration = context.Argument("configuration", "Release");
        
        // Read modinfo.json to get version and modid
        var modinfoPath = "./PantheonWars/assets/modinfo.json";
        if (System.IO.File.Exists(modinfoPath))
        {
            var modinfo = JObject.Parse(System.IO.File.ReadAllText(modinfoPath));
            Version = modinfo["version"]?.ToString() ?? "0.1.0";
            ModId = modinfo["modid"]?.ToString() ?? "pantheonwars";
        }
        else
        {
            Version = "0.1.0";
            ModId = "pantheonwars";
        }
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
        context.Information("Packaging mod...");
        
        var releaseDir = "./Release";
        var tempDir = $"{releaseDir}/temp";
        var outputZip = $"{releaseDir}/{context.ModId}_{context.Version}.zip";
        
        // Clean and create directories
        context.CleanDirectory(releaseDir);
        context.CreateDirectory(tempDir);
        
        // Copy mod files
        var buildOutput = $"./PantheonWars/bin/{context.BuildConfiguration}/net7.0";
        context.CopyFile($"{buildOutput}/PantheonWars.dll", $"{tempDir}/PantheonWars.dll");
        context.CopyFile($"{buildOutput}/PantheonWars.pdb", $"{tempDir}/PantheonWars.pdb");
        
        // Copy assets
        context.CopyDirectory("./PantheonWars/assets", $"{tempDir}/assets");
        
        // Create zip
        context.Zip(tempDir, outputZip);
        
        // Clean temp directory
        context.DeleteDirectory(tempDir, new DeleteDirectorySettings
        {
            Recursive = true,
            Force = true
        });
        
        context.Information($"✓ Package created: {outputZip}");
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(PackageTask))]
public class DefaultTask : FrostingTask
{
}
