namespace ModdingToolkit.Config;

public interface IConfigBase
{
    string Name { get; }
    Type SubTypeDef { get; }
    string ModName { get; }
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

    public enum NetworkSync
    {
        NoSync, ServerAuthority, ClientPermissive
    }

    public enum DisplayType
    {
        DropdownEnum, DropdownList, KeyOrMouse, Standard
    }
}