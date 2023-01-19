﻿using Barotrauma.Networking;
using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigList : IConfigList, INetConfigBase
{
    #region INTERNALS

    protected string _value = String.Empty;
    protected ImmutableList<string> _valueList = ImmutableList<string>.Empty;
    protected Func<string, bool>? _valueChangePredicate = null;
    protected System.Action<IConfigList>? _onValueChanged;
    protected System.Action<INetConfigBase>? _onNetworkEvent;

    #endregion

    public string Name { get; private set; } = String.Empty;
    public string DisplayName { get; private set; } = String.Empty;
    public string ModName { get; private set; } = String.Empty;
    public string DisplayModName { get; private set; } = String.Empty;
    public string DisplayCategory { get; private set; } = String.Empty;

    public Type SubTypeDef => typeof(string);
    public Type NetSyncVarTypeDef => typeof(ushort);
    public void SetNetworkingId(Guid id)
    {
        NetId = id;
    }

    public Guid NetId { get; private set; }
    
    public virtual string Value
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

    public string DefaultValue { get; private set; } = String.Empty;
    
    public ref readonly ImmutableList<string> GetReadOnlyList() => ref _valueList;
    
    public void Initialize(string name, string modName, string newValue, string defaultValue, List<string> valueList,
        NetworkSync sync = NetworkSync.NoSync, IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay,
        Func<string, bool>? valueChangePredicate = null, Action<IConfigList>? onValueChanged = null,
        string? displayName = null, string? displayModName = null, string? displayCategory = null)
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

        // Client menu data
        if (displayName is not null)
            this.DisplayName = displayName;
        if (displayModName is not null)
            this.DisplayModName = displayModName;
        if (displayCategory is not null)
            this.DisplayCategory = displayCategory;
        
        if (this._valueList.Count < 1)
            this._valueList = new List<string> { "" }.ToImmutableList();    //Empty lists not allowed to reduce overhead elsewhere.
        
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
    public NetworkSync NetSync { get; private set; }

    public bool IsInitialized { get; private set; } = false;

    public virtual bool Validate(string value) => this._valueList.Contains(value) && (this._valueChangePredicate?.Invoke(value) ?? true);
    public int GetDefaultValueIndex()
    {
        if (_valueList.Count < 1)
            return -1;
        for (int i = 0; i < _valueList.Count; i++)
        {
            if (_valueList[i] == DefaultValue)
                return i;
        }
        return 0;
    }

    public void SetValueFromIndex(int index)
    {
        if (index >= _valueList.Count || index < 0)
            return;
        this.Value = _valueList[index];
    }

    public virtual string GetStringValue() => this.Value.ToString() ?? "";

    public virtual string GetStringDefaultValue() => this.DefaultValue.ToString() ?? "";

    public virtual void SetValueFromString(string value) => this.Value = value;

    public void SetValueAsDefault()
    {
        this.Value = this.DefaultValue;
    }

    public virtual IConfigBase.DisplayType GetDisplayType() => IConfigBase.DisplayType.DropdownList;
    public bool ValidateString(string value) => Validate(value);

    bool INetConfigBase.WriteNetworkValue(IWriteMessage msg)
    {
        Utils.Networking.WriteNetValueFromType(msg, (ushort)_valueList.IndexOf(_value));
        return true;
    }

    bool INetConfigBase.ReadNetworkValue(IReadMessage msg)
    {
        ushort val = Utils.Networking.ReadNetValueFromType<ushort>(msg);
        if (val >= _valueList.Count)
        {
            Utils.Logging.PrintError($"ConfigList::ReadNetworkValue() | The index value of {val} is out of bounds.");
            return false;
        }
        this._value = _valueList[val];
        this._onValueChanged?.Invoke(this);
        return true;
    }

    void INetConfigBase.SubscribeToNetEvents(Action<INetConfigBase> evtHandle)
    {
        this._onNetworkEvent += evtHandle;
    }

    void INetConfigBase.UnsubscribeFromNetEvents(Action<INetConfigBase> evtHandle)
    {
        this._onNetworkEvent -= evtHandle;
    }
}