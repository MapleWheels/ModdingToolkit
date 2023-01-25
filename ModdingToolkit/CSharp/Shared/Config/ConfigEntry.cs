using System.ComponentModel;
using Barotrauma.Networking;
using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigEntry<T> : IConfigEntry<T>, INetConfigBase where T : IConvertible
{
    #region INTERNALS
    
    protected T _value = default!;
    protected Func<T, bool>? _valueChangePredicate;
    protected System.Action<IConfigEntry<T>>? _onValueChanged;
    protected System.Action<INetConfigBase>? _onNetworkEvent;

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
                this._onValueChanged?.Invoke(this);
                this.TriggerNetEvent();
            }
        }
    }

    public T DefaultValue { get; private set; } = default!;
    public NetworkSync NetSync { get; private set; }

    public void SetNetworkingId(Guid id)
    {
        NetId = id;
    }

    public Guid NetId { get; private set; }
    
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool IsInitialized { get; private set; } = false;

    public virtual void Initialize(string name, string modName, T newValue, T defaultValue,
        Func<T, bool>? valueChangePredicate = null,
        Action<IConfigEntry<T>>? onValueChanged = null)
    {
        if (name.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<{typeof(T).Name}>::Initialize() | Name is null or empty.");
        if (modName.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<{typeof(T).Name}>::Initialize() | ModName is null or empty.");

        this.Name = name;
        this.ModName = modName;
        this.DefaultValue = defaultValue;
        this._value = this.Validate(newValue) ? newValue : this.DefaultValue;

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
            if (typeof(T) == typeof(string))
            {
                this.Value = (T)(object)value;
                return;
            }
            
            var conv = TypeDescriptor.GetConverter(typeof(T));
            T? val = (T?)conv.ConvertFromString(value);
            if (val is not null)
                this.Value = val;
            else
                Utils.Logging.PrintError($"ConfigEntry::SetValueFromString() | Name: {Name}. ModName: {ModName}. " +
                                         $"Cannot convert from string value {value} to {typeof(T)}.");
        }
        catch (Exception e)
        {
            Utils.Logging.PrintError(
                $"ConfigEntry::SetValueFromString() | Name: {Name}. ModName: {ModName}. " +
                $"Cannot convert from string value {value} to {typeof(T)}. EXCEPTION: {e.Message}. INNER_EXCEPTION: {e.InnerException}");
        }
    }

    public void SetValueAsDefault()
    {
        this.Value = this.DefaultValue;
    }

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

    bool INetConfigBase.WriteNetworkValue(IWriteMessage msg)
    {
        Utils.Networking.WriteNetValueFromType(msg, this.Value);
        return true;
    }

    bool INetConfigBase.ReadNetworkValue(IReadMessage msg)
    {
        try
        {
            T value = Utils.Networking.ReadNetValueFromType<T>(msg);
            if (Validate(value))
            {
                this._value = value;
                this._onValueChanged?.Invoke(this);
            }

            return true;
        }
        catch (Exception e)
        {
            Utils.Logging.PrintError($"ConfigEntry::ReadNetworkValue() | Bad read. ModName={this.ModName}, Name={this.Name}");
            return false;
        }
    }

    void INetConfigBase.SubscribeToNetEvents(Action<INetConfigBase> evtHandle)
    {
        this._onNetworkEvent += evtHandle;
    }

    void INetConfigBase.UnsubscribeFromNetEvents(Action<INetConfigBase> evtHandle)
    {
        this._onNetworkEvent -= evtHandle;
    }

    public void InitializeNetworking(Guid netId, NetworkSync networkSync)
    {
        this.NetId = netId;
        this.NetSync = networkSync;
    }
}