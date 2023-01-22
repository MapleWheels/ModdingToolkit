using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public interface IConfigBase
{
    string Name { get; }
    Type SubTypeDef { get; }
    string ModName { get; }
    string GetStringValue();
    string GetStringDefaultValue();
    void SetValueFromString(string value);
    void SetValueAsDefault();
    bool ValidateString(string value);
}