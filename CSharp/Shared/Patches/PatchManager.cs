namespace ModdingToolkit.Patches;

public static class PatchManager
{
    public static Action<bool>? OnPatchStateUpdate;
    public record PatchData(MethodInfo orig, HarmonyMethod? pre, HarmonyMethod? post);
    
    private static readonly Harmony Instance = new Harmony("com.ModConfigManager.PatchManager");
    private static bool IsLoaded = false;

    private static readonly List<PatchData> PatchList = new List<PatchData>();

    public static void AddPatchData(PatchData patchData) => PatchList.Add(patchData);

    /// <summary>
    /// Clears all stored patches. WARNING: Unloads all patches first! 
    /// </summary>
    public static void ClearPatches()
    {
        Unload();
        PatchList.Clear();
    }

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