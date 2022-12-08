
namespace ModdingToolkit.Patches;

public interface ISettingsMenu
{
    void CreateAudioAndVCTab();
    void CreateControlsTab();
    void CreateGameplayTab();
    void CreateGraphicsTab();
    void Close();
}