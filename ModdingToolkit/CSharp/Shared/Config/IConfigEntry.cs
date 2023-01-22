using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public interface IConfigEntry<T> : IConfigBase where T : IConvertible
{
    public T Value { get; set; }
    public T DefaultValue { get; }

    void Initialize(string name, string modName, T newValue, T defaultValue,
        Func<T, bool>? valueChangePredicate = null,
        Action<IConfigEntry<T>>? onValueChanged = null,
        string? displayName = null,
        string? displayModName = null,
        string? displayCategory = null);
    bool Validate(T value);
}


