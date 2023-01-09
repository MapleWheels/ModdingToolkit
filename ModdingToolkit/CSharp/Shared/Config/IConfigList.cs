using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public interface IConfigList : IConfigBase
{
    public string Value { get; set; }
    public string DefaultValue { get; }
    ref readonly ImmutableList<string> GetReadOnlyList();
    void Initialize(string name, string modName, string newValue, string defaultValue, List<string> valueList, 
        NetworkSync sync = NetworkSync.NoSync, 
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay, 
        Func<string, bool>? valueChangePredicate = null,
        Action? onValueChanged = null);
    bool Validate(string value);
    public int GetDefaultValueIndex();
    public void SetValueFromIndex(int index);
}