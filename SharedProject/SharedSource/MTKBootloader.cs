using ModdingToolkit.Client;
using ModdingToolkit.Config;
using ModdingToolkit.Networking;
using ModdingToolkit.Patches;
using MoonSharp.Interpreter;

#if CLIENT
using ModConfigManager;
#endif

namespace ModdingToolkit;

// ReSharper disable once InconsistentNaming
public sealed class MTKBootloader : IAssemblyPlugin
{
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool IsLoaded { get; private set; }

    public MTKBootloader()
    {
        Utils.Logging.PrintMessage($"Modding Toolkit: Starting...");
        RegisterLua();
    }

    public void Initialize()
    {
        bool enabled = CheckIfEnabled();
        
        if (enabled)
            Utils.Logging.PrintMessage($"Modding Toolkit loading in Standard Mode.");
        else
            Utils.Logging.PrintMessage($"Modding Toolkit loading in Forced Mode.");
        
#if CLIENT
        if (enabled)
            NetworkingManager.Initialize(true);
#else
        NetworkingManager.Initialize(true);
#endif

#if CLIENT
        PatchManager.RegisterPatches(ModConfigManager.Patches.GetPatches());
#endif
    }

    public void OnLoadCompleted()
    {
        LoadPatches();
        RegisterCommands();
        ConsoleCommands.ReloadAllCommands();
#if CLIENT
        if (GameMain.IsMultiplayer && CheckIfEnabled())
            NetworkingManager.SynchronizeAll();        
#endif
        IsLoaded = true;
    }

    public void PreInitPatching()
    {
        // Not Impl
    }

    private bool CheckIfEnabled()
    {
        return ContentPackageManager.EnabledPackages.All.Any(p => 
            p.UgcId.ValueEquals(new SteamWorkshopId(2905375979)) 
            || p.Name.Trim().ToLowerInvariant().Contains("moddingtoolkit"));
    }

    private void RegisterCommands()
    {
        ConsoleCommands.RegisterCommand(
            "cl_cfgsetvar",
            "Sets a config member to the supplied string. Format is <command> <modname> <name> \"<value>\"",
            argsv =>
            {
                if (argsv.Length < 1)
                {
                    Utils.Logging.PrintError($"Arguments missing!");
                    return;
                }
                
                string[] args = (string[])argsv[0];
                if (args.Length >= 3
                    && args[0] is { } modname
                    && args[1] is { } name
                    && args[2] is { } newvalue)
                {
                    if (ConfigManager.GetConfigMember(modname, name) is { } icb)
                    {
                        if (!icb.ValidateString(newvalue))
                        {
                            LuaCsSetup.PrintCsMessage($"ConfigManager: The value of \"{newvalue}\" is not valid for the Type {icb.SubTypeDef}");
                            return;
                        }
                        icb.SetValueFromString(newvalue);
                        LuaCsSetup.PrintCsMessage($"ConfigManager: ModName={modname}, Name={name}, NewValue={icb.GetStringValue()}");
                        return;
                    }
                    LuaCsSetup.PrintCsMessage($"ConfigManager: Could not find a cvar by the name of {modname} : {name}.");
                }
            });
        
        ConsoleCommands.RegisterCommand(
            "cl_cfglistvars",
            "Prints a list of all config members",
            _ =>
            {
                PrintAllConfigVars();   
            });

        ConsoleCommands.RegisterCommand(
            "cl_cfggetvar",
            "Gets a config member. Format is <command> \"<modname>\" \"<name>\"",
            argsv =>
            {
                if (argsv.Length < 1)
                {
                    Utils.Logging.PrintError($"Arguments missing!");
                    return;
                }
                
                string[] args = (string[])argsv[0];
                if (args[0].Length >= 2
                    && args[0] is { } modname
                    && args[1] is { } name)
                {
                    if (ConfigManager.GetConfigMember(modname, name) is { } icb)
                    {
                        LuaCsSetup.PrintCsMessage($"ConfigManager: ModName={modname}, Name={name}, Value={icb.GetStringValue()}");
                    }
                    LuaCsSetup.PrintCsMessage($"ConfigManager: Could not find a cvar by the name of {modname} : {name}.");
                }
            }
        );
        
        ConsoleCommands.RegisterCommand(
            "cl_cfgsaveall", 
            "Save all config variables to file.",
            _ =>
            {
                ConfigManager.SaveAll();
                LuaCsSetup.PrintCsMessage($"ConfigManager: All files saved to disk.");
            });
    }

    private void LoadPatches()
    {
        PatchManager.OnPatchStateUpdate += OnPatchStateUpdate;
        PatchManager.BuildPatchList();
        PatchManager.Load();
    }

