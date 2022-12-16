
namespace ModdingToolkit.Config;

/// <summary>
/// [NOT THREAD-SAFE]
/// Allows for the creation of configuration data with management for Network Sync and Save/Load operations.
/// </summary>
public static class ConfigManager
{
    #region Events

    public static event System.Action<string, string> OnEntryAdded;
    public static event System.Action<string, string> OnEntryRemoved;
    public static event System.Action<string, string, Exception> OnEntryAddError; 

    #endregion
    
    #region API
    
    /// <summary>
    /// The base location where config save file folders are stored.
    /// </summary>
    public static readonly string BaseConfigDir = Path.Combine(Directory.GetCurrentDirectory(), "Config");

    
    public static IConfigEntry<T> AddConfigEntry<T>(
        string name,
        string modName,
        T defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged,
        Func<T, bool> validateNewInput) where T : IConvertible
    {
        throw new NotImplementedException();
    }

    public static IConfigControl AddConfigKeyOrMouseBind(
        string name,
        string modName,
        KeyOrMouse defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged,
        Func<KeyOrMouse, bool> validateNewInput
    )
    {
        throw new NotImplementedException();
    }

    public static IConfigRangeBase<double> AddConfigRangeDoubleEntry(string name,
        string modName,
        double defaultValue,
        double minValue,
        double maxValue,
        double stepValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged,
        Func<double, bool> validateNewInput)
    {
        throw new NotImplementedException();
    }
    
    public static IConfigRangeBase<int> AddConfigRangeIntEntry(string name,
        string modName,
        int defaultValue,
        int minValue,
        int maxValue,
        int stepValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged,
        Func<int, bool> validateNewInput)
    {
        throw new NotImplementedException();
    }

    public static bool Save(IConfigBase configEntry)
    {
        throw new NotImplementedException();
    }

    public static bool Save(string modName, string name)
    {
        throw new NotImplementedException();
    }

    public static bool SaveAllFromMod(string modName)
    {
        throw new NotImplementedException();
    }

    public static void ReloadAllFromFiles()
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<IConfigBase> GetConfigMembers(IConfigBase.Category category)
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<IConfigBase> GetConfigMembers(IConfigBase.NetworkSync networkSync)
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<IConfigControl> GetControlConfigs()
    {
        throw new NotImplementedException();
    }
    


    #endregion

    #region Helper_Extensions
    
    public static IConfigEntry<double> AddConfigDouble(string name, string modName, double defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged, Func<double, bool> validateNewInput)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput);
    
    public static IConfigEntry<string> AddConfigString(string name, string modName, string defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged, Func<string, bool> validateNewInput)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput);
    
    public static IConfigEntry<bool> AddConfigBoolean(string name, string modName, bool defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged, Func<bool, bool> validateNewInput)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput);
    
    public static IConfigEntry<int> AddConfigInteger(string name, string modName, int defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged, Func<int, bool> validateNewInput)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput);

    #endregion


    #region Internal_Func

    internal static void SaveAll()
    {
        throw new NotImplementedException();
    }

    internal static void Initialize()
    {
        throw new NotImplementedException();
    }

    internal static void Dispose()
    {
        throw new NotImplementedException();
    }

    

    #endregion
    
    #region Type_Vars

    /// <summary>
    /// KeyMap: [ModName] -> [Name]
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, IConfigBase>> LoadedConfigEntries = new();
    private static readonly Dictionary<IConfigBase.Category, ConfigIndex> Indexer_MenuCategory = new();
    private static readonly Dictionary<IConfigBase.NetworkSync, ConfigIndex> Indexer_NetSync = new();
    private static readonly List<ConfigIndex> Indexer_KeyMouseControls = new();

    #endregion


    #region Typedef

    // public for reflection use.
    public record ConfigIndex(string ModName, string Name);

    #endregion
}