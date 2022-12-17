namespace ModdingToolkit.Config;

public interface IConfigEntry<T> : IConfigBase where T : IConvertible
{
    public T Value { get; set; }
    public T DefaultValue { get; }

    void Initialize(string name, string modName, T newValue);
    bool Validate(T value);
}


