namespace ModdingToolkit.Config;

public interface IConfigList : IConfigBase
{
    public string Value { get; set; }
    public string DefaultValue { get; }
    ref readonly ImmutableList<string> GetReadOnlyList();
    void Initialize(string name, string modName, string newValue, string defaultValue, List<string> valueList, 
        IConfigBase.NetworkSync sync = IConfigBase.NetworkSync.NoSync, 
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay, 
        Func<string, bool>? valueChangePredicate = null,
        Action? onValueChanged = null);
    bool Validate(string value);
    public int GetDefaultValueIndex();
}