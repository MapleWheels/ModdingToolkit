using ModdingToolkit.Patches;

namespace ModdingToolkit;

public class Initializer : IPatchable
{
    public List<PatchManager.PatchData> GetPatches()
    {
        return new List<PatchManager.PatchData>()
        {
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
                null)
        };
    }
}