    private void RegisterLua()
    {
        // Interfaces
        UserData.RegisterType<INetConfigBase>();
        
        UserData.RegisterType<IConfigBase>();
        UserData.RegisterType<IConfigEntry<bool>>();
        UserData.RegisterType<IConfigEntry<byte>>();
        UserData.RegisterType<IConfigEntry<short>>();
        UserData.RegisterType<IConfigEntry<int>>();
        UserData.RegisterType<IConfigEntry<sbyte>>();
        UserData.RegisterType<IConfigEntry<ushort>>();
        UserData.RegisterType<IConfigEntry<uint>>();
        UserData.RegisterType<IConfigEntry<ulong>>();
        UserData.RegisterType<IConfigEntry<long>>();
        UserData.RegisterType<IConfigEntry<float>>();
        UserData.RegisterType<IConfigEntry<double>>();
        UserData.RegisterType<IConfigEntry<string>>();
        UserData.RegisterType<IConfigList>();
        UserData.RegisterType<IConfigRangeFloat>();
        UserData.RegisterType<IConfigRangeInt>();
        
        // Types
        UserData.RegisterType<DisplayData>();
        UserData.RegisterType<NetworkSync>();
        UserData.RegisterType<DisplayType>();
        
        UserData.RegisterType<ConfigEntry<bool>>();
        UserData.RegisterType<ConfigEntry<byte>>();
        UserData.RegisterType<ConfigEntry<short>>();
        UserData.RegisterType<ConfigEntry<int>>();
        UserData.RegisterType<ConfigEntry<sbyte>>();
        UserData.RegisterType<ConfigEntry<ushort>>();
        UserData.RegisterType<ConfigEntry<uint>>();
        UserData.RegisterType<ConfigEntry<ulong>>();
        UserData.RegisterType<ConfigEntry<long>>();
        UserData.RegisterType<ConfigEntry<float>>();
        UserData.RegisterType<ConfigEntry<double>>();
        UserData.RegisterType<ConfigEntry<string>>();
        UserData.RegisterType<ConfigList>();
        UserData.RegisterType<ConfigRangeFloat>();
        UserData.RegisterType<ConfigRangeInt>();
        
#if CLIENT
        UserData.RegisterType<IConfigControl>();
        UserData.RegisterType<ConfigControl>();
        UserData.RegisterType<IDisplayable>();
#endif
        // Statics
        UserData.RegisterType(typeof(ConfigManager));
        UserData.RegisterType(typeof(NetworkingManager));
        UserData.RegisterType(typeof(MemoryCallbackCache));
        GameMain.LuaCs.Lua.Globals[nameof(ConfigManager)] = UserData.CreateStatic(typeof(ConfigManager));
        GameMain.LuaCs.Lua.Globals[nameof(NetworkingManager)] = UserData.CreateStatic(typeof(NetworkingManager));
        GameMain.LuaCs.Lua.Globals[nameof(MemoryCallbackCache)] = UserData.CreateStatic(typeof(MemoryCallbackCache));
    }

    private void DeregisterLua()
    {
        // Note: As of Barotrauma LuaCs 1.0.74, type de-registration is automatic. 
        // Deregister in reverse order
        try
        {
            GameMain.LuaCs.Lua.Globals[nameof(ConfigManager)] = null;
            GameMain.LuaCs.Lua.Globals[nameof(NetworkingManager)] = null;
            GameMain.LuaCs.Lua.Globals[nameof(MemoryCallbackCache)] = null;
        }
        catch
        {
            //continue
        }
    }

    private void PrintAllConfigVars()
    {
        foreach (var configVar in ConfigManager.GetAllConfigMembers())
        {
            LuaCsSetup.PrintCsMessage($"ConfigVar: \"{configVar.ModName}:{configVar.Name}\" Type={configVar.SubTypeDef} Value={configVar.GetStringValue()}");
        }
    }

    // ReSharper disable once UnusedMember.Local
    private void UpdateConfigVarByString(string modname, string name, string newValue)
    {
        IConfigBase? entry = ConfigManager.GetConfigMember(modname, name);
        if (entry is null)
        {
            LuaCsSetup.PrintCsMessage($"Could not find a config var by the name of {modname}::{name}");
            return;
        }
        if (!entry.ValidateString(newValue))
        {
            LuaCsSetup.PrintCsMessage($"Value of {newValue} is not valid for {modname}::{name}");
            return;
        }
        entry.SetValueFromString(newValue);
    }

    private void OnPatchStateUpdate(bool isLoaded)
    {
        if (isLoaded)
        {
            LuaCsSetup.PrintCsMessage("ModConfigManager: Patches Loaded.");
        }
        else
        {
            LuaCsSetup.PrintCsMessage("ModConfigManager: Patches Unloaded.");
        }
    }

    private void UnloadPatches()
    {
        PatchManager.Unload();
        PatchManager.OnPatchStateUpdate -= OnPatchStateUpdate;
    }

    public void Dispose()
    {
#if CLIENT
        LuaCsSetup.PrintCsMessage($"ModConfigMenu: Dispose called.");
        Barotrauma.SettingsMenu.Instance?.Close();
        Barotrauma.SettingsMenu.Instance = null;
#endif
        
        NetworkingManager.Dispose();
        ConfigManager.Dispose();
        XMLDocumentHelper.UnloadCache();
        ConsoleCommands.UnloadAllCommands();
        UnloadPatches();
        PatchManager.Dispose();
        DeregisterLua();
        IsLoaded = false;
    }
}