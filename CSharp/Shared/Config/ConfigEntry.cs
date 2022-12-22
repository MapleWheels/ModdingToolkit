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

    public IConfigEntry<T>.Category MenuCategory { get; private set; }
    public IConfigEntry<T>.NetworkSync NetSync { get; private set; }

    public bool IsInitialized { get; private set; } = false;

    public virtual void Initialize(string name, string modName, T newValue, T defaultValue, 
        IConfigBase.NetworkSync sync = IConfigBase.NetworkSync.NoSync, 
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay, 
        Func<T, bool>? valueChangePredicate = null,
        Action? onValueChanged = null)
    {
        if (name.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<{typeof(T).Name}>::Initialize() | Name is null or empty.");
        if (modName.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<{typeof(T).Name}>::Initialize() | ModName is null or empty.");

        this.Name = name;
        this.ModName = modName;
        this.Value = newValue;
        this.DefaultValue = defaultValue;
        this.NetSync = sync;
        this.MenuCategory = menuCategory;
        if (valueChangePredicate is not null)
            this._valueChangePredicate = valueChangePredicate;
        if (onValueChanged is not null)
            this._onValueChanged = onValueChanged;

        this.IsInitialized = true;
    }

    public virtual bool Validate(T value) => this._valueChangePredicate?.Invoke(value) ?? true;

    public virtual string GetStringValue() => this._value.ToString() ?? "";

    public virtual string GetStringDefaultValue() => this.DefaultValue.ToString() ?? "";

    public virtual void SetValueFromString(string value)
    {
        try
        {
            this.Value = (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception ice)
        {
            LuaCsSetup.PrintCsError(
                $"ConfigEntry::SetValueFromString() | Name: {Name}. ModName: {ModName}. Cannot convert from string value {value} to {typeof(T)}");
        }
    }

    public void SetValueAsDefault()
    {
        this.Value = this.DefaultValue;
    }

    public virtual IConfigBase.DisplayType GetDisplayType() =>
        typeof(T) switch
        {
            { IsEnum: true } => IConfigBase.DisplayType.DropdownEnum,
            { Name: nameof(Boolean) } => IConfigBase.DisplayType.Tickbox,
            { IsPrimitive: true } => IConfigBase.DisplayType.Number,
            _ => IConfigBase.DisplayType.Standard
        };

    public bool ValidateString(string value)
    {
        try
        {
            _ = (T)Convert.ChangeType(value, typeof(T));    //try to convert & cast.
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}