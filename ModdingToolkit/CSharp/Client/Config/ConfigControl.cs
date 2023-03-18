using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;
using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public sealed class ConfigControl : IConfigControl, IDisplayable
{
    private event System.Func<KeyOrMouse, bool>? _validateInput;
    private event System.Action? _onValueChanged;
    public string Name { get; private set; } = String.Empty;
    public Type SubTypeDef => typeof(KeyOrMouse);
    public string ModName { get; private set; } = String.Empty;
    public string DisplayName { get; private set; }
    public string DisplayModName { get; private set; }
    public string DisplayCategory { get; private set; }
    public string Tooltip { get; private set; }
    public string ImageIcon { get; private set; }
    public Category MenuCategory => Category.Controls;
    public NetworkSync NetSync => NetworkSync.NoSync;

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

    public DisplayType GetDisplayType() => DisplayType.KeyOrMouse;

    public void InitializeDisplay(string? name = "", string? modName = "", string? displayName = "", string? displayModName = "",
        string? displayCategory = "", string? tooltip = "", string? imageIcon = "", Category menuCategory = Category.Gameplay)
    {
        if (!displayName.IsNullOrWhiteSpace())
            this.DisplayName = displayName;
        if (!displayModName.IsNullOrWhiteSpace())
            this.DisplayModName = displayModName;
        if (!displayCategory.IsNullOrWhiteSpace())
            this.DisplayCategory = displayCategory;
        if (!tooltip.IsNullOrWhiteSpace())
            this.Tooltip = tooltip;
        if (!imageIcon.IsNullOrWhiteSpace())
            this.ImageIcon = imageIcon;
    }

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

    public bool IsHit()
    {
        if (this.Value is null)
            return false;
        switch (this.Value.MouseButton)
        {   
            case MouseButton.None:
                return Barotrauma.PlayerInput.KeyHit(this.Value.Key);
            case MouseButton.PrimaryMouse:
                return Barotrauma.PlayerInput.PrimaryMouseButtonClicked();
            case MouseButton.SecondaryMouse:
                return Barotrauma.PlayerInput.SecondaryMouseButtonClicked();
            case MouseButton.MiddleMouse:
                return Barotrauma.PlayerInput.MidButtonClicked();
            case MouseButton.MouseButton4:
                return Barotrauma.PlayerInput.Mouse4ButtonClicked();
            case MouseButton.MouseButton5:
                return Barotrauma.PlayerInput.Mouse5ButtonClicked();
            case MouseButton.MouseWheelUp:
                return Barotrauma.PlayerInput.MouseWheelUpClicked();
            case MouseButton.MouseWheelDown:
                return Barotrauma.PlayerInput.MouseWheelDownClicked();
        }
        return false;
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