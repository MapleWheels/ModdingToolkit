using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Linq;
using Barotrauma;
using HarmonyLib;

namespace ModConfigManager;

public sealed class Bootloader : ACsMod
{
    public bool IsLoaded { get; private set; }

    private Harmony _instance;
    
    private static Dictionary<Tab, GUIFrame>? menuTabs;

    public Bootloader()
    {
        DebugConsole.LogError($"ModConfigManager: Loaded.");

        PatchSettingsMenu();
    }

    private void PatchSettingsMenu()
    {

    }
    
    //copied from main menu layout-cast
    private enum Tab
    {
        NewGame = 0,
        LoadGame = 1,
        HostServer = 2,
        Settings = 3,
        Tutorials = 4,
        JoinServer = 5,
        CharacterEditor = 6,
        SubmarineEditor = 7,
        SteamWorkshop = 8,
        Credits = 9,
        Empty = 10
    }
    
    public override void Stop()
    {
        
    }
}