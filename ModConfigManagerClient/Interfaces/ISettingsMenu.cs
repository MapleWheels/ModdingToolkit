
namespace ModConfigManager;

public interface ISettingsMenu
{
    void CreateAudioAndVCTab();
    void CreateControlsTab();
    void CreateGameplayTab();
    void CreateGraphicsTab();
    void Close();
}