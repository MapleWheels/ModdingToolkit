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
    private static bool HCall_ApplyModSettings = false;
    
    public static bool Prefix_Create(ref Barotrauma.SettingsMenu __result, RectTransform mainParent)
    {
        if (typeof(Barotrauma.SettingsMenu).IsAssignableFrom(typeof(T)))
        {
            LuaCsSetup.PrintCsMessage("MCMC: Create Called.");
            if (!HCall_Create)
            { 
                HCall_Create = true;
                MethodInfo? mi = AccessTools.DeclaredMethod(typeof(T), "Create");
                if (mi is not null)
                {
                    try
                    {
                        __result = Unsafe.As<Barotrauma.SettingsMenu>(mi.Invoke(null, new object?[]
                        {
                            mainParent
                        }))!;
                    }
                    catch (Exception e)
                    {
                        LuaCsSetup.PrintCsMessage($"MCMC: Create Err: MSG: {e.Message}. Inner: {e.InnerException?.Message}. Stack: {e.StackTrace}");
                    }
                }
                
                HCall_Create = false;
                return false;
            }
        }
        
        return true;
    }
    
    public static bool Prefix_CreateControlsTab(Barotrauma.SettingsMenu __instance)
    {
        if (__instance is T inst)
        {
            if (!HCall_Controls)
            {
                HCall_Controls = true;
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
        if (__instance is T inst)
        {
            if (!HCall_Graphics)
            {
                HCall_Graphics = true;
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

    public static bool Prefix_ApplyInstalledModChanges(Barotrauma.SettingsMenu __instance)
    {
        if (__instance is T inst)
        {
            if (!HCall_ApplyModSettings)
            {
                HCall_ApplyModSettings = true;
                inst.ApplyInstalledModChanges();
                HCall_ApplyModSettings = false;
                return false;
            }
        }
        return true;
    }

}