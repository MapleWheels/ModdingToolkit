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
        /// <summary>
        /// Does not synchronize between the Client and Server
        /// </summary>
        NoSync, 
        /// <summary>
        /// Only the server can make changes.
        /// </summary>
        ServerAuthority, 
        /// <summary>
        /// The client is allowed to make changes BUT will not be synced to the server. Any changes made by the server are synced to clients.
        /// </summary>
        ClientPermissiveDesync, 
        /// <summary>
        /// Any changes made by either the client or the server will be synced.
        /// </summary>
        TwoWaySync
    }

    public enum DisplayType
    {
        DropdownEnum, DropdownList, KeyOrMouse, Number, Slider, Standard, Tickbox
    }
}