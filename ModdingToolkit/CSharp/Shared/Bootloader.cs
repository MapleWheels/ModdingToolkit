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
        IsLoaded = false;
    }
}