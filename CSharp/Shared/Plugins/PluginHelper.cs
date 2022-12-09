using System.IO;
using Path = System.IO.Path;

namespace ModdingToolkit;

public static class PluginHelper
{
    public static readonly string PluginAsmFileSuffix = "*.plugin.dll";
    
    public static List<string> FindAssembliesFilePaths(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            return new List<string>();
        }
        return Directory.GetFiles(rootPath, PluginAsmFileSuffix, SearchOption.AllDirectories).ToList();
    }

    public static string GetApplicationModSubDir(ApplicationMode mode) =>
        mode switch
        {
            ApplicationMode.Client => "bin/Client",
            ApplicationMode.Server => "bin/Server",
            _ => "bin/Client"   //default to client mode
        };

    public static List<string> GetAllAssemblyPathsInPackages(ApplicationMode mode)
    {
#warning TODO: Remove debug command.
        LuaCsSetup.PrintCsMessage($"MCM: Scanning packages...");
        List<ContentPackage> scannedPackages = new();
        List<string> dllPaths = new();
        // Sometimes ALL packages doesn't include packages downloaded from the server. So we need to search twice.
        foreach (ContentPackage package in 
                 ContentPackageManager.AllPackages.Concat(ContentPackageManager.EnabledPackages.All))
        {
            if (scannedPackages.Contains(package))
                continue;
            scannedPackages.Add(package);
            string baseForcedSearchPath = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(package.Path),
                    GetApplicationModSubDir(mode), 
                    "Forced")
                );
            string baseStandardSearchPath = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(package.Path),
                    GetApplicationModSubDir(mode), 
                    "Standard")
            );
            // Add always load packages
            dllPaths.AddRange(FindAssembliesFilePaths(baseForcedSearchPath));
            // Add enabled-only load packages
            if (ContentPackageManager.EnabledPackages.All.Contains(package))
            {
                dllPaths.AddRange(FindAssembliesFilePaths(baseStandardSearchPath));
            }
        }
        return dllPaths;
    }
}