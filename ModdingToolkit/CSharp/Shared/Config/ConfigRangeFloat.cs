using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public class ConfigRangeFloat : ConfigEntry<float>, IConfigRangeFloat
{
    public float MinValue { get; private set; }
    public float MaxValue { get; private set; }
    public int Steps { get; private set; }

    private Action<IConfigRangeFloat>? _onValChanged;


    public void Initialize(string name, string modName, float newValue, float defaultValue, float minValue, float maxValue, int steps,
        NetworkSync sync = NetworkSync.NoSync, IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        Func<float, bool>? valueChangePredicate = null, Action<IConfigRangeFloat>? onValueChanged = null)
    {
        if (minValue >= maxValue)
            throw new ArgumentException(
                $"ConfigRangefloat::Initialize() | The Minimum Value of {minValue} is equal or greater than {maxValue}. \nDetails: \nModName={modName} \nName={Name}");

        Steps = steps;
        MinValue = minValue;
        MaxValue = maxValue;
        _onValChanged = onValueChanged;
        
        base.Initialize(name, modName, newValue, defaultValue, sync, menuCategory, valueChangePredicate, _ =>
        {
            _onValChanged?.Invoke(this);
        });
    }

    public override bool Validate(float value) => value >= MinValue && value <= MaxValue && base.Validate(value);
    public override IConfigBase.DisplayType GetDisplayType() => IConfigBase.DisplayType.Slider;
}