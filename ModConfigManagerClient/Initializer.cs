[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("NetScriptAssembly")]
namespace ModConfigManager;

public sealed class Initializer : IAssemblyPlugin
{
    public void Initialize()
    {
        LuaCsSetup.PrintCsMessage($"MCMC: Init called.");
    }

    public void OnLoadCompleted()
    {
        LuaCsSetup.PrintCsMessage($"MCMC: OnLoadCompleted called.");
    }

    public PluginInfo GetPluginInfo()
    {
        LuaCsSetup.PrintCsMessage($"MCMC: GetPluginInfo called.");
        return new PluginInfo("ModConfigManagerClient", "0.0.0.0", ImmutableArray<string>.Empty);
    }

    public void Dispose()
    {
        LuaCsSetup.PrintCsMessage($"MCMC: Dispose called.");
        Barotrauma.SettingsMenu.Instance?.Close();
        Barotrauma.SettingsMenu.Instance = null;
    }
}