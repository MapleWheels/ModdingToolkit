namespace ModdingToolkit.Config;

public class ConfigList<T> : IConfigList<T> where T : IConvertible
{
    #region INTERNALS

    protected T _value;
    protected ImmutableList<T> _valueList;
    protected Func<T, bool> _valueChangePredicate;
    protected System.Action _onValueChanged;

    #endregion

    public string Name { get; private set; }

    public Type SubTypeDef => typeof(T);
    public string ModName { get; private set; }

    public virtual T Value
    {
        get => this._value;
        set
        {
            if (Validate(value))
            {
                this._value = value;
                this._onValueChanged?.Invoke();
            }
        }
    }

    public T DefaultValue { get; private set; }
    
    public ref readonly ImmutableList<T> GetReadonlyList() => ref _valueList;

    public void Initialize(string name, string modName, T newValue, T defaultValue, List<T> valueList,
        IConfigBase.NetworkSync sync = IConfigBase.NetworkSync.NoSync, IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        Func<T, bool>? valueChangePredicate = null, Action? onValueChanged = null)
    {
        if (name.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<{typeof(T).Name}>::Initialize() | Name is null or empty.");
        if (modName.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<{typeof(T).Name}>::Initialize() | ModName is null or empty.");

        this.Name = name;
        this.ModName = modName;

        this.NetSync = sync;
        this.MenuCategory = menuCategory;
        this._valueList = valueList.ToImmutableList();

        if (_valueList.Contains(newValue))
            this.Value = newValue;
        if (_valueList.Contains(defaultValue))
            this.DefaultValue = defaultValue;

        if (valueChangePredicate is not null)
            this._valueChangePredicate = valueChangePredicate;
        if (onValueChanged is not null)
            this._onValueChanged = onValueChanged;

        this.IsInitialized = true;
    }

    public IConfigEntry<T>.Category MenuCategory { get; private set; }
    public IConfigEntry<T>.NetworkSync NetSync { get; private set; }

    public bool IsInitialized { get; private set; } = false;

    public virtual bool Validate(T value) => this._valueList.Contains(value) && (this._valueChangePredicate?.Invoke(value) ?? true);

    public virtual string GetStringValue() => this._value.ToString() ?? "";

    public virtual string GetStringDefaultValue() => this.DefaultValue.ToString() ?? "";

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
        this.Value = this.DefaultValue;
    }

    public virtual IConfigBase.DisplayType GetDisplayType() => IConfigBase.DisplayType.DropdownList;
    public bool ValidateString(string value)
    {
        try
        {
            var k = (T)Convert.ChangeType(value, typeof(T));
            return Validate(k);
        }
        catch (Exception e)
        {
            return false;
        }
    }
}