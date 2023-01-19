using System.Runtime.Serialization;

namespace ModdingToolkit.Config;

public interface IConfigControl : IConfigBase
{
    KeyOrMouse? Value { get; set; }
    KeyOrMouse DefaultValue { get; }
    void Initialize(string name, string modName, KeyOrMouse? currentValue, KeyOrMouse? defaultValue, System.Action? onValueChanged,
        string? displayName = null, string? displayModName = null, string? displayCategory = null);
    bool Validate(KeyOrMouse newValue);
    bool IsHit();
}