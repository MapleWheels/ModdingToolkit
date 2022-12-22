using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;

namespace ModdingToolkit.Config;

public sealed class ConfigControl : IConfigControl
{
    private event System.Func<KeyOrMouse, bool>? _validateInput;
    private event System.Action? _onValueChanged;
    public string Name { get; set; } = String.Empty;
    public Type SubTypeDef => typeof(KeyOrMouse);
    public string ModName { get; set; } = String.Empty;
    public IConfigBase.Category MenuCategory => IConfigBase.Category.Ignore;
    public IConfigBase.NetworkSync NetSync => IConfigBase.NetworkSync.NoSync;

    public string GetStringValue()
    {
        if (this.Value is null)
            return GetStringDefaultValue();
        return this.Value.MouseButton == MouseButton.None 
            ? this.Value.Key.ToString() : this.Value.MouseButton.ToString();
    }

    public string GetStringDefaultValue() => this.DefaultValue.MouseButton == MouseButton.None 
        ? this.DefaultValue.Key.ToString() 
        : this.DefaultValue.MouseButton.ToString();
    
    public void SetValueFromString(string value)
    {
        if (Enum.IsDefined(typeof(Keys), value))
        {
            Keys k = Enum.Parse<Keys>(value);
            this.Value = new KeyOrMouse(k);
        }
        else if (Enum.IsDefined(typeof(MouseButton), value))
        {
            MouseButton mb = Enum.Parse<MouseButton>(value);
            this.Value = new KeyOrMouse(mb);
        }
    }

    public void SetValueAsDefault()
    {
        if (DefaultValue.MouseButton == MouseButton.None)
            this.Value = new KeyOrMouse(DefaultValue.Key);
        else
            this.Value = new KeyOrMouse(DefaultValue.MouseButton);
    }

    public IConfigBase.DisplayType GetDisplayType() => IConfigBase.DisplayType.KeyOrMouse;

    private KeyOrMouse? _value;
    public KeyOrMouse? Value
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

    public KeyOrMouse DefaultValue { get; private set; } = new KeyOrMouse(Keys.NumLock);

    public void Initialize(string name, string modName, KeyOrMouse? currentValue, KeyOrMouse? defaultValue, System.Action? onValueChanged)
    {
        if (name.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigControl::Initialize() | Name is null or empty.");
        if (modName.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigControl::Initialize() | ModName is null or empty.");

        this.Name = name;
        this.ModName = modName;
        this.Value = currentValue;
        if (defaultValue is not null)
            this.DefaultValue = defaultValue;
    }

    public bool Validate(KeyOrMouse? newValue)
    {
        if (newValue is null)
            return false;
        return this._validateInput?.Invoke(newValue) ?? true;
    }

    public bool ValidateString(string value)
    {
        if (Enum.IsDefined(typeof(Keys), value))
            return true;
        if (Enum.IsDefined(typeof(MouseButton), value))
            return true;
        return false;
    }
}