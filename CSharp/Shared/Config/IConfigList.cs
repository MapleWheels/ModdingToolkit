namespace ModdingToolkit.Config;

public interface IConfigList<T> : IConfigBase where T : IConvertible
{
    public T Value { get; set; }
    public T DefaultValue { get; }
    ref readonly ImmutableList<T> GetReadonlyList();
    void Initialize(string name, string modName, T newValue, T defaultValue, List<T> valueList, 
        IConfigBase.NetworkSync sync = IConfigBase.NetworkSync.NoSync, 
        IConfigBase.Category menuCategory = IConfigBase.Category.Gameplay, 
        Func<T, bool>? valueChangePredicate = null,
        Action? onValueChanged = null);
    bool Validate(T value);
}