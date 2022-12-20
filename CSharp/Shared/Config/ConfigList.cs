namespace ModdingToolkit.Config;

public class ConfigList : IConfigList
{
    #region INTERNALS

    protected string _value;
    protected ImmutableList<string> _valueList;
    protected Func<string, bool> _valueChangePredicate;
    protected System.Action _onValueChanged;

    #endregion

    public string Name { get; private set; }

    public Type SubTypeDef => typeof(string);
    public string ModName { get; private set; }

    public virtual string Value
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

    public string DefaultValue { get; private set; }
    
    public ref readonly ImmutableList<string> GetReadOnlyList() => ref _valueList;

    public void Initialize(string name, string modName, string newValue, string defaultValue, List<string> valueList,
        IConfigBase.NetworkSync sync = IConfigBase.NetworkSync.NoSync, IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        Func<string, bool>? valueChangePredicate = null, Action? onValueChanged = null)
    {
        if (name.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<string>::Initialize() | Name is null or empty.");
        if (modName.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<string>::Initialize() | ModName is null or empty.");

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

    public IConfigEntry<string>.Category MenuCategory { get; private set; }
    public IConfigEntry<string>.NetworkSync NetSync { get; private set; }

    public bool IsInitialized { get; private set; } = false;

    public virtual bool Validate(string value) => this._valueList.Contains(value) && (this._valueChangePredicate?.Invoke(value) ?? true);

    public virtual string GetStringValue() => this._value.ToString() ?? "";

    public virtual string GetStringDefaultValue() => this.DefaultValue.ToString() ?? "";

    public virtual void SetValueFromString(string value)
    {
        this.Value = value;
    }

    public void SetValueAsDefault()
    {
        this.Value = this.DefaultValue;
    }

    public virtual IConfigBase.DisplayType GetDisplayType() => IConfigBase.DisplayType.DropdownList;
    public bool ValidateString(string value)
    {
        return Validate(value);
    }
}