using Microsoft.Xna.Framework.Input;
using ModdingToolkit.Patches;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("NetScriptAssembly")]
namespace ModConfigManager;

public sealed class Initializer : IAssemblyPlugin
{
    public void Initialize()
    {
        PatchManager.RegisterPatches(Patches.GetPatches());
    }

    public void OnLoadCompleted()
    {
        
    }

    public void PreInitPatching()
    {
        
    }

    public void Dispose()
    {
        LuaCsSetup.PrintCsMessage($"MCMC: Dispose called.");
        Barotrauma.SettingsMenu.Instance?.Close();
        Barotrauma.SettingsMenu.Instance = null;
    }
}