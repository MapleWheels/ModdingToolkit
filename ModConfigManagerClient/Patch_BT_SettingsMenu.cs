namespace ModdingToolkit.Patches;

public static class Patch_BT_SettingsMenu<T> where T : class, ISettingsMenu
{
    public static bool Prefix_Create(ref Barotrauma.SettingsMenu __result, RectTransform mainParent)
    {
        if (typeof(Barotrauma.SettingsMenu).IsAssignableFrom(typeof(T)))
        {
            Barotrauma.SettingsMenu.Instance?.Close();
            Barotrauma.SettingsMenu newInst
                = Unsafe.As<SettingsMenu>(Activator.CreateInstance(typeof(T), new object[] { mainParent }))!;
            Barotrauma.SettingsMenu.Instance = newInst;
            __result = newInst;
            return false;
        }

        return true;
    }
    
    public static bool Prefix_CreateControlsTab(Barotrauma.SettingsMenu __instance)
    {
        if (__instance is T inst)
        {
            inst.CreateControlsTab();
            return false;
        }
        return true;
    }

    public static bool Prefix_CreateGameplayTab(Barotrauma.SettingsMenu __instance)
    {
        if (__instance is T inst)
        {
            inst.CreateGameplayTab();
            return false;
        }
        return true;
    }
    
    public static bool Prefix_CreateGraphicsTab(Barotrauma.SettingsMenu __instance)
    {
        if (__instance is T inst)
        {
            inst.CreateGraphicsTab();
            return false;
        }
        return true;
    }

    public static bool Prefix_CreateAudioAndVCTab(Barotrauma.SettingsMenu __instance)
    {
        if (__instance is T inst)
        {
            inst.CreateAudioAndVCTab();
            return false;
        }
        return true;
    }

    public static bool Prefix_Close(Barotrauma.SettingsMenu __instance)
    {
        if (__instance is T inst)
        {
            inst.Close();
            return false;
        }
        return true;
    }

}