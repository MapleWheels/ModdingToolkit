namespace ModdingToolkit.Config;

public static partial class ConfigManager
{
    public static IConfigControl AddConfigKeyOrMouseBind(
        string name,
        string modName,
        KeyOrMouse defaultValue,
        Action? onValueChanged = null,
        string? filePathOverride = null
    )
    {
        return CreateIConfigControl(name, modName, defaultValue, onValueChanged, filePathOverride);
    }
    
    public static IEnumerable<IConfigControl> GetControlConfigs()
    {
        List<IConfigControl> members = new();
        Indexer_KeyMouseControls.ForEach(ci =>
        {
            if (LoadedConfigEntries.ContainsKey(ci.ModName) 
                && LoadedConfigEntries[ci.ModName].ContainsKey(ci.Name)
                && LoadedConfigEntries[ci.ModName][ci.Name] is IConfigControl icc)
            {
                members.Add(icc);   
            }
        });
        return members;
    }
    
    private static IConfigControl CreateIConfigControl(
        string name,
        string modName,
        KeyOrMouse defaultValue,
        Action? onValueChanged,
        string? filePathOverride = null)
    {
        ConfigControl cc = new();
        cc.Initialize(name, modName, null, defaultValue, onValueChanged);
        AddConfigToLists(cc);
        LoadData(cc, filePathOverride);
        return cc;
    }
}