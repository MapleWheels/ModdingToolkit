using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public interface IConfigBase
{
    /// <summary>
    /// The name of the instance.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// If this type stores an internal value, then this should return that type.
    /// </summary>
    Type SubTypeDef { get; }
    /// <summary>
    /// The mod name collection that this instance belongs to.
    /// </summary>
    string ModName { get; }
    /// <summary>
    /// Get the string representation of the internal value. 
    /// </summary>
    /// <returns></returns>
    string GetStringValue();
    /// <summary>
    /// Gets the string representation of the default internal value.
    /// </summary>
    /// <returns></returns>
    string GetStringDefaultValue();
    /// <summary>
    /// Sets the internal value with the string equivalent.
    /// </summary>
    /// <param name="value"></param>
    void SetValueFromString(string value);
    /// <summary>
    /// Sets the internal value to default.
    /// </summary>
    void SetValueAsDefault();
    /// <summary>
    /// Validates a string-equivalent of an internal value.
    /// </summary>
    /// <param name="value">The string-value to test.</param>
    /// <returns>If the string is valid.</returns>
    bool ValidateString(string value);
}