using ModdingToolkit.Client;
using ModdingToolkit.Config;
using ModdingToolkit.Networking;
using ModdingToolkit.Patches;
using MoonSharp.Interpreter;

namespace ModdingToolkit;

internal sealed class Bootloader : ACsMod
{
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool IsLoaded { get; private set; }

    public Bootloader()
    {
        DebugConsole.LogError($"ModConfigManager: Starting...");
        Init();
    }

    private void Init()
    {
        RegisterLua();
        NetworkingManager.Initialize(true);
        PluginHelper.LoadAssemblies();
        LoadPatches();
        RegisterCommands();
        ConsoleCommands.ReloadAllCommands();
        NetworkingManager.SynchronizeAll();
        IsLoaded = true;
    }

    private void RegisterCommands()
    {
        // TODO: Fix console commands. This is disabled as unloading plugins requires unloading and reloading Config and Networking but there is no way to ensure compliance from ContentPackage mods. This will just leads to runtime errors for end-users.
        /*ConsoleCommands.RegisterCommand(
            "cl_reloadassemblies", 
            "Reloads all assemblies and their plugins.",
            _ =>
            {
                ConfigManager.Dispose();
                NetworkingManager.Dispose();
                XMLDocumentHelper.UnloadCache();
                UnloadPatches();
                PatchManager.Dispose();
                PluginHelper.UnloadAssemblies();
                PluginHelper.LoadAssemblies();
                NetworkingManager.Initialize(true);
                NetworkingManager.SynchronizeAll();
                LoadPatches();
            });
        
        ConsoleCommands.RegisterCommand(
            "cl_unloadassemblies", 
            "Unloads all assemblies and their plugins.",
            _ =>
            {
                ConfigManager.Dispose();
                NetworkingManager.Dispose();
                XMLDocumentHelper.UnloadCache();
                UnloadPatches();
                PatchManager.Dispose();
                PluginHelper.UnloadAssemblies();
            });*/
        
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
        UserData.RegisterType<NetworkSync>();
        UserData.RegisterType<IConfigBase.DisplayType>();
        
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
#endif
        // Statics
        UserData.RegisterType(typeof(ConfigManager));
        UserData.RegisterType(typeof(NetworkingManager));
        GameMain.LuaCs.Lua.Globals[nameof(ConfigManager)] = UserData.CreateStatic(typeof(ConfigManager));
        GameMain.LuaCs.Lua.Globals[nameof(NetworkingManager)] = UserData.CreateStatic(typeof(NetworkingManager));
    }

    private void DeregisterLua()
    {
        // Deregister in reverse order
        // Statics
        GameMain.LuaCs.Lua.Globals[nameof(ConfigManager)] = null;
        GameMain.LuaCs.Lua.Globals[nameof(NetworkingManager)] = null;
        UserData.UnregisterType(typeof(NetworkingManager));
        UserData.UnregisterType(typeof(ConfigManager));

#if CLIENT
        UserData.UnregisterType<ConfigControl>();
        UserData.UnregisterType<IConfigControl>();
#endif
        // Types
        UserData.UnregisterType<ConfigList>();
        UserData.UnregisterType<ConfigRangeFloat>();
        UserData.UnregisterType<ConfigRangeInt>();
        UserData.UnregisterType<ConfigEntry<bool>>();
        UserData.UnregisterType<ConfigEntry<byte>>();
        UserData.UnregisterType<ConfigEntry<short>>();
        UserData.UnregisterType<ConfigEntry<int>>();
        UserData.UnregisterType<ConfigEntry<sbyte>>();
        UserData.UnregisterType<ConfigEntry<ushort>>();
        UserData.UnregisterType<ConfigEntry<uint>>();
        UserData.UnregisterType<ConfigEntry<ulong>>();
        UserData.UnregisterType<ConfigEntry<long>>();
        UserData.UnregisterType<ConfigEntry<float>>();
        UserData.UnregisterType<ConfigEntry<double>>();
        UserData.UnregisterType<ConfigEntry<string>>();
        
        UserData.UnregisterType<NetworkSync>();
        UserData.UnregisterType<IConfigBase.DisplayType>();
        
        // Interfaces
        UserData.UnregisterType<IConfigList>();
        UserData.UnregisterType<IConfigRangeFloat>();
        UserData.UnregisterType<IConfigRangeInt>();
        UserData.UnregisterType<IConfigEntry<bool>>();
        UserData.UnregisterType<IConfigEntry<byte>>();
        UserData.UnregisterType<IConfigEntry<short>>();
        UserData.UnregisterType<IConfigEntry<int>>();
        UserData.UnregisterType<IConfigEntry<sbyte>>();
        UserData.UnregisterType<IConfigEntry<ushort>>();
        UserData.UnregisterType<IConfigEntry<uint>>();
        UserData.UnregisterType<IConfigEntry<ulong>>();
        UserData.UnregisterType<IConfigEntry<long>>();
        UserData.UnregisterType<IConfigEntry<float>>();
        UserData.UnregisterType<IConfigEntry<double>>();
        UserData.UnregisterType<IConfigEntry<string>>();
        UserData.UnregisterType<IConfigBase>();
        
        UserData.UnregisterType<INetConfigBase>();
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

    public override void Stop()
    {
        NetworkingManager.Dispose();
        ConfigManager.Dispose();
        XMLDocumentHelper.UnloadCache();
        ConsoleCommands.UnloadAllCommands();
        UnloadPatches();
        PatchManager.Dispose();
        PluginHelper.UnloadAssemblies();
        DeregisterLua();
        IsLoaded = false;
    }
}