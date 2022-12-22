namespace ModdingToolkit.Config;

public interface IConfigRangeInt : IConfigEntry<int>
{
    public int MinValue { get; }
    public int MaxValue { get; }
    public int Steps { get; }
    
    void Initialize(string name, string modName, int newValue, int defaultValue, int minValue, int maxValue, int steps,
        IConfigBase.NetworkSync sync = IConfigBase.NetworkSync.NoSync, 
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay, 
        Func<int, bool>? valueChangePredicate = null,
        Action? onValueChanged = null);
}