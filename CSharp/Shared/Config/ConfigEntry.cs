namespace ModdingToolkit.Config;

public class ConfigEntry<T> : IConfigEntry<T> where T : IConvertible
{
    #region INTERNALS

    protected T _value;
    protected Func<T, bool> _valueChangePredicate;
    protected System.Action _onValueChanged;

    #endregion

    public string Name { get; private set; }

    public Type SubTypeDef => typeof(T);
    public string ModName { get; private set; }

    public virtual T Value
    {
        get => _value;
        set
        {
            if (_valueChangePredicate?.Invoke(value) ?? true)
            {
                _value = value;
                _onValueChanged?.Invoke();
            }
        }
    }

    public T DefaultValue { get; private set; }

    public IConfigEntry<T>.Category MenuCategory { get; private set; }
    public IConfigEntry<T>.NetworkSync NetSync { get; private set; }

    public bool IsInitialized { get; private set; } = false;

    public ConfigEntry(T defaultValue,
        IConfigEntry<T>.Category menuCategory = IConfigEntry<T>.Category.Gameplay,
        IConfigEntry<T>.NetworkSync netSync = IConfigEntry<T>.NetworkSync.NoSync)
    {
        DefaultValue = defaultValue;
    }

    public virtual void Initialize(string name, string modName, T newValue)
    {
        if (name.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<{typeof(T).Name}>::Initialize() | Name is null or empty.");
        if (modName.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<{typeof(T).Name}>::Initialize() | ModName is null or empty.");

        Name = name;
        ModName = modName;
        Value = newValue;

        IsInitialized = true;
    }

    public virtual bool Validate(T value) => _valueChangePredicate?.Invoke(value) ?? true;

    public virtual string GetStringValue() => _value.ToString() ?? "";

    public virtual string GetStringDefaultValue() => DefaultValue.ToString() ?? "";

    public virtual void SetValueFromString(string value)
    {
        try
        {
            this.Value = (T)Convert.ChangeType(value, typeof(T));
        }
        catch (InvalidCastException ice)
        {
            LuaCsSetup.PrintCsError(
                $"ConfigEntry::SetValueFromString() | Name: {Name}. ModName: {ModName}. Cannot convert from string value {value} to {typeof(T)}");
        }
    }

    public void SetValueAsDefault()
    {
        this.Value = DefaultValue;
    }

    public virtual IConfigBase.DisplayType GetDisplayType() => IConfigBase.DisplayType.Standard;
}