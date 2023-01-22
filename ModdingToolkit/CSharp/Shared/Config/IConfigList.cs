using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public interface IConfigList : IConfigBase
{
    public string Value { get; set; }
    public string DefaultValue { get; }
    ref readonly ImmutableList<string> GetReadOnlyList();
    void Initialize(string name, string modName, string newValue, string defaultValue, 
        List<string> valueList,
        Func<string, bool>? valueChangePredicate = null,
        Action<IConfigList>? onValueChanged = null,
        string? displayName = null,
        string? displayModName = null,
        string? displayCategory = null);
    bool Validate(string value);
    public int GetDefaultValueIndex();
    public void SetValueFromIndex(int index);
}