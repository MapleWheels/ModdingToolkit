using Microsoft.Xna.Framework.Input;
using ModdingToolkit.Config;

namespace ModdingToolkitTestPlugin;

public class Bootloader : IAssemblyPlugin
{
    private IConfigControl? cc;
    private IConfigEntry<string> ce_string;
    private IConfigEntry<float> ce_float;
    private IConfigEntry<int> ce_int;
    private IConfigList cl;
    private IConfigRangeInt icri;
    private IConfigRangeFloat icrf;

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

        for (int i = 6; i < 50; i++)
        {
            ConfigManager.AddConfigRangeFloat(
                $"TestEntry{i:D2}",
                "ModdingTK",
                10f, 0f, 100f, 101
            );
        }

        for (int j = 0; j < 20; j++)
        {
            for (int i = 0; i < 50; i++)
            {
                ConfigManager.AddConfigRangeFloat(
                    $"TestEntry{i:D2}",
                    $"ModdingTK{j:D2}",
                    10f, 0f, 100f, 101
                );
            }
        }
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