using ModdingToolkit.Patches;

namespace ModdingToolkit;

internal sealed class Bootloader : ACsMod
{
    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsLoaded { get; private set; }
    private Assembly SettingsMenuAssembly;

    public Bootloader()
    {
        DebugConsole.LogError($"ModConfigManager: Loaded.");
        LoadAssemblies();
        PatchSettingsMenu();
        IsLoaded = true;
    }

    private void LoadAssemblies()
    {
        
    }

    private void PatchSettingsMenu()
    {
        #warning TODO: IMPL
    }
    
    public override void Stop()
    {
        Func<bool> disposeCompleted = AssemblyManager.BeginDispose();
        while (!disposeCompleted.Invoke())
        {
            Thread.Sleep(50); //Halt mod unloading thread until assemblies are confirmed to be unloaded.  
        } 
    }
}