using ModdingToolkit.Patches;

namespace ModdingToolkit;

internal sealed class Bootloader : ACsMod
{
    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsLoaded { get; private set; }

    public Bootloader()
    {
        DebugConsole.LogError($"ModConfigManager: Starting...");
        LoadAssemblies();
        LoadPatches();
        IsLoaded = true;
    }

    private void LoadAssemblies()
    {
        LuaCsSetup.PrintCsMessage("ModConfigManager: Loading Assembly Plugins...");
#if SERVER
        List<string> pluginDllPaths = PluginHelper.GetAllAssemblyPathsInPackages(ApplicationMode.Server);
#else
        List<string> pluginDllPaths = PluginHelper.GetAllAssemblyPathsInPackages(ApplicationMode.Client);
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
#warning TODO: Remove debug statement.
            LuaCsSetup.PrintCsMessage($"MCM: Loading ACL: DATA: {loadedAcl?.FilePath}");
            LuaCsSetup.PrintCsMessage($"MCM: ACL STATE: {alss.ToString()}");

            if (alss == AssemblyManager.AssemblyLoadingSuccessState.Success)
            {
#warning TODO: Remove debug statement.
                foreach (Type type in loadedAcl.PluginTypes)
                {
                    LuaCsSetup.PrintCsMessage($"ACL Types: {type.Name}");
                }
                
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
            LuaCsSetup.PrintGenericError("ModConfigManager: ERROR: Unable to load plugins.");
        }

    }

    private void LoadPatches()
    {
        #warning TODO: IMPL
    }

    private void UnloadPatches()
    {
        AssemblyManager.BeginDispose();
        while (!AssemblyManager.FinalizeDispose())
        {
            System.Threading.Thread.Sleep(10);
        }
    }

    private void UnloadAssemblies()
    {
        
    }
    
    public override void Stop()
    {
        UnloadPatches();
        UnloadPatches();
    }
}