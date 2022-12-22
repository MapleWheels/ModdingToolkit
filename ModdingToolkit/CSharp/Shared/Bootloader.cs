using ModdingToolkit.Client;
using ModdingToolkit.Config;
using ModdingToolkit.Patches;

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
        PluginHelper.LoadAssemblies();
        LoadPatches();
        RegisterCommands();
        ConsoleCommands.ReloadAllCommands();
        IsLoaded = true;
    }

    private void RegisterCommands()
    {
        ConsoleCommands.RegisterCommand(
            "cl_reloadassemblies", 
            "Reloads all assemblies and their plugins.",
            _ =>
            {
                ConfigManager.Dispose();
                XMLDocumentHelper.UnloadCache();
                UnloadPatches();
                PatchManager.Dispose();
                PluginHelper.UnloadAssemblies();
                PluginHelper.LoadAssemblies();
                LoadPatches();
            });
        
        ConsoleCommands.RegisterCommand(
            "cl_unloadassemblies", 
            "Unloads all assemblies and their plugins.",
            _ =>
            {
                ConfigManager.Dispose();
                XMLDocumentHelper.UnloadCache();
                UnloadPatches();
                PatchManager.Dispose();
                PluginHelper.UnloadAssemblies();
            });
        
        ConsoleCommands.RegisterCommand(
            "cl_cfgsetvar",
            "Sets a config member to the supplied string. Format is <command> <modname> <name> \"<value>\"",
            argsv =>
            {
                if (argsv.Length < 1)
                {
                    LuaCsSetup.PrintCsError($"Arguments missing!");
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
                    LuaCsSetup.PrintCsError($"Arguments missing!");
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
        MoonSharp.Interpreter.UserData.RegisterType<IConfigBase>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<bool>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<byte>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<short>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<int>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<sbyte>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<ushort>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<uint>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<ulong>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<long>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<float>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<double>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<decimal>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigEntry<string>>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigList>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigRangeFloat>();
        MoonSharp.Interpreter.UserData.RegisterType<IConfigRangeInt>();
        
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<bool>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<byte>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<short>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<int>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<sbyte>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<ushort>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<uint>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<ulong>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<long>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<float>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<double>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<decimal>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigEntry<string>>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigList>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigRangeFloat>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigRangeInt>();
        
#if CLIENT
        MoonSharp.Interpreter.UserData.RegisterType<IConfigControl>();
        MoonSharp.Interpreter.UserData.RegisterType<ConfigControl>();
#endif
    }

    private void DeregisterLua()
    {
        // Deregister in reverse order
#if CLIENT
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigControl>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigControl>();
#endif
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigList>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigRangeFloat>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigRangeInt>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<bool>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<byte>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<short>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<int>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<sbyte>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<ushort>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<uint>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<ulong>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<long>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<float>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<double>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<decimal>>();
        MoonSharp.Interpreter.UserData.UnregisterType<ConfigEntry<string>>();
        
        
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigList>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigRangeFloat>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigRangeInt>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<bool>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<byte>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<short>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<int>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<sbyte>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<ushort>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<uint>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<ulong>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<long>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<float>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<double>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<decimal>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigEntry<string>>();
        MoonSharp.Interpreter.UserData.UnregisterType<IConfigBase>();
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