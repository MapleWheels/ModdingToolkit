namespace ModdingToolkit.Config;

public sealed class ConfigEntry<T> : IConfigEntry<T> where T : IConvertible
{
    #region INTERNALS

    private T _value;
    private Func<T, bool> _valueChangePredicate;
    private System.Action _onValueChanged;

    #endregion
    
    public string Name { get; private set; }
    public string ModName { get; private set; }

    public T Value
    {
        get => _value;
        set
        {
            if (_valueChangePredicate?.Invoke(value) ?? true)
            {
                _value = value;
                _onValueChanged?.Invoke();
            }
        }
    }
    public IConfigEntry<T>.Category MenuCategory { get; private set; }
    public IConfigEntry<T>.NetworkSync NetSync { get; private set; }
    public bool SaveOnValueChanged { get; private set; }
    public bool IsInitialized { get; private set; } = false;
    
    public ConfigEntry(T defaultValue, 
        IConfigEntry<T>.Category menuCategory = IConfigEntry<T>.Category.Gameplay, 
        IConfigEntry<T>.NetworkSync netSync = IConfigEntry<T>.NetworkSync.NoSync)
    {
        
    }

    public void Initialize(string name, string modName, T newValue)
    {
        if (name.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<{typeof(T).Name}>::Initialize() | Name is null or empty");
        if (modName.Trim().IsNullOrEmpty())
            throw new ArgumentNullException($"ConfigEntry<{typeof(T).Name}>::Initialize() | ModName is null or empty");

        Name = name;
        ModName = modName;
        Value = newValue;

        IsInitialized = true;
    }

    public bool Validate(T value) => _valueChangePredicate?.Invoke(value) ?? true;

    public string GetStringValue() => _value.ToString() ?? "";
}