namespace ModdingToolkit.Patches;

public interface IPatchable
{
    List<PatchManager.PatchData> GetPatches();
}