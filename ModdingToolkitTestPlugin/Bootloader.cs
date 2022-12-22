using Microsoft.Xna.Framework.Input;
using ModdingToolkit.Config;

namespace ModdingToolkitTestPlugin;

public class Bootloader : IAssemblyPlugin
{
    private IConfigControl? cc;
    private IConfigEntry<string>? ce_string;
    private IConfigEntry<float>? ce_float;
    private IConfigEntry<int>? ce_int;
    private IConfigList? cl;
    private IConfigRangeInt? icri;
    private IConfigRangeFloat? icrf;

    public void Initialize()
    {
        cc = ConfigManager.AddConfigKeyOrMouseBind(
            "TestEntry", 
            "ModdingTK",
            new KeyOrMouse(Keys.A),
            () => {}
            );

        ce_float = ConfigManager.AddConfigEntry(
            "TestEntry00",
            "ModdingTK",
            20.0f
        );
        
        ce_int = ConfigManager.AddConfigEntry(
            "TestEntry01",
            "ModdingTK",
            20
        );
        
        ce_string = ConfigManager.AddConfigEntry(
            "TestEntry02",
            "ModdingTK",
            "MyValue"
        );

        cl = ConfigManager.AddConfigList(
            "TestEntry03",
            "ModdingTK",
            "03",
            new List<string> { "01", "02", "03", "04", "05" }
        );

        icri = ConfigManager.AddConfigRangeInt(
            "TestEntry04",
            "ModdingTK",
            10, 0, 20, 21
            );

        icrf = ConfigManager.AddConfigRangeFloat(
            "TestEntry05",
            "ModdingTK",
            10f, 0f, 100f, 101
        );

        ConfigManager.AddConfigBoolean(
            "TestEntry06",
            "ModdingTK",
            false);
    }

    public void OnLoadCompleted()
    {
        LuaCsSetup.PrintCsError($"MTTP: KeyBind value: {cc?.GetStringValue()}");
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