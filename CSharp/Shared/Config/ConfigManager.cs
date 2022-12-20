
using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Input;
using ModdingToolkit;

namespace ModdingToolkit.Config;

/// <summary>
/// [NOT THREAD-SAFE]
/// Allows for the creation of configuration data with management for Network Sync and Save/Load operations.
/// </summary>
public static class ConfigManager
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

    
    public static IConfigEntry<T> AddConfigEntry<T>(
        string name,
        string modName,
        T defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged,
        Func<T, bool> validateNewInput,
        string? filePath = null) where T : IConvertible
    {
        return CreateIConfigEntryInst<T>(name, modName, defaultValue, menuCategory, networkSync, onValueChanged,
            validateNewInput, filePath);
    }

    public static IConfigControl AddConfigKeyOrMouseBind(
        string name,
        string modName,
        KeyOrMouse defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged
    )
    {
        return CreateIConfigControl(name, modName, defaultValue, menuCategory, networkSync, onValueChanged);
    }

    /// <summary>
    /// [NOT IMPLEMENTED]
    /// [IGNORE]
    /// </summary>
    /// <param name="name"></param>
    /// <param name="modName"></param>
    /// <param name="defaultValue"></param>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <param name="stepValue"></param>
    /// <param name="menuCategory"></param>
    /// <param name="networkSync"></param>
    /// <param name="onValueChanged"></param>
    /// <param name="validateNewInput"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IConfigRangeBase<double> AddConfigRangeDoubleEntry(string name,
        string modName,
        double defaultValue,
        double minValue,
        double maxValue,
        double stepValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged,
        Func<double, bool> validateNewInput,
        string? filePath = null)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// [NOT IMPLEMENTED]
    /// [IGNORE]
    /// </summary>
    /// <param name="name"></param>
    /// <param name="modName"></param>
    /// <param name="defaultValue"></param>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <param name="stepValue"></param>
    /// <param name="menuCategory"></param>
    /// <param name="networkSync"></param>
    /// <param name="onValueChanged"></param>
    /// <param name="validateNewInput"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IConfigRangeBase<int> AddConfigRangeIntEntry(string name,
        string modName,
        int defaultValue,
        int minValue,
        int maxValue,
        int stepValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged,
        Func<int, bool> validateNewInput,
        string? filePath = null)
    {
        throw new NotImplementedException();
    }

    public static bool Save(IConfigBase configEntry)
    {
        return SaveData(configEntry);
    }

    public static bool Save(string modName, string name)
    {
        if (LoadedConfigEntries.ContainsKey(modName)
            && LoadedConfigEntries[modName].ContainsKey(name)) 
            return SaveData(LoadedConfigEntries[modName][name]);
        LuaCsSetup.PrintCsError($"ConfigManager: Tried to save a config: {modName}:{name} does not exist in the dictionary!");
        return false;
    }

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

    public static bool ReloadAllValuesForModFromFiles(string modName)
    {
        if (!LoadedConfigEntries.ContainsKey(modName))
            return false;
        foreach (var pair in LoadedConfigEntries[modName].ToImmutableDictionary())
        {
            LoadData(pair.Value, null, true, true);
        }

        return true;
    }

    

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

    public static IEnumerable<IConfigBase> GetConfigMembers(IConfigBase.NetworkSync networkSync)
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

    #endregion

    #region Helper_Extensions
    /**
     * For use by Lua/MoonSharp Interop
     */
    
    public static IConfigEntry<double> AddConfigDouble(string name, string modName, double defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged, Func<double, bool> validateNewInput, string? filePath = null)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput, filePath);
    
    public static IConfigEntry<string> AddConfigString(string name, string modName, string defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged, Func<string, bool> validateNewInput, string? filePath = null)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput, filePath);
    
    public static IConfigEntry<bool> AddConfigBoolean(string name, string modName, bool defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged, Func<bool, bool> validateNewInput, string? filePath = null)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput, filePath);
    
    public static IConfigEntry<int> AddConfigInteger(string name, string modName, int defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged, Func<int, bool> validateNewInput, string? filePath = null)
        => AddConfigEntry(name, modName, defaultValue, menuCategory, networkSync, onValueChanged, validateNewInput, filePath);

    #endregion


    #region Internal_Func
    
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
                    OnDispose -= (System.Action)del;
                }
            }
        }
        finally
        {
            OnDispose = null;
        }
    }

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
        
        #warning Remove debug message.
        LuaCsSetup.PrintCsMessage($"ConfigManager::SaveData() | XDOC: {doc?.Root?.ToString() ?? "null"}");
        LuaCsSetup.PrintCsMessage($"ConfigManager::SaveData() | CNFG: {config.ModName}:{config.Name}");

        XElement? ce = doc?.Root ?? null;
        LuaCsSetup.PrintCsMessage($"ConfigManager::SaveData() | DESC1: {ce?.ToString() ?? "null"}");

        XElement? ce2 = ce?.Element("ConfigManager");
        LuaCsSetup.PrintCsMessage($"ConfigManager::SaveData() | DESC2: {ce2?.ToString() ?? "null"}");

        XElement? ce3 = ce2?.Descendants(config.ModName).FirstOrDefault(defaultValue: null);
        LuaCsSetup.PrintCsMessage($"ConfigManager::SaveData() | DESC3: {ce3?.ToString() ?? "null"}");

        XElement? ce4 = ce3?.Descendants(config.Name).FirstOrDefault(defaultValue: null);
        LuaCsSetup.PrintCsMessage($"ConfigManager::SaveData() | DESC4: {ce4?.ToString() ?? "null"}");
        
        XElement? configElement = doc?.Root?
            .Descendants(nameof(ConfigManager)).FirstOrDefault(defaultValue: null)?
            .Descendants(config.ModName).FirstOrDefault(defaultValue: null)?
            .Descendants(config.Name).FirstOrDefault(defaultValue: null) ?? null;
        if (configElement is null)
        {
            LuaCsSetup.PrintCsError($"ConfigManager::SaveData() | XDocument does not contain an entry for this config!");
            return false;
        }
        configElement.Value = config.GetStringValue();
        var r = XMLDocumentHelper.SaveLoadedDocToDisk(LoadedXDocKeys[keyIndex]);
        if (r != Utils.IOActionResultState.Success)
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
            if (!XMLDocumentHelper.LoadOrCreateDocToCache(fp!, out key, true, true))
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
            LoadedXDocKeys.Add(lxkey, key);

            XDocument? doc = null;
            XElement? configRoot = null;
            XElement? configModData = null;
            XElement? configVarData = null;
            config.SetValueAsDefault();
            
            if (XMLDocumentHelper.TryGetLoadedXmlDoc(key, out doc))
            {
                Debug.Assert(doc != null, "doc != null");
                Debug.Assert(doc.Root != null, "doc.Root != null");
                
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
                        if (XMLDocumentHelper.SaveLoadedDocToDisk(key) != Utils.IOActionResultState.Success)
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
    
    private static IConfigControl CreateIConfigControl(
        string name,
        string modName,
        KeyOrMouse defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged,
        string? filePathOverride = null)
    {
        ConfigControl cc = new ConfigControl();
        cc.Initialize(name, modName, null, defaultValue, onValueChanged);
        AddConfigToLists(cc);
        LoadData(cc, filePathOverride);
        return cc;
    }

    private static IConfigEntry<T> CreateIConfigEntryInst<T>(
        string name,
        string modName,
        T defaultValue,
        IConfigBase.Category menuCategory,
        IConfigBase.NetworkSync networkSync,
        Action onValueChanged,
        Func<T, bool> validateNewInput,
        string? filePath = null
        ) where T : IConvertible
    {
        ConfigEntry<T> ce = new ConfigEntry<T>(defaultValue, menuCategory, networkSync);
        ce.Initialize(name, modName, defaultValue);
        AddConfigToLists(ce);
        LoadData(ce, null, true);
        return ce;
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
        fp = Path.Combine(BaseConfigDir, Utils.SanitizePath(config.ModName), Utils.SanitizeFileName(config.ModName) + ".xml");
        return true;
    }

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
        
        if (config.NetSync != IConfigBase.NetworkSync.NoSync)
        {
            ConfigIndex? ci = Indexer_NetSync[config.NetSync].First(entry => 
                entry.Name.Equals(config.Name) && entry.ModName.Equals(config.ModName));
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (ci is not null)
                Indexer_NetSync[config.NetSync].Remove(ci);
        }

        if (config is IConfigControl)
        {
            ConfigIndex? ci = Indexer_KeyMouseControls.First(entry =>
                entry.Name.Equals(config.Name) && entry.ModName.Equals(config.ModName));
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (ci is not null)
                Indexer_KeyMouseControls.Remove(ci);
        }

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
        
        if (config.NetSync != IConfigBase.NetworkSync.NoSync)
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

        if (config is IConfigControl)
        {
            Indexer_KeyMouseControls.Add(new ConfigIndex(config.ModName, config.Name));
        }

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
    
    #region Type_Vars

    /// <summary>
    /// KeyMap: [ModName] -> [Name]
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, IConfigBase>> LoadedConfigEntries = new();
    private static readonly Dictionary<IConfigBase.Category, List<ConfigIndex>> Indexer_MenuCategory = new();
    private static readonly Dictionary<IConfigBase.NetworkSync, List<ConfigIndex>> Indexer_NetSync = new();
    private static readonly List<ConfigIndex> Indexer_KeyMouseControls = new();
    private static readonly List<ConfigIndex> Indexer_AllLoadedEntries = new();
    private static readonly Dictionary<string, string> LoadedXDocKeys = new();

    #endregion


    #region Typedef

    // public for reflection use.
    public record ConfigIndex(string ModName, string Name);

    #endregion
}