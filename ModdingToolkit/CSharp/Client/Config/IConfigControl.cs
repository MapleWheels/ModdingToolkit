using System.Runtime.Serialization;

namespace ModdingToolkit.Config;

public interface IConfigControl : IConfigBase
{
    KeyOrMouse? Value { get; set; }
    KeyOrMouse DefaultValue { get; }
    void Initialize(string name, string modName, KeyOrMouse? currentValue, KeyOrMouse? defaultValue, System.Action? onValueChanged);
    bool Validate(KeyOrMouse newValue);
}