using Barotrauma;
using ModdingToolkit.Patches;

namespace ModdingToolkit;

public class MSettingsMenu : Barotrauma.SettingsMenu, ISettingsMenu
{
    public MSettingsMenu(RectTransform mainParent, GameSettings.Config setConfig = new GameSettings.Config()) : base(mainParent, setConfig)
    {
    }

    public new static Barotrauma.SettingsMenu Create(RectTransform mainParent)
    {
        Instance?.Close();
        Instance = new MSettingsMenu(mainParent);
        return Instance;
    }
    
    #region API Impl

    /// <summary>
    /// Disambiguation call implementation.
    /// </summary>
    void ISettingsMenu.CreateAudioAndVCTab()
    {
        this.CreateAudioAndVCTab();
    }

    /// <summary>
    /// Disambiguation call implementation.
    /// </summary>
    void ISettingsMenu.CreateControlsTab()
    {
        this.CreateControlsTab();
    }

    /// <summary>
    /// Disambiguation call implementation.
    /// </summary>
    void ISettingsMenu.CreateGameplayTab()
    {
        this.CreateGameplayTab();
    }

    /// <summary>
    /// Disambiguation call implementation.
    /// </summary>
    void ISettingsMenu.CreateGraphicsTab()
    {
        this.CreateGraphicsTab();
    }

    #endregion
    
    public new void CreateAudioAndVCTab()
    {
#warning TODO: Implement custom menu.
        base.CreateAudioAndVCTab();
    }
    
    public new void CreateControlsTab()
    {
#warning TODO: Implement custom menu.
        base.CreateControlsTab();
    }

    public new void CreateGameplayTab()
    {
#warning TODO: Implement custom menu.
        base.CreateGameplayTab();
    }

    public new void CreateGraphicsTab()
    {
#warning TODO: Implement custom menu.
        base.CreateGraphicsTab();
    }
}