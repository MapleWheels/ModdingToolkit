using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public interface IConfigEntry<T> : IConfigBase where T : IConvertible
{
    public T Value { get; set; }
    public T DefaultValue { get; }

    void Initialize(string name, string modName, T newValue, T defaultValue, 
        NetworkSync sync = NetworkSync.NoSync, 
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay, 
        Func<T, bool>? valueChangePredicate = null,
        Action? onValueChanged = null);
    bool Validate(T value);
}


