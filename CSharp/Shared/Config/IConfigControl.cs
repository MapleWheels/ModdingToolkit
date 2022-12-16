using System.Runtime.Serialization;

namespace ModdingToolkit.Config;

public interface IConfigControl : IConfigBase //, ISerializable, IDisposable
{
    #warning TODO: Implement ISerializable, IDisposable intefaces.
    KeyOrMouse? Value { get; set; }
    KeyOrMouse? DefaultValue { get; }
    bool SaveOnValueChanged { get; }
    void Initialize(string name, string modName, KeyOrMouse currentValue, KeyOrMouse? defaultValue);
    bool Validate(KeyOrMouse newValue);
}