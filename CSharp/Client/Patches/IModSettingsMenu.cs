using System.Collections.Immutable;
using Barotrauma;

namespace ModConfigManager.Client.Patches;

public interface IModSettingsMenu
{
    void CreateControlsTab(
        SettingsMenu instance,
        ref Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters,
        ref GameSettings.Config unsavedConfig,
        GUIFrame mainFrame,
        GUILayoutGroup tabber,
        GUIFrame contentFrame,
        GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes);

    void CreateGameplayTab(
        SettingsMenu instance,
        ref Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters,
        ref GameSettings.Config unsavedConfig,
        GUIFrame mainFrame,
        GUILayoutGroup tabber,
        GUIFrame contentFrame,
        GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes);
    
    void CreateGraphicsTab(
        SettingsMenu instance,
        ref Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters,
        ref GameSettings.Config unsavedConfig,
        GUIFrame mainFrame,
        GUILayoutGroup tabber,
        GUIFrame contentFrame,
        GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes);
    
    
}