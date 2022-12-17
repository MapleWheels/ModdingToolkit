using Microsoft.Xna.Framework.Input;
using ModdingToolkit.Config;

namespace ModdingToolkitTestPlugin;

public class Bootloader : IAssemblyPlugin
{
    private ModdingToolkit.Config.IConfigControl? cc;
    
    public void Initialize()
    {
        cc = ConfigManager.AddConfigKeyOrMouseBind(
            "TestEntry", 
            "ModdingTK",
            new KeyOrMouse(Keys.A),
            IConfigBase.Category.Ignore,
            IConfigBase.NetworkSync.NoSync,
            () => {}
            );
    }

    public void OnLoadCompleted()
    {
        LuaCsSetup.PrintCsError($"MTTP: KeyBind value: {cc?.Value?.Key.ToString()}");
    }

    public PluginInfo GetPluginInfo()
    {
        return new PluginInfo("ModdingToolkit.TestPlugin", "0.0.0.0", ImmutableArray<string>.Empty);
    }

    public void Dispose()
    {
        cc = null;
    }
}