namespace ModdingToolkit.Config;

public interface IConfigEntry<T> : IConfigBase where T : IConvertible
{
    public T Value { get; set; }
    public Category MenuCategory { get; }
    public NetworkSync NetSync { get; }
    bool SaveOnValueChanged { get; }

    void Initialize(string name, string modName, T newValue);
    bool Validate(T value);
    void SetValueFromString(string strvalue);

    public enum Category
    {
        Audio, Gameplay, Graphics    
    }

    public enum NetworkSync
    {
        NoSync, ServerAuthority, ClientPermissive
    }
}