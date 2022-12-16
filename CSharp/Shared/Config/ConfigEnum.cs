namespace ModdingToolkit.Config;

public class ConfigEnum<T> : ConfigEntry<T> where T : Enum, IConfigEnum<T>
{
    public ConfigEnum(T defaultValue, IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay, IConfigBase.NetworkSync netSync = IConfigBase.NetworkSync.NoSync) : base(defaultValue, menuCategory, netSync)
    {
    }

    public override T Value
    {
        get => base._value;
        set
        {
            _value = value;
            _onValueChanged?.Invoke();
        }
    }

    public override bool Validate(T value) => true;

    public override void SetValueFromString(string value)
    {
        if (Enum.IsDefined(typeof(T), value))
        {
            Value = (T)Enum.Parse(typeof(T), value);
        }
    }

    public override IConfigBase.DisplayType GetDisplayType() => IConfigBase.DisplayType.Dropdown;
}