using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public interface IConfigBase
{
    string Name { get; }
    string DisplayName { get; }
    string ModName { get; }
    string DisplayModName { get; }
    string DisplayCategory { get; }
    Type SubTypeDef { get; }
    public Category MenuCategory { get; }
    public NetworkSync NetSync { get; }
    string GetStringValue();
    string GetStringDefaultValue();
    void SetValueFromString(string value);
    void SetValueAsDefault();
    DisplayType GetDisplayType();
    bool ValidateString(string value);
    
    public enum Category
    {
        Gameplay, 
        Ignore,    
        Audio_NOTIMPL,
        Graphics_NOTIMPL
    }

    public enum DisplayType
    {
        DropdownEnum, DropdownList, KeyOrMouse, Number, Slider, Standard, Tickbox
    }
}