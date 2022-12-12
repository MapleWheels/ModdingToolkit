using ModdingToolkit.Client;
using ModdingToolkit.Patches;

namespace ModdingToolkit;

internal sealed class Bootloader : ACsMod
{
    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsLoaded { get; private set; }

    public Bootloader()
    {
        DebugConsole.LogError($"ModConfigManager: Starting...");
        Init();
    }

    private void Init()
    {
        PluginHelper.LoadAssemblies();
        LoadPatches();
        RegisterCommands();
        ConsoleCommands.ReloadAllCommands();
        IsLoaded = true;
    }

    private void RegisterCommands()
    {
        ConsoleCommands.RegisterCommand(
            "cl_reloadassemblies", 
            "Reloads all assemblies and their plugins.",
            args =>
            {
                UnloadPatches();
                PluginHelper.UnloadAssemblies();
                PluginHelper.LoadAssemblies();
                LoadPatches();
            });
        
        ConsoleCommands.RegisterCommand(
            "cl_unloadassemblies", 
            "Unloads all assemblies and their plugins.",
            args =>
            {
                UnloadPatches();
                PluginHelper.UnloadAssemblies();
            });
    }

    private void LoadPatches()
    {
        PatchManager.OnPatchStateUpdate += OnPatchStateUpdate;
        PatchManager.BuildPatchList();
        PatchManager.Load();
    }

    private void OnPatchStateUpdate(bool isLoaded)
    {
        if (isLoaded)
        {
            LuaCsSetup.PrintCsMessage("ModConfigManager: Patches Loaded.");
        }
        else
        {
            LuaCsSetup.PrintCsMessage("ModConfigManager: Patches Unloaded.");
        }
    }

    private void UnloadPatches()
    {
        PatchManager.Unload();
        PatchManager.OnPatchStateUpdate -= OnPatchStateUpdate;
    }

    public override void Stop()
    {
        ConsoleCommands.UnloadAllCommands();
        UnloadPatches();
        
        PluginHelper.UnloadAssemblies();
        IsLoaded = false;
    }
}