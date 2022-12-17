namespace ModdingToolkit.Config;

public interface IConfigBase
{
    string Name { get; }
    Type SubTypeDef { get; }
    string ModName { get; }
    public Category MenuCategory { get; }
    public NetworkSync NetSync { get; }
    string GetStringValue();
    void SetValueFromString(string value);
    void SetValueAsDefault();
    DisplayType GetDisplayType();
    
    public enum Category
    {
        Audio, Gameplay, Graphics, Ignore    
    }

    public enum NetworkSync
    {
        NoSync, ServerAuthority, ClientPermissive
    }

    public enum DisplayType
    {
        Dropdown, KeyOrMouse, Slider, Standard
    }
}