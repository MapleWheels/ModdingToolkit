using System.IO;
using Path = System.IO.Path;

namespace ModdingToolkit;

public static class PluginHelper
{
    public static readonly string PluginAsmFileSuffix = "*.plugin.dll";
    private static readonly object _OpsLock = new object();
    public static bool IsInit { get; private set; } = false;
    
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
        if (!IsInit)
            InitHooks();
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
            try
            {
                string baseForcedSearchPath = Path.GetFullPath(
                    Path.Combine(
                        Path.GetDirectoryName(package.Path)!,
                        GetApplicationModSubDir(mode),
                        "Forced")
                );
                string baseStandardSearchPath = Path.GetFullPath(
                    Path.Combine(
                        Path.GetDirectoryName(package.Path)!,
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
            catch(Exception e)
            {
                Utils.Logging.PrintError($"PluginHelper::GetAllAssemblyPathsInPackages() | Unable to parse the package: {package.Name} | Details: {e.Message}");
            }
        }
        return dllPaths;
    }
    
    [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining)]
    internal static void UnloadAssemblies()
    {
        if (!IsInit)
            InitHooks();
        
        lock (_OpsLock)
        {
            int count = 100;
            AssemblyManager.BeginDispose();
            while (!AssemblyManager.FinalizeDispose() && count > 0)
            {
                count--;
                System.Threading.Thread.Sleep(10);
            }
        }
    }
    
    [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining)]
    internal static void LoadAssemblies()
    {
        if (!IsInit)
            InitHooks();
        
        lock (_OpsLock)
        {
            LuaCsSetup.PrintCsMessage("ModConfigManager: Loading Assembly Plugins...");
#if SERVER
        List<string> pluginDllPaths = PluginHelper.GetAllAssemblyPathsInPackages(ApplicationMode.Server);
#else
            List<string> pluginDllPaths = GetAllAssemblyPathsInPackages(ApplicationMode.Client);
#endif
            foreach (string path in pluginDllPaths)
            {
                LuaCsSetup.PrintCsMessage($"Found Assembly Path: {path}");
            }
            List<AssemblyManager.LoadedACL> loadedAcls = new();
            foreach (string dllPath in pluginDllPaths)
            {
                AssemblyManager.AssemblyLoadingSuccessState alss
                    = AssemblyManager.LoadAssembliesAndPluginsFromLocation(dllPath, out var loadedAcl);
                if (alss == AssemblyManager.AssemblyLoadingSuccessState.Success)
                {
                    if (loadedAcl is not null)
                        loadedAcls.Add(loadedAcl);
                }
            }

            if (AssemblyManager.LoadPlugins(out var pluginInfos))
            {
                foreach (var pluginInfo in pluginInfos)
                {
                    LuaCsSetup.PrintCsMessage($"ModConfigManager: Loaded Assembly Plugin: {pluginInfo.ModName}, Version: {pluginInfo.Version}");
                }
            }
            else
            {
                Utils.Logging.PrintError("ModConfigManager: ERROR: Unable to load plugins.");
            }
        }
    }

    private static void OnAssemblyLoadedHandle(Assembly assembly)
    {
        Utils.Logging.PrintMessage($"Modding TK: Registering Assembly {assembly.FullName}");
        Barotrauma.ReflectionUtils.AddNonAbstractAssemblyTypes(assembly);
    }

    private static void InitHooks()
    {
        if (IsInit)
            return;
        AssemblyManager.OnException += AssemblyManagerOnException;
        AssemblyManager.OnAssemblyLoaded += OnAssemblyLoadedHandle;
        AssemblyManager.OnAssemblyUnloading += AssemblyManagerOnAssemblyUnloading;
        IsInit = true;
    }

    private static void AssemblyManagerOnAssemblyUnloading(Assembly assembly)
    {
        Barotrauma.ReflectionUtils.RemoveAssemblyFromCache(assembly);
    }

    private static void ReleaseHooks()
    {
        if (!IsInit)
            return;
        AssemblyManager.OnAssemblyUnloading -= AssemblyManagerOnAssemblyUnloading;
        AssemblyManager.OnAssemblyLoaded -= OnAssemblyLoadedHandle;
        AssemblyManager.OnException -= AssemblyManagerOnException;
        IsInit = false;
    }

    private static void AssemblyManagerOnException(string arg1, Exception arg2)
    {
        Utils.Logging.PrintError($"{arg1} | Exception Details: {arg2.Message}");
    }
}