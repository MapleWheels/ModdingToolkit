namespace ModdingToolkit.Config;

/// <summary>
/// A pointer container to reduce type assignment checking at runtime. Both vars point to the same object.
/// </summary>
/// <param name="Displayable">The IDisplayable interface.</param>
/// <param name="Control">The IConfigControl interface.</param>
public record DisplayableControl(IDisplayable Displayable, IConfigControl Control);