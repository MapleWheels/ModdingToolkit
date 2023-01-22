using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public interface IConfigRangeFloat : IConfigEntry<float>
{
    public float MinValue { get; }
    public float MaxValue { get; }
    public int Steps { get; }

    void Initialize(string name, string modName, float newValue, float defaultValue, float minValue, float maxValue, int steps,
        Func<float, bool>? valueChangePredicate = null,
        Action<IConfigRangeFloat>? onValueChanged = null,
        string? displayName = null,
        string? displayModname = null,
        string? displayCategory = null);
}