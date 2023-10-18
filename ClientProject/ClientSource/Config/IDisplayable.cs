namespace ModdingToolkit.Config;

public interface IDisplayable
{
    string Name { get; }
    string ModName { get; }
    string DisplayName { get; }
    string DisplayModName { get; }
    string DisplayCategory { get; }
    string Tooltip { get; }
    string ImageIcon { get; }
    Category MenuCategory { get; }

    bool ValidateString(string value);
    void SetValueAsDefault();
    void SetValueFromString(string value);
    string GetStringValue();
    string GetStringDefaultValue();
    DisplayType GetDisplayType();

    void InitializeDisplay(
        string? name = "",
        string? modName = "",
        string? displayName = "",
        string? displayModName = "",
        string? displayCategory = "",
        string? tooltip = "",
        string? imageIcon = "",
        Category menuCategory = Category.Gameplay);
}