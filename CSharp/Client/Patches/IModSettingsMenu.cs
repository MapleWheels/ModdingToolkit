namespace ModConfigManager.Client.Patches;

internal interface IModSettingsMenu
{
    void ReloadModMenu();
    void Dispose();

    void CreateControlsTab(
        SettingsMenu instance,
        Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters,
        GUIFrame mainFrame,
        GUILayoutGroup tabber,
        GUIFrame contentFrame,
        GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes);

    void CreateGameplayTab(
        SettingsMenu instance,
        Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters,
        GUIFrame mainFrame,
        GUILayoutGroup tabber,
        GUIFrame contentFrame,
        GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes);
    
    void CreateGraphicsTab(
        SettingsMenu instance,
        Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters,
        GUIFrame mainFrame,
        GUILayoutGroup tabber,
        GUIFrame contentFrame,
        GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes);


}