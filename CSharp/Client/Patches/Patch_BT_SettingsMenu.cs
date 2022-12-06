namespace ModConfigManager.Client.Patches;

internal static class Patch_BT_SettingsMenu<T> where T : class, IModSettingsMenu
{
    public static T? HandlesInstance { get; set; }
    
    public static bool Prefix_CreateControlsTab(SettingsMenu __instance)
    {
        if (GetSettingsMenuParams(__instance,
                out var a, out var b,
                out var c, out var d,
                out var e, out var f,
                out var g))
        {
            HandlesInstance?.CreateControlsTab(__instance, a,  
                c, d, e, f, g);
        }
        return false;
    }

    public static bool Prefix_CreateGameplayTab(SettingsMenu __instance)
    {
        if (GetSettingsMenuParams(__instance,
                out var a, out var b,
                out var c, out var d,
                out var e, out var f,
                out var g))
        {
            HandlesInstance?.CreateGameplayTab(__instance, a,  
                c, d, e, f, g);
        }
        return false;
    }
    
    public static bool Prefix_CreateGraphicsTab(SettingsMenu __instance)
    {
        if (GetSettingsMenuParams(__instance,
                out var a, out var b,
                out var c, out var d,
                out var e, out var f,
                out var g))
        {
            HandlesInstance?.CreateGraphicsTab(__instance, a,  
                c, d, e, f, g);
        }
        return false;
    }

    static bool GetSettingsMenuParams(
        SettingsMenu? instance,
        out Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters,
        out GameSettings.Config unsavedConfig,
        out GUIFrame mainFrame,
        out GUILayoutGroup tabber,
        out GUIFrame contentFrame,
        out GUILayoutGroup bottom,
        out ImmutableHashSet<InputType> LegacyInputTypes)
    {
        if (instance is null)
        {
            inputButtonValueNameGetters = default!;
            unsavedConfig = default!;
            mainFrame = default!;
            tabber = default!;
            contentFrame = default!;
            bottom = default!;
            LegacyInputTypes = default!;
            return false;
        }

        Type btMenu = typeof(Barotrauma.SettingsMenu);
        inputButtonValueNameGetters = (Dictionary<GUIButton, Func<LocalizedString>>)
            AccessTools.DeclaredField(btMenu, "inputButtonValueNameGetters").GetValue(instance)!;
        unsavedConfig = (GameSettings.Config)
            AccessTools.DeclaredField(btMenu, "unsavedConfig").GetValue(instance)!;
        mainFrame = (GUIFrame)
            AccessTools.DeclaredField(btMenu, "mainFrame").GetValue(instance)!;
        tabber = (GUILayoutGroup)
            AccessTools.DeclaredField(btMenu, "tabber").GetValue(instance)!;
        contentFrame = (GUIFrame)
            AccessTools.DeclaredField(btMenu, "contentFrame").GetValue(instance)!;
        bottom = (GUILayoutGroup)
            AccessTools.DeclaredField(btMenu, "bottom").GetValue(instance)!;
        LegacyInputTypes = (ImmutableHashSet<InputType>)
            AccessTools.DeclaredField(btMenu, "LegacyInputTypes").GetValue(null)!;

        return true;
    }
}