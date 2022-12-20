using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;

namespace ModdingToolkit.Config;

public sealed class ConfigControl : IConfigControl
{
    private event System.Func<KeyOrMouse, bool>? _validateInput;
    private event System.Action? _onValueChanged;
    public string Name { get; set; }
    public Type SubTypeDef => typeof(KeyOrMouse);
    public string ModName { get; set; }
    public IConfigBase.Category MenuCategory => IConfigBase.Category.Ignore;
    public IConfigBase.NetworkSync NetSync => IConfigBase.NetworkSync.NoSync;

    public string GetStringValue()
    {
        if (Value is null)
            return GetStringDefaultValue();
        return Value.MouseButton == MouseButton.None ? Value.Key.ToString() : Value.MouseButton.ToString();
    }

    public string GetStringDefaultValue() => DefaultValue.MouseButton == MouseButton.None 
        ? DefaultValue.Key.ToString() : DefaultValue.MouseButton.ToString();
    
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
        this.Value = DefaultValue;
    }

    public IConfigBase.DisplayType GetDisplayType() => IConfigBase.DisplayType.KeyOrMouse;

    private KeyOrMouse? _value;
    public KeyOrMouse? Value
    {
        get => _value;
        set
        {
            if (Validate(value))
                _value = value;
        }
    }
    public KeyOrMouse DefaultValue { get; set; }
    public bool SaveOnValueChanged { get; }
    
    public void Initialize(string name, string modName, KeyOrMouse? currentValue, KeyOrMouse? defaultValue, System.Action? onValueChanged)
    {
        if (name.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigControl::Initialize() | Name is null or empty.");
        if (modName.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigControl::Initialize() | ModName is null or empty.");

        Name = name;
        ModName = modName;
        Value = currentValue;
        if (defaultValue is not null)
            DefaultValue = defaultValue;
    }

    public bool Validate(KeyOrMouse? newValue)
    {
        if (newValue is null)
            return false;
        return _validateInput?.Invoke(newValue) ?? true;
    }
}