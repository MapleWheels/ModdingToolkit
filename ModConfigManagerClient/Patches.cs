using ModdingToolkit.Patches;

namespace ModConfigManager;

//public sealed class Patches : IPatchable // interface search not working.
public static class Patches 
{
    //public List<PatchManager.PatchData> GetPatches()
    public static List<PatchManager.PatchData> GetPatches()
    {
        Barotrauma.LuaCsSetup.PrintCsMessage("MCMC: Patches loading.");
        return new List<PatchManager.PatchData>()
        {
            new (
                AccessTools.DeclaredMethod(typeof(Barotrauma.SettingsMenu), "Create"),
                new HarmonyMethod(AccessTools.DeclaredMethod(
                    typeof(Patch_BT_SettingsMenu<MSettingsMenu>),
                    nameof(Patch_BT_SettingsMenu<MSettingsMenu>.Prefix_Create))),
                null),
            new (
                AccessTools.DeclaredMethod(typeof(Barotrauma.SettingsMenu), "CreateAudioAndVCTab"),
                new HarmonyMethod(AccessTools.DeclaredMethod(
                    typeof(Patch_BT_SettingsMenu<MSettingsMenu>),
                    nameof(Patch_BT_SettingsMenu<MSettingsMenu>.Prefix_CreateAudioAndVCTab))),
                null),
            new (
                AccessTools.DeclaredMethod(typeof(Barotrauma.SettingsMenu), "CreateControlsTab"),
                new HarmonyMethod(AccessTools.DeclaredMethod(
                    typeof(Patch_BT_SettingsMenu<MSettingsMenu>),
                    nameof(Patch_BT_SettingsMenu<MSettingsMenu>.Prefix_CreateControlsTab))),
                null),
            new (
                AccessTools.DeclaredMethod(typeof(Barotrauma.SettingsMenu), "CreateGameplayTab"),
                new HarmonyMethod(AccessTools.DeclaredMethod(
                    typeof(Patch_BT_SettingsMenu<MSettingsMenu>),
                    nameof(Patch_BT_SettingsMenu<MSettingsMenu>.Prefix_CreateGameplayTab))),
                null),
            new (
                AccessTools.DeclaredMethod(typeof(Barotrauma.SettingsMenu), "CreateGraphicsTab"),
                new HarmonyMethod(AccessTools.DeclaredMethod(
                    typeof(Patch_BT_SettingsMenu<MSettingsMenu>),
                    nameof(Patch_BT_SettingsMenu<MSettingsMenu>.Prefix_CreateGraphicsTab))),
                null),
            new (
                AccessTools.DeclaredMethod(typeof(Barotrauma.SettingsMenu), "Close"),
                new HarmonyMethod(AccessTools.DeclaredMethod(
                    typeof(Patch_BT_SettingsMenu<MSettingsMenu>),
                    nameof(Patch_BT_SettingsMenu<MSettingsMenu>.Prefix_Close))),
                null),
            new (
                AccessTools.DeclaredMethod(typeof(Barotrauma.SettingsMenu), "ApplyInstalledModChanges"),
                new HarmonyMethod(AccessTools.DeclaredMethod(
                    typeof(Patch_BT_SettingsMenu<MSettingsMenu>),
                    nameof(Patch_BT_SettingsMenu<MSettingsMenu>.Prefix_ApplyInstalledModChanges))),
                null)
        };
    }
}