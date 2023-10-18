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
    /// Unloads all patches and clears them. 
    /// </summary>
    public static void ClearPatches()
    {
        Unload();
        PatchList.Clear();
    }

    public static void RegisterPatches(List<PatchData> pdata)
    {
        PatchList.AddRange(pdata);
        if (IsLoaded)
        {
            foreach (PatchData p in pdata)
                Instance.Patch(p.orig, p.pre, p.post);
        }
    }

    public static void RegisterPatch(PatchData pdata)
    {
        PatchList.Add(pdata);
        if (IsLoaded)
        {
            Instance.Patch(pdata.orig, pdata.pre, pdata.post);
        }
    }

    /// <summary>
    /// Searches all loaded assemblies for IPatchable Harmony patches and saves them for managed loading/unloading.
    /// </summary>
    public static void BuildPatchList()
    {
        if (IsLoaded)
            Unload();
        try
        {
            foreach (Type patchType in LuaCsSetup.AssemblyManager.GetSubTypesInLoadedAssemblies<IPatchable>())
            {
                IPatchable? p = (IPatchable?)Activator.CreateInstance(patchType);
                if (p is not null)
                    PatchList.AddRange(p.GetPatches());
            }
        }
        catch (Exception e)
        {
            Utils.Logging.PrintError($"PatchManager::BuildPatchList() | Could not load patches: {e.Message}");
        }
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

    public static void Dispose()
    {
        Unload();
        PatchList.Clear();
    }
}