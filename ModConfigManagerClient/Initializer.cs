using ModdingToolkit.Patches;


[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("NetScriptAssembly")]
namespace ModdingToolkit;

public sealed class Initializer : IPatchable, IAssemblyPlugin
{
    public List<PatchManager.PatchData> GetPatches()
    {
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
                null)
        };
    }

    public void Initialize()
    {
        LuaCsSetup.PrintCsMessage($"MCMC: Init called.");
    }

    public void OnLoadCompleted()
    {
        LuaCsSetup.PrintCsMessage($"MCMC: OnLoadCompleted called.");
    }

    public PluginInfo GetPluginInfo()
    {
        LuaCsSetup.PrintCsMessage($"MCMC: GetPluginInfo called.");
        return new PluginInfo("ModConfigManagerClient", "0.0.0.0", ImmutableArray<string>.Empty);
    }

    public void Dispose()
    {
        LuaCsSetup.PrintCsMessage($"MCMC: Dispose called.");
    }
}