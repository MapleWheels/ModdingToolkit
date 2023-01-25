using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigRangeInt : ConfigEntry<int>, IConfigRangeInt
{
    public int MinValue { get; private set; }
    public int MaxValue { get; private set; }
    
    public int Steps { get; private set; }

    private Action<IConfigRangeInt>? _onValChanged;

    public void Initialize(string name, string modName, int newValue, int defaultValue, int minValue, int maxValue, int steps,
        Func<int, bool>? valueChangePredicate = null, 
        Action<IConfigRangeInt>? onValueChanged = null)
    {
        if (minValue >= maxValue)
            throw new ArgumentException(
                $"ConfigRangeInt::Initialize() | The Minimum Value of {minValue} is equal or greater than {maxValue}. \nDetails: \nModName={modName} \nName={Name}");
        Steps = steps;
        MinValue = minValue;
        MaxValue = maxValue;
        _onValChanged = onValueChanged;
        
        base.Initialize(name, modName, newValue, defaultValue,
            valueChangePredicate, _ => _onValChanged?.Invoke(this));
    }

    public override bool Validate(int value) => value >= MinValue && value <= MaxValue && base.Validate(value);
}