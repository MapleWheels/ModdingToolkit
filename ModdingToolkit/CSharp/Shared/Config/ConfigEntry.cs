using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigEntry<T> : IConfigEntry<T>, INetConfigEntry<T> where T : IConvertible
{
    #region INTERNALS
    
    protected T _value = default!;
    protected Func<T, bool>? _valueChangePredicate;
    protected System.Action? _onValueChanged;
    protected System.Action<uint, T>? _onNetworkEvent;

    #endregion

    public string Name { get; private set; } = String.Empty;

    public Type SubTypeDef => typeof(T);
    public Type NetSyncVarTypeDef => typeof(T);
    public string ModName { get; private set; } = String.Empty;

    public virtual T Value
    {
        get => this._value;
        set
        {
            if (Validate(value) && NetAuthorityValidate())
            {
                this._value = value;
                this._onValueChanged?.Invoke();
                this._onNetworkEvent?.Invoke(NetId,_value);
            }
        }
    }

    public T DefaultValue { get; private set; } = default!;

    public IConfigEntry<T>.Category MenuCategory { get; private set; }
    public IConfigEntry<T>.NetworkSync NetSync { get; private set; }

    public void SetNetworkingId(uint id)
    {
        NetId = id;
    }

    public uint NetId { get; private set; }
    
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
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
        catch (Exception)
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
            var a = (T)Convert.ChangeType(value, typeof(T));    //try to convert & cast.
            return Validate(a);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool SetStringValueFromNetwork(string value)
    {
        if (!ValidateString(value))
            return false;
        try
        {
            this._value = (T)Convert.ChangeType(value, typeof(T));
            this._onValueChanged?.Invoke();
            return true;
        }
        catch (Exception)
        {
            LuaCsSetup.PrintCsError($"ConfigEntry<{typeof(T)}>::SetStringValueFromNetwork() | Unable to convert string to Native type. StringValue={value}, CName={ModName}::{Name}");
            return false;
        }
    }

    public string GetStringNetworkValue() => GetStringValue();

    public bool SetNativeValueFromNetwork(T value)
    {
        if (!Validate(value))
            return false;
        this._value = value;
        this._onValueChanged?.Invoke();
        return true;
    }

    public void SubscribeToNetEvents(Action<uint, T> evtHandle)
    {
        this._onNetworkEvent += evtHandle;
    }

    public void UnsubscribeFromNetEvents(Action<uint, T> evtHandle)
    {
        this._onNetworkEvent -= evtHandle;
    }

    public T GetNetworkValue() => this._value;
}