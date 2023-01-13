using System.Diagnostics;
using System.Xml.Linq;
using Barotrauma.Networking;
using ModdingToolkit;
using ModdingToolkit.Networking;
#if CLIENT
using Microsoft.Xna.Framework.Input;
#else

#endif

namespace ModdingToolkit.Config;

/// <summary>
/// [NOT THREAD-SAFE]
/// Allows for the creation of configuration data with management for Network Sync and Save/Load operations.
/// </summary>
public static partial class ConfigManager
{
    #region Events

    /// <summary>
    /// All public API consumers should use this to cleanup their references before unloading and do any saving needed.
    /// NOTE: This method will automatically unsubscribe you after it has been called as part of cleanup.
    /// </summary>
    public static event System.Action? OnDispose;
    
    #endregion
    
    #region API
    /**
     * Contains wrappers and adapters for API consumers.  
     */
    
    /// <summary>
    /// The base location where config save file folders are stored.
    /// </summary>
    public static readonly string BaseConfigDir = Path.Combine(Directory.GetCurrentDirectory(), "Config");

    /// <summary>
    /// [CanBeNull]
    /// Registers a new Config Member and returns the instance to it. Returns null if a Config Member with the same ModName and Name exist.
    /// </summary>
    /// <param name="name">Name of your config variable</param>
    /// <param name="modName">The name of your Mod. Acts a collection everything with the same ModName.</param>
    /// <param name="defaultValue">The default value if one cannot be loaded from file.</param>
    /// <param name="menuCategory">The Menu Category to show this in. Default is Gameplay (recommended).</param>
    /// <param name="networkSync">Whether this should be synced or not with the server and clients.</param>
    /// <param name="onValueChanged">Called whenever the value has been successfully changed.</param>
    /// <param name="validateNewInput">Allows you to validate any potential changes to the Value. Return false to deny.</param>
    /// <param name="filePathOverride">Use if you want to load this variable from another config file on disk. Takes an absolute path.</param>
    /// <typeparam name="T">The Value's Type</typeparam>
    /// <returns>[CanBeNull] Returns the Config Member instance or null if failed.</returns>
    public static IConfigEntry<T> AddConfigEntry<T>(
        string name,
        string modName,
        T defaultValue,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        NetworkSync networkSync = NetworkSync.NoSync,
        Action<IConfigEntry<T>>? onValueChanged = null,
        Func<T, bool>? validateNewInput = null,
        string? filePathOverride = null) where T : IConvertible
    {
        return CreateIConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged,
            validateNewInput, filePathOverride);
    }

    /// <summary>
    /// [CanBeNull]
    /// Registers a new Config Member List[string] and returns the instance to it. Returns null if a Config Member with the same ModName and Name exist.
    /// </summary>
    /// <param name="name">Name of your config variable</param>
    /// <param name="modName">The name of your Mod. Acts a collection everything with the same ModName.</param>
    /// <param name="defaultValue">The default string value if one cannot be loaded from file. Must exist in the List or the first entry in the list will be used.</param>
    /// <param name="valueList">A list of string values.</param>
    /// <param name="networkSync">Whether this should be synced or not with the server and clients.</param>
    /// <param name="menuCategory">The Menu Category to show this in. Default is Gameplay (recommended).</param>
    /// <param name="valueChangePredicate">Allows you to validate any potential changes to the Value. Return false to deny.</param>
    /// <param name="onValueChanged">Called whenever the value has been successfully changed.</param>
    /// <param name="filePathOverride">Use if you want to load this variable from another config file on disk. Takes an absolute path.</param>
    /// <returns>[CanBeNull] Returns the Config Member instance or null if failed.</returns>
    public static IConfigList AddConfigList(
        string name, string modName, 
        string defaultValue,
        List<string> valueList,
        NetworkSync networkSync = NetworkSync.NoSync,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        Func<string, bool>? valueChangePredicate = null,
        Action<IConfigList>? onValueChanged = null,
        string? filePathOverride = null)
    {
        return CreateIConfigList(name, modName, defaultValue, valueList, networkSync, 
            menuCategory, valueChangePredicate, onValueChanged, filePathOverride);
    }

    /// <summary>
    /// [CanBeNull]
    /// Registers a new Config Member Range Int and returns the instance to it. Returns null if a Config Member with the same ModName and Name exist.
    /// </summary>
    /// <param name="name">Name of your config variable</param>
    /// <param name="modName">The name of your Mod. Acts a collection everything with the same ModName.</param>
    /// <param name="defaultValue">The default value if one cannot be loaded from file.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="steps">The number of steps in the Slider in the menu.</param>
    /// <param name="networkSync">Whether this should be synced or not with the server and clients.</param>
    /// <param name="menuCategory">The Menu Category to show this in. Default is Gameplay (recommended).</param>
    /// <param name="valueChangePredicate">Allows you to validate any potential changes to the Value. Return false to deny.</param>
    /// <param name="onValueChanged">Called whenever the value has been successfully changed.</param>
    /// <param name="filePathOverride">Use if you want to load this variable from another config file on disk. Takes an absolute path.</param>
    /// <returns>[CanBeNull] Returns the Config Member instance or null if failed.</returns>
    public static IConfigRangeInt AddConfigRangeInt(
        string name, string modName,
        int defaultValue, int minValue, int maxValue, int steps,
        NetworkSync networkSync = NetworkSync.NoSync,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        Func<int, bool>? valueChangePredicate = null,
        Action<IConfigRangeInt>? onValueChanged = null,
        string? filePathOverride = null)
    {
        return CreateIConfigRangeInt(name, modName, defaultValue, minValue, maxValue, steps, networkSync, menuCategory,
            valueChangePredicate, onValueChanged, filePathOverride);
    }
    
    /// <summary>
    /// [CanBeNull]
    /// Registers a new Config Member Range Float and returns the instance to it. Returns null if a Config Member with the same ModName and Name exist.
    /// </summary>
    /// <param name="name">Name of your config variable</param>
    /// <param name="modName">The name of your Mod. Acts a collection everything with the same ModName.</param>
    /// <param name="defaultValue">The default value if one cannot be loaded from file.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="steps">The number of steps in the Slider in the menu.</param>
    /// <param name="networkSync">Whether this should be synced or not with the server and clients.</param>
    /// <param name="menuCategory">The Menu Category to show this in. Default is Gameplay (recommended).</param>
    /// <param name="valueChangePredicate">Allows you to validate any potential changes to the Value. Return false to deny.</param>
    /// <param name="onValueChanged">Called whenever the value has been successfully changed.</param>
    /// <param name="filePathOverride">Use if you want to load this variable from another config file on disk. Takes an absolute path.</param>
    /// <returns>[CanBeNull] Returns the Config Member instance or null if failed.</returns>
    public static IConfigRangeFloat AddConfigRangeFloat(
        string name, string modName,
        float defaultValue, float minValue, float maxValue, int steps,
        NetworkSync networkSync = NetworkSync.NoSync,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        Func<float, bool>? valueChangePredicate = null,
        Action<IConfigRangeFloat>? onValueChanged = null,
        string? filePathOverride = null)
    {
        return CreateIConfigRangeFloat(name, modName, defaultValue, minValue, maxValue, steps, networkSync, menuCategory,
            valueChangePredicate, onValueChanged, filePathOverride);
    }

    /// <summary>
    /// Saves the config member to disk.
    /// </summary>
    /// <param name="configEntry">Instance to save.</param>
    /// <returns>Whether or not the operation was successful.</returns>
    public static bool Save(IConfigBase configEntry)
    {
        return SaveData(configEntry);
    }

    /// <summary>
    /// Saves the config member with the given ModName and Name to disk.
    /// </summary>
    /// <param name="modName">The ModName of the member.</param>
    /// <param name="name">The Name of the member.</param>
    /// <returns>Whether or not the operation was successful.</returns>
    public static bool Save(string modName, string name)
    {
        if (LoadedConfigEntries.ContainsKey(modName)
            && LoadedConfigEntries[modName].ContainsKey(name)) 
            return SaveData(LoadedConfigEntries[modName][name]);
        LuaCsSetup.PrintCsError($"ConfigManager: Tried to save a config: {modName}:{name} does not exist in the dictionary!");
        return false;
    }

    /// <summary>
    /// Saves all config members with a given ModName to disk.
    /// </summary>
    /// <param name="modName">The ModName of the members.</param>
    /// <returns>Whether or not the operation was successful.</returns>
    public static bool SaveAllFromMod(string modName)
    {
        if (LoadedConfigEntries.ContainsKey(modName))
        {
            bool success = true;
            foreach (var configBase in LoadedConfigEntries[modName])
            {
                if (!SaveData(configBase.Value))
                    success = false;
            }
            return success;
        }
        return false;
    }

    /// <summary>
    /// Reloads the values from disk for all config members with the given ModName.
    /// </summary>
    /// <param name="modName">The ModName of the members.</param>
    /// <returns>Whether or not the operation was successful.</returns>
    public static bool ReloadAllValuesForModFromFiles(string modName)
    {
        if (!LoadedConfigEntries.ContainsKey(modName))
            return false;
        bool b = true;
        foreach (var pair in LoadedConfigEntries[modName].ToImmutableDictionary())
        {
            if (!LoadData(pair.Value, null, true, true))
            {
                b = false;
            }
        }
        return b;
    }

    /// <summary>
    /// Returns the config member instance with the given ModName and Name if found. 
    /// </summary>
    /// <param name="modName">The ModName of the member.</param>
    /// <param name="name">The Name of the member.</param>
    /// <returns>[CanBeNull] Returns the instance or null if not found.</returns>
    public static IConfigBase? GetConfigMember(string modName, string name)
    {
        if (LoadedConfigEntries.ContainsKey(modName))
            if (LoadedConfigEntries[modName].ContainsKey(name))
                return LoadedConfigEntries[modName][name];
        return null;
    }

    /// <summary>
    /// [NOT CONCURRENCY SAFE]
    /// Allows enumeration over all instanced config members.
    /// </summary>
    /// <returns>Enumerator for iteration.</returns>
    public static IEnumerable<IConfigBase> GetAllConfigMembers()
    {
        foreach (ConfigIndex index in Indexer_AllLoadedEntries)
        {
            yield return LoadedConfigEntries[index.ModName][index.Name];
        }
    }

    /// <summary>
    /// Returns a collection of config members under a given menu category.
    /// </summary>
    /// <param name="category">The menu category.</param>
    /// <returns>Enumerator for iteration.</returns>
    public static IEnumerable<IConfigBase> GetConfigMembers(IConfigBase.Category category)
    {
        if (!Indexer_MenuCategory.ContainsKey(category))
            return new List<IConfigBase>();
        List<IConfigBase> members = new();
        Indexer_MenuCategory[category].ForEach(ci =>
        {
            if (LoadedConfigEntries.ContainsKey(ci.ModName) 
                && LoadedConfigEntries[ci.ModName].ContainsKey(ci.Name))
            {
                members.Add(LoadedConfigEntries[ci.ModName][ci.Name]);
            }
        });
        return members;
    }

    /// <summary>
    /// Returns a collection of config members under a given network sync category.
    /// </summary>
    /// <param name="networkSync">The sync setting.</param>
    /// <returns>Enumerator for iteration.</returns>
    public static IEnumerable<IConfigBase> GetConfigMembers(NetworkSync networkSync)
    {
        if (!Indexer_NetSync.ContainsKey(networkSync))
            return new List<IConfigBase>();
        List<IConfigBase> members = new();
        Indexer_NetSync[networkSync].ForEach(ci =>
        {
            if (LoadedConfigEntries.ContainsKey(ci.ModName) 
                && LoadedConfigEntries[ci.ModName].ContainsKey(ci.Name))
            {
                members.Add(LoadedConfigEntries[ci.ModName][ci.Name]);
            }
        });
        return members;
    }

    #endregion

    #region Helper_Extensions
    /**
     * For use by Lua/MoonSharp Interop
     */
    
    public static IConfigEntry<double> AddConfigDouble(string name, string modName, double defaultValue,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        NetworkSync networkSync = NetworkSync.NoSync,
        Action<IConfigEntry<double>>? onValueChanged = null, 
        Func<double, bool>? validateNewInput = null, 
        string? filePath = null)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput, filePath);
    
    public static IConfigEntry<string> AddConfigString(string name, string modName, string defaultValue,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        NetworkSync networkSync = NetworkSync.NoSync,
        Action<IConfigEntry<string>>? onValueChanged = null, 
        Func<string, bool>? validateNewInput = null, 
        string? filePath = null)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput, filePath);

    
    public static IConfigEntry<bool> AddConfigBoolean(string name, string modName, bool defaultValue,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        NetworkSync networkSync = NetworkSync.NoSync,
        Action<IConfigEntry<bool>>? onValueChanged = null, 
        Func<bool, bool>? validateNewInput = null, 
        string? filePath = null)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput, filePath);

    
    public static IConfigEntry<int> AddConfigInteger(string name, string modName, int defaultValue,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        NetworkSync networkSync = NetworkSync.NoSync,
        Action<IConfigEntry<int>>? onValueChanged = null, 
        Func<int, bool>? validateNewInput = null, 
        string? filePath = null)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput, filePath);

    #endregion


    #region Internal_Func
    
    private static IConfigEntry<T> CreateIConfigEntry<T>(
        string name,
        string modName,
        T defaultValue,
        IConfigBase.Category menuCategory,
        NetworkSync networkSync,
        Action<IConfigEntry<T>>? onValueChanged,
        Func<T, bool>? validateNewInput,
        string? filePathOverride = null
        ) where T : IConvertible
    {
        ConfigEntry<T> ce = new();
        ce.Initialize(name, modName, defaultValue, defaultValue, networkSync, menuCategory, validateNewInput, onValueChanged);
        AddConfigToLists(ce);
        LoadData(ce, filePathOverride);
        if (ce is INetConfigEntry<T> ince)
            RegisterForNetworking(ince);
        return ce;
    }

    private static IConfigList CreateIConfigList(string name, string modName, string defaultValue,
        List<string> valueList,
        NetworkSync sync = NetworkSync.NoSync,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        Func<string, bool>? valueChangePredicate = null,
        Action<IConfigList>? onValueChanged = null,
        string? filePathOverride = null)
    {
        ConfigList cl = new();
        cl.Initialize(name, modName, defaultValue, defaultValue, valueList, sync, menuCategory, valueChangePredicate, onValueChanged);
        AddConfigToLists(cl);
        LoadData(cl, filePathOverride);
        if (cl is INetConfigEntry<ushort> ince)
            RegisterForNetworking(ince);
        return cl;
    }
    
    private static IConfigRangeInt CreateIConfigRangeInt(
        string name, string modName,
        int defaultValue, int minValue, int maxValue, int steps,
        NetworkSync sync = NetworkSync.NoSync,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        Func<int, bool>? valueChangePredicate = null,
        Action<IConfigRangeInt>? onValueChanged = null,
        string? filePathOverride = null)
    {
        ConfigRangeInt cr = new();
        cr.Initialize(name, modName, defaultValue, defaultValue, minValue, maxValue, steps, sync, menuCategory, valueChangePredicate);
        AddConfigToLists(cr);
        LoadData(cr, filePathOverride);
        if (cr is INetConfigEntry<int> ince)
            RegisterForNetworking(ince);
        return cr;
    }

    private static IConfigRangeFloat CreateIConfigRangeFloat(
        string name, string modName,
        float defaultValue, float minValue, float maxValue, int steps,
        NetworkSync sync = NetworkSync.NoSync,
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        Func<float, bool>? valueChangePredicate = null,
        Action<IConfigRangeFloat>? onValueChanged = null,
        string? filePathOverride = null)
    {
        ConfigRangeFloat cr = new();
        cr.Initialize(name, modName, defaultValue, defaultValue, minValue, maxValue, steps, sync, menuCategory, valueChangePredicate);
        AddConfigToLists(cr);
        LoadData(cr, filePathOverride);
        if (cr is INetConfigEntry<float> ince)
            RegisterForNetworking(ince);
        return cr;
    }

    #region INTERNAL_API

    internal static bool ReloadAllValueFromFiles()
    {
        XMLDocumentHelper.UnloadCache();
        bool success = true;
        foreach (var cList in LoadedConfigEntries.ToImmutableDictionary())
        {
            if (!ReloadAllValuesForModFromFiles(cList.Key))
                success = false;
        }

        return success;
    }
    
    internal static void SaveAll()
    {
        foreach (var configDictL in LoadedConfigEntries)
        {
            foreach (var configBase in configDictL.Value)
            {
                SaveData(configBase.Value);
            }
        }
    }

    internal static void Dispose()
    {
        OnDispose?.Invoke();
        
        foreach (ConfigIndex configIndex in Indexer_AllLoadedEntries.ToImmutableList())
        {
            RemoveConfigFromLists(LoadedConfigEntries[configIndex.ModName][configIndex.Name]);
        }

        LoadedConfigEntries.Clear();
        LoadedXDocKeys.Clear();
        Indexer_MenuCategory.Clear();
        Indexer_NetSync.Clear();
        Indexer_AllLoadedEntries.Clear();
        
        CleanupEvents();
    }

    private static void CleanupEvents()
    {
        try
        {
            if (OnDispose is not null)
            {
                foreach (Delegate del in OnDispose.GetInvocationList())
                {
                    OnDispose -= (Action)del;
                }
            }
        }
        finally
        {
            OnDispose = null;
        }
    }
    
    #endregion
    
    #region INTERNAL_OPERATIONS

    #region LOCAL_IO
    private static bool SaveData(IConfigBase config)
    {
        if (config.Name.IsNullOrWhiteSpace())
        {
            LuaCsSetup.PrintCsError($"ConfigManager::SaveData() | config var Name is null!");
            return false;
        }
        if (config.ModName.IsNullOrWhiteSpace())
        {
            LuaCsSetup.PrintCsError($"ConfigManager::SaveData() | config var ModName is null!");
            return false;
        }

        string keyIndex = GenerateXDocKey(config)!;
        if (!LoadedXDocKeys.ContainsKey(keyIndex))
        {
            LuaCsSetup.PrintCsError($"ConfigManager::SaveData() | Cannot find XDoc key!");
            return false;
        }

        XDocument? doc;
        if (!XMLDocumentHelper.TryGetLoadedXmlDoc(LoadedXDocKeys[keyIndex], out doc))
        {
            LuaCsSetup.PrintCsError($"ConfigManager::SaveData() | Cannot load XDocument!");
            return false;
        }

        XElement? configElement = doc?.Root?
            .DescendantsAndSelf(nameof(ConfigManager)).FirstOrDefault(defaultValue: null)?
            .Descendants(config.ModName).FirstOrDefault(defaultValue: null)?
            .Descendants(config.Name).FirstOrDefault(defaultValue: null) ?? null;
        if (configElement is null)
        {
            LuaCsSetup.PrintCsError($"ConfigManager::SaveData() | XDocument does not contain an entry for this config!");
            return false;
        }
        configElement.Value = config.GetStringValue();
        var r = XMLDocumentHelper.SaveLoadedDocToDisk(LoadedXDocKeys[keyIndex]);
        if (r != Utils.IO.IOActionResultState.Success)
        {
            LuaCsSetup.PrintCsError($"ConfigManager::SaveData() | Could not save to disk! IOActionResult: {r.ToString()}");
            return false;
        }

        return true;
    }

    private static bool LoadData(IConfigBase config, string? filePath = null, bool overwriteBadXMLFile = true, bool overrideExisting = false)
    {
        string? fp = filePath;
        bool b = true;
        if (filePath.IsNullOrWhiteSpace())
            b = GetDefaultFilePath(config, out fp);

        if (b)
        {
            string? key;

            XDocument? doc = null;
            XElement? configRoot = null;
            XElement? configModData = null;
            XElement? configVarData = null;
            
            if (!XMLDocumentHelper.LoadOrCreateDocToCache(fp!, out key, true, true,
            () => new XDocument(
                new XDeclaration("1.0", "utf-8", "true"),
                new XComment("Generated by ModdingToolkit::ConfigManager"),
                configRoot = new XElement(nameof(ConfigManager),
                    configModData = new XElement(config.ModName, 
                        configVarData = new XElement(config.Name, 
                            config.GetStringValue())))
                ).ToString()
            ))
            {
                LuaCsSetup.PrintCsError($"ConfigManager::LoadData() | Failed to load existing data. | Config: {config.ModName}, {config.Name}");
            }
            string? lxkey = GenerateXDocKey(config);
            if (lxkey is null)
            {
                LuaCsSetup.PrintCsError($"ConfigManager::LoadData() | Unable to create lxkey.");
                return false;
            }
            if (LoadedXDocKeys.ContainsKey(lxkey))
            {
                if (overrideExisting)
                {
                    LoadedXDocKeys.Remove(lxkey);
                }
                else
                {
                    LuaCsSetup.PrintCsError($"ConfigManager::LoadData() | lxkey {lxkey} already exists!");
                    return false;
                }
            }
            Debug.Assert(key is not null);
            LoadedXDocKeys.Add(lxkey, key);

            if (XMLDocumentHelper.TryGetLoadedXmlDoc(key, out doc))
            {
                Debug.Assert(doc != null);
                Debug.Assert(doc.Root != null);
                
                configRoot = doc.Root.DescendantsAndSelf(nameof(ConfigManager)).FirstOrDefault(defaultValue: null);
                configModData = configRoot?.Descendants(config.ModName).FirstOrDefault(defaultValue: null) ?? null;
                configVarData = configModData?.Descendants(config.Name).FirstOrDefault(defaultValue: null) ?? null;
            } 

            if (doc is null || configRoot is null || configModData is null || configVarData is null)
            {
                config.SetValueAsDefault();
                if (overwriteBadXMLFile)
                {
                    if (doc is null || (doc.Root?.IsEmpty ?? true) || configRoot is null)
                    {
                        doc = new XDocument(
                            new XDeclaration("1.0", "utf-8", "true"),
                            new XComment("Generated by ModdingToolkit::ConfigManager"),
                            configRoot = new XElement(nameof(ConfigManager),
                                configModData = new XElement(config.ModName, 
                                    configVarData = new XElement(config.Name, 
                                        config.GetStringValue()))
                            )
                        );
                    }

                    if (configModData is null)
                    {
                        configRoot.Add(
                            configModData = new XElement(config.ModName,
                                configVarData = new XElement(config.Name), config.GetStringValue())
                        );
                    }

                    if (configVarData is null)
                    {
                        configModData.Add(
                            configVarData = new XElement(config.Name, config.GetStringValue()));
                    }

                    if (XMLDocumentHelper.TrySetRefLoadedXmlDoc(key, doc, true))
                    {
                        if (XMLDocumentHelper.SaveLoadedDocToDisk(key) != Utils.IO.IOActionResultState.Success)
                        {
                            LuaCsSetup.PrintCsError($"ConfigManager::LoadData() | Could not save new XDoc for {config.ModName}, {config.Name}");
                        }
                    }
                    else
                        LuaCsSetup.PrintCsError($"ConfigManager::LoadData() | Could not save new XDoc for {config.ModName}, {config.Name}");
                }
            }
            else
            {
                if (configVarData.Value.IsNullOrWhiteSpace())
                    config.SetValueAsDefault();
                else
                    config.SetValueFromString(configVarData.Value);
            }
            return true;
        }
        LuaCsSetup.PrintCsError(
            $"ConfigManager::LoadData() | Unable to get a valid file path for config name {config.Name}, modname {config.ModName}");
        return false;
    }

    private static string? GenerateXDocKey(IConfigBase config)
    {
        if (config.Name.IsNullOrWhiteSpace())
        {
            LuaCsSetup.PrintCsError($"ConfigManager::GenerateXDocKey() | config var Name is null!");
            return null;
        }
        if (config.ModName.IsNullOrWhiteSpace())
        {
            LuaCsSetup.PrintCsError($"ConfigManager::GenerateXDocKey() | config var ModName is null!");
            return null;
        }

        return config.ModName + "::" + config.Name;
    }
    
    private static bool GetDefaultFilePath(IConfigBase config, out string? fp)
    {
        fp = null;
        if (config.Name.IsNullOrWhiteSpace())
        {
            LuaCsSetup.PrintCsError($"ConfigManager::GetDefaultFilePath() | config var Name is null!");
            return false;
        }
        if (config.ModName.IsNullOrWhiteSpace())
        {
            LuaCsSetup.PrintCsError($"ConfigManager::GetDefaultFilePath() | config var ModName is null!");
            return false;
        }
        fp = Path.Combine(BaseConfigDir, Utils.IO.SanitizePath(config.ModName), Utils.IO.SanitizeFileName(config.ModName) + ".xml");
        return true;
    }
    
    #endregion

    #region NETWORKING

    private static void RegisterForNetworking(INetConfigBase cfg)
    {
        if (!GameMain.IsMultiplayer || cfg.NetSync == NetworkSync.NoSync)
            return;
        if (!NetworkingManager.RegisterNetConfigInstance(cfg))
        {
            LuaCsSetup.PrintCsError($"Network Registration for {cfg.ModName} {cfg.Name} failed.");
        }
    }

    private static void RegisterForNetworking<T>(INetConfigEntry<T> cfg) where T : IConvertible
    {
        if (!GameMain.IsMultiplayer || cfg.NetSync == NetworkSync.NoSync)
            return;
        if (!NetworkingManager.RegisterNetConfigInstance(cfg))
        {
            LuaCsSetup.PrintCsError($"Network Registration for {cfg.ModName} {cfg.Name} failed.");
        }
    }

    #endregion

    private static void RemoveConfigFromLists(IConfigBase config)
    {
        if (config.MenuCategory != IConfigBase.Category.Ignore)
        {
            ConfigIndex? ci = Indexer_MenuCategory[config.MenuCategory].First(entry => 
                entry.Name.Equals(config.Name) && entry.ModName.Equals(config.ModName));
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (ci is not null)
                Indexer_MenuCategory[config.MenuCategory].Remove(ci);
        }
        
        if (config.NetSync != NetworkSync.NoSync)
        {
            ConfigIndex? ci = Indexer_NetSync[config.NetSync].First(entry => 
                entry.Name.Equals(config.Name) && entry.ModName.Equals(config.ModName));
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (ci is not null)
                Indexer_NetSync[config.NetSync].Remove(ci);
        }

#if CLIENT
        if (config is IConfigControl)
        {
            ConfigIndex? ci = Indexer_KeyMouseControls.First(entry =>
                entry.Name.Equals(config.Name) && entry.ModName.Equals(config.ModName));
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (ci is not null)
                Indexer_KeyMouseControls.Remove(ci);
        }
#endif

        LoadedConfigEntries[config.ModName]?.Remove(config.Name);

        ConfigIndex? ci2 = null;
        Indexer_AllLoadedEntries.ForEach(entry =>
        {
            if (entry.ModName.Equals(config.ModName) && entry.Name.Equals(config.Name))
                ci2 = entry;
        });
        if (ci2 is not null)
            Indexer_AllLoadedEntries.Remove(ci2);
        string? k = GenerateXDocKey(config);
        if (k is not null)
        {
            LoadedXDocKeys.Remove(k);
        }
    }
    
    private static void AddConfigToLists(IConfigBase config)
    {
        if (config.Name.Trim().IsNullOrEmpty())
        {
            LuaCsSetup.PrintCsError($"ConfigManager::AddConfigToList() | Name is null!");
            return;
        }
        
        if (config.ModName.Trim().IsNullOrEmpty())
        {
            LuaCsSetup.PrintCsError($"ConfigManager::AddConfigToList() | Name is null!");
            return;
        }
        
        if (!LoadedConfigEntries.ContainsKey(config.ModName))
            LoadedConfigEntries.Add(config.ModName, new Dictionary<string, IConfigBase>());

        if (LoadedConfigEntries[config.ModName].ContainsKey(config.Name))
        {
            LuaCsSetup.PrintCsError($"ConfigManager::AddConfigToList() | Could not register the config entry from ModName {config.ModName} with the name {config.Name}. An entry already exists!");
            return;
        }
        LoadedConfigEntries[config.ModName].Add(config.Name, config);
        
        if (config.NetSync != NetworkSync.NoSync)
        {
            if (!Indexer_NetSync.ContainsKey(config.NetSync))
                Indexer_NetSync.Add(config.NetSync, new List<ConfigIndex>());
            bool exists = false;
            Indexer_NetSync[config.NetSync].ForEach(index =>
            {
                if (index.Name.Equals(config.Name) && index.ModName.Equals(config.ModName))
                {
                    exists = true;
                }
            });
            if (exists)
                LuaCsSetup.PrintCsError($"ConfigManager::AddConfigToList() | Could not register the config entry from ModName {config.ModName} with the name {config.Name} for NETSYNC. An entry already exists!");
            else
                Indexer_NetSync[config.NetSync].Add(new ConfigIndex(config.ModName, config.Name));
        }

        if (config.MenuCategory != IConfigBase.Category.Ignore)
        {
            if (!Indexer_MenuCategory.ContainsKey(config.MenuCategory))
                Indexer_MenuCategory.Add(config.MenuCategory, new List<ConfigIndex>());
            bool exists = false;
            Indexer_MenuCategory[config.MenuCategory].ForEach(index =>
            {
                if (index.Name.Equals(config.Name) && index.ModName.Equals(config.ModName))
                {
                    exists = true;
                }
            });
            if (exists)
                LuaCsSetup.PrintCsError($"ConfigManager::AddConfigToList() | Could not register the config entry from ModName {config.ModName} with the name {config.Name} for DISPLAY. An entry already exists!");
            else
                Indexer_MenuCategory[config.MenuCategory].Add(new ConfigIndex(config.ModName, config.Name));
        }

#if CLIENT
        if (config is IConfigControl)
        {
            Indexer_KeyMouseControls.Add(new ConfigIndex(config.ModName, config.Name));
        }
#endif

        ConfigIndex? ci = null;
        Indexer_AllLoadedEntries.ForEach(entry =>
        {
            if (entry.ModName.Equals(config.ModName) && entry.Name.Equals(config.Name))
                ci = entry;
        });
        if (ci is null)
        {
            Indexer_AllLoadedEntries.Add(new ConfigIndex(config.ModName, config.Name));
        }
    }
    
    #endregion

    #endregion
    
    #region Type_Vars

    /// <summary>
    /// KeyMap: [ModName] -> [Name]
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, IConfigBase>> LoadedConfigEntries = new();
    private static readonly Dictionary<IConfigBase.Category, List<ConfigIndex>> Indexer_MenuCategory = new();
    private static readonly Dictionary<NetworkSync, List<ConfigIndex>> Indexer_NetSync = new();
    private static readonly List<ConfigIndex> Indexer_KeyMouseControls = new();
    private static readonly List<ConfigIndex> Indexer_AllLoadedEntries = new();
    private static readonly Dictionary<string, string> LoadedXDocKeys = new();

    #endregion


    #region Typedef

    // public for reflection use.
    public record ConfigIndex(string ModName, string Name);

    #endregion
}