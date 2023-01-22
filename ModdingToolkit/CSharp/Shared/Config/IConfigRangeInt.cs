using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public interface IConfigRangeInt : IConfigEntry<int>
{
    public int MinValue { get; }
    public int MaxValue { get; }
    public int Steps { get; }
    
    void Initialize(string name, string modName, int newValue, int defaultValue, int minValue, int maxValue, int steps,
        Func<int, bool>? valueChangePredicate = null,
        Action<IConfigRangeInt>? onValueChanged = null,
        string? displayName = null,
        string? displayModname = null,
        string? displayCategory = null);
}