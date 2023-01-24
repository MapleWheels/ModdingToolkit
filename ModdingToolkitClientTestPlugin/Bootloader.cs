using System.Globalization;
using Microsoft.Xna.Framework.Input;
using ModdingToolkit.Config;
using ModdingToolkit.Networking;

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
    private IConfigEntry<float>? net_ce_float;
    private IConfigEntry<string>? net_ce_string;
    private IConfigList? net_cl;

    public void Initialize()
    {
        #region STANDARD-MENU_TESTS

        cc = ConfigManager.AddConfigKeyOrMouseBind(
            "TestEntry", 
            "ModdingTK",
            new KeyOrMouse(Keys.A),
            () => {}
        );

        ce_float = ConfigManager.AddConfigEntry(
            "TestEntry00",
            "ModdingTK",
            20.0f,
            displayData: new DisplayData(DisplayName: "Test Entry 00", DisplayModName: "Modding TK", DisplayCategory: "Category 0") 
        );
        
        ce_int = ConfigManager.AddConfigEntry(
            "TestEntry01",
            "ModdingTK",
            20,
            displayData: new DisplayData(DisplayName: "Test Entry 01", DisplayModName: "Modding TK", DisplayCategory: "Category 0")
        );
        
        ce_string = ConfigManager.AddConfigEntry(
            "TestEntry02",
            "ModdingTK",
            "MyValue",
            displayData: new DisplayData(DisplayName: "Test Entry 02", DisplayModName: "Modding TK", DisplayCategory: "Category 1")
        );

        cl = ConfigManager.AddConfigList(
            "TestEntry03",
            "ModdingTK",
            "03",
            new List<string> { "01", "02", "03", "04", "05" },
            displayData: new DisplayData()
        );

        icri = ConfigManager.AddConfigRangeInt(
            "TestEntry04",
            "ModdingTK",
            10, 0, 20, 21,
            displayData: new DisplayData()
        );

        icrf = ConfigManager.AddConfigRangeFloat(
            "TestEntry05",
            "ModdingTK",
            10f, 0f, 100f, 101,
            displayData: new DisplayData(
                DisplayName: "Test Entry 05",
                DisplayCategory: "Category 1"
                )
        );

        ConfigManager.AddConfigBoolean(
            "TestEntry06",
            "ModdingTK",
            false);

        #endregion

        #region NETWORKING_TESTS_CVARS

        net_ce_float = ConfigManager.AddConfigEntry<float>(
            "net_ce_test01",
            "ModdingTK2",
            10f,
            NetworkSync.ServerAuthority,
            (ce) =>
            {
                PrintNetTestMsg(
                    ce.ModName + ":" + ce.Name,
                    ce.Value.ToString(CultureInfo.CurrentCulture));
            }
        );
        
        net_ce_string = ConfigManager.AddConfigEntry<string>(
            "net_ce_test02",
            "ModdingTK2",
            "Hello",
            NetworkSync.TwoWaySync,
            (ce) =>
            {
                PrintNetTestMsg(ce.ModName + ":" + ce.Name, ce.Value);
            }
        );
        
        net_cl = ConfigManager.AddConfigList(
            "net_ce_test03",
            "ModdingTK2",
            "03",
            new List<string> { "01", "02", "03", "04", "05" },
            NetworkSync.TwoWaySync,
            null,
            (ce) =>
            {
                PrintNetTestMsg(ce.ModName + ":" + ce.Name, ce.Value);
            }
        );

        #endregion
    }

    #region NETWORKING_TESTS

    private static void PrintNetTestMsg(string name, string value)
    {
        string mode = GameMain.IsSingleplayer ? "sp" : "mp";
        Utils.Logging.PrintMessage(NetworkingManager.IsClient
            ? $"net_ce_test: client, name: {name}, mode: {mode}, net_auth: srvauth, value {value}"
            : $"net_ce_test: server, name: {name}, mode: {mode}, net_auth: srvauth, value {value}");
    }

    #endregion
    
    

    public void OnLoadCompleted()
    {
        Utils.Logging.PrintError($"MTTP: KeyBind value: {cc?.GetStringValue()}");
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