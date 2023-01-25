using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public interface IConfigEntry<T> : IConfigBase where T : IConvertible
{
    /// <summary>
    /// The internal value.
    /// </summary>
    public T Value { get; set; }
    /// <summary>
    /// The default value. Used if none other set.
    /// </summary>
    public T DefaultValue { get; }
    /// <summary>
    /// Initializes the type-behind. This is intended for use by the ConfigManager and should not be called by API consumers.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="modName"></param>
    /// <param name="newValue">The internal value.</param>
    /// <param name="defaultValue">The default internal value.</param>
    /// <param name="valueChangePredicate">Called before a new internal value is assigned. Will only assign a new internal value if this returns true.</param>
    /// <param name="onValueChanged">Called after a new internal value is assigned.</param>
    void Initialize(string name, string modName, T newValue, T defaultValue,
        Func<T, bool>? valueChangePredicate = null,
        Action<IConfigEntry<T>>? onValueChanged = null);
    /// <summary>
    /// Checks if the value is valid. This function will invoke "valueChangePredicate". Native-type internal equivalent of ValidateString();
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    bool Validate(T value);
}


