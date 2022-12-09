using Path = Barotrauma.IO.Path;

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
            ApplicationMode.Client => "/bin/Client",
            ApplicationMode.Server => "/bin/Server",
            _ => "/bin/Client"   //default to client mode
        };

    public static List<string> GetAllAssemblyPathsInPackages(ApplicationMode mode)
    {
        List<ContentPackage> scannedPackages = new();
        List<string> dllPaths = new();
        // I'm not sure why we concat EnPkg.ALL packages list here but this is what LuaCs does.
        // I'm too lazy to check if it was in error or some packages are missing from the AllPackages list.
        foreach (ContentPackage package in 
                 ContentPackageManager.AllPackages.Concat(ContentPackageManager.EnabledPackages.All))
        {
            if (scannedPackages.Contains(package))
                continue;
            scannedPackages.Add(package);
            // Add always load packages
            dllPaths.AddRange(FindAssembliesFilePaths(
                Path.Combine(
                    $"{Path.GetFullPath(Path.GetDirectoryName(package.Path)).Replace('\\', '/')}", 
                    GetApplicationModSubDir(mode), 
                    "/Forced")
                ));
            // Add enabled-only load packages
            if (ContentPackageManager.EnabledPackages.All.Contains(package))
            {
                dllPaths.AddRange(FindAssembliesFilePaths(
                    Path.Combine(
                        $"{Path.GetFullPath(Path.GetDirectoryName(package.Path)).Replace('\\', '/')}", 
                        GetApplicationModSubDir(mode), 
                        "/Standard")
                ));
            }
        }

        return dllPaths;
    }
}