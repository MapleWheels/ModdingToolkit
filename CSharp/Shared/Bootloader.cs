using ModConfigManager.Client;
using ModConfigManager.Client.Patches;

namespace ModConfigManager;

internal sealed class Bootloader : ACsMod
{
    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsLoaded { get; private set; }

    public Bootloader()
    {
        DebugConsole.LogError($"ModConfigManager: Loaded.");
        PatchSettingsMenu();
        IsLoaded = true;
    }

    private void PatchSettingsMenu()
    {
        Patch_BT_SettingsMenu<ModSettingsMenu>.HandlesInstance = new ModSettingsMenu();
        PatchManager.OnPatchStateUpdate += OnPatchUpdate;
        PatchManager.Load();
        Patch_BT_SettingsMenu<ModSettingsMenu>.HandlesInstance.ReloadModMenu();
    }
    
    public override void Stop()
    {
        PatchManager.Unload();
        PatchManager.OnPatchStateUpdate -= OnPatchUpdate;
        IsLoaded = false;
    }

    void OnPatchUpdate(bool state) => DebugConsole.Log(state ? "ModConfigManager: Patches loaded." : "ModConfigManager: Patches unloaded.");
}