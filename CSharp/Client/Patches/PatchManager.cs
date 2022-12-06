namespace ModConfigManager.Client.Patches;

internal static class PatchManager
{
    public static Action<bool>? OnPatchStateUpdate;
    private record PatchData(MethodInfo orig, HarmonyMethod? pre, HarmonyMethod? post);
    
    private static readonly Harmony Instance = new Harmony("com.ModConfigManager.PatchManager");
    private static bool IsLoaded = false;

    private static readonly ImmutableArray<PatchData> PatchList = ImmutableArray.Create(
        new PatchData(
            AccessTools.DeclaredMethod(typeof(Barotrauma.SettingsMenu), "CreateControlsTab"),
            new HarmonyMethod(AccessTools.DeclaredMethod(
                typeof(Patch_BT_SettingsMenu<ModSettingsMenu>), 
                nameof(Patch_BT_SettingsMenu<ModSettingsMenu>.Prefix_CreateControlsTab))),
            null),
        new PatchData(
            AccessTools.DeclaredMethod(typeof(Barotrauma.SettingsMenu), "CreateGameplayTab"),
            new HarmonyMethod(AccessTools.DeclaredMethod(
                typeof(Patch_BT_SettingsMenu<ModSettingsMenu>), 
                nameof(Patch_BT_SettingsMenu<ModSettingsMenu>.Prefix_CreateGameplayTab))),
            null),
        new PatchData(
            AccessTools.DeclaredMethod(typeof(Barotrauma.SettingsMenu), "CreateGraphicsTab"),
            new HarmonyMethod(AccessTools.DeclaredMethod(
                typeof(Patch_BT_SettingsMenu<ModSettingsMenu>), 
                nameof(Patch_BT_SettingsMenu<ModSettingsMenu>.Prefix_CreateGraphicsTab))),
            null)
    );

    public static void Load()
    {
        if (IsLoaded)
            Unload();
        PatchAll();
        IsLoaded = true;
        OnPatchStateUpdate?.Invoke(IsLoaded);
    }

    static void PatchAll()
    {
        foreach (PatchData p in PatchList)
            Instance.Patch(p.orig, p.pre, p.post);
    }

    public static void Unload()
    {
        Instance?.UnpatchAll();
        IsLoaded = false;
        OnPatchStateUpdate?.Invoke(IsLoaded);
    }
}