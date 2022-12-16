namespace ModConfigManager;

public static class Patch_BT_SettingsMenu<T> where T : class, ISettingsMenu
{
    //Anti-recursion exit vars
    private static bool HCall_Create = false;
    private static bool HCall_Controls = false;
    private static bool HCall_Gameplay = false;
    private static bool HCall_Graphics = false;
    private static bool HCall_Audio = false;
    private static bool HCall_Close = false;
    
    public static bool Prefix_Create(ref Barotrauma.SettingsMenu __result, RectTransform mainParent)
    {
        if (typeof(Barotrauma.SettingsMenu).IsAssignableFrom(typeof(T)))
        {
            if (!HCall_Create)
            {
                HCall_Create = true;
                SettingsMenu.Instance?.Close();
                SettingsMenu newInst
                    = (SettingsMenu)Activator.CreateInstance(
                        typeof(T), mainParent, null)!;
                SettingsMenu.Instance = newInst;
                __result = newInst;
                HCall_Create = false;
                return false;
            }
        }
        
        return true;
    }
    
    public static bool Prefix_CreateControlsTab(Barotrauma.SettingsMenu __instance)
    {
        #warning TODO: Remove debug statements.
        Barotrauma.LuaCsSetup.PrintCsMessage("MCMC: CreateControlsTab.");
        if (__instance is T inst)
        {
            if (!HCall_Controls)
            {
#warning TODO: Remove debug statements.
                HCall_Controls = true;
                LuaCsSetup.PrintCsMessage("MCMC: CCT Overridden.");
                inst.CreateControlsTab();
                HCall_Controls = false;
                return false;
            }
        }
        return true;
    }

    public static bool Prefix_CreateGameplayTab(Barotrauma.SettingsMenu __instance)
    {
        if (__instance is T inst)
        {
            if (!HCall_Gameplay)
            {
                HCall_Gameplay = true;
                inst.CreateGameplayTab();
                HCall_Gameplay = false;
                return false;
            }
        }
        return true;
    }
    
    public static bool Prefix_CreateGraphicsTab(Barotrauma.SettingsMenu __instance)
    {
#warning TODO: Remove debug statements.
        Barotrauma.LuaCsSetup.PrintCsMessage("MCMC: CreateControlsTab.");
        if (__instance is T inst)
        {
            if (!HCall_Graphics)
            {
#warning TODO: Remove debug statements.
                HCall_Graphics = true;
                LuaCsSetup.PrintCsMessage("MCMC: CCT Overridden.");
                inst.CreateGraphicsTab();
                HCall_Graphics = false;
                return false;
            }
        }
        return true;
    }

    public static bool Prefix_CreateAudioAndVCTab(Barotrauma.SettingsMenu __instance)
    {
        if (__instance is T inst)
        {
            if (!HCall_Audio)
            {
                HCall_Audio = true;
                inst.CreateAudioAndVCTab();
                HCall_Audio = false;
                return false;
            }
        }
        return true;
    }

    public static bool Prefix_Close(Barotrauma.SettingsMenu __instance)
    {
        if (__instance is T inst)
        {
            if (!HCall_Close)
            {
                HCall_Close = true;
                inst.Close();
                HCall_Close = false;
                return false;
            }
        }
        return true;
    }

}