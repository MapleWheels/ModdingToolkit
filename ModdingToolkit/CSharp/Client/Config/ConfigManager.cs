using Barotrauma.Networking;
using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public static partial class ConfigManager
{
    #region PUBLIC_API

    public static IConfigControl AddConfigKeyOrMouseBind(
        string name,
        string modName,
        KeyOrMouse defaultValue,
        Action? onValueChanged = null,
        string? filePathOverride = null
    )
    {
        return CreateIConfigControl(name, modName, defaultValue, onValueChanged, filePathOverride);
    }

    public static IEnumerable<DisplayableControl> GetControlConfigs() => DisplayableControls.ToImmutableList();

    #endregion

    #region INTERNAL_OPS

    private static IConfigControl CreateIConfigControl(
        string name,
        string modName,
        KeyOrMouse defaultValue,
        Action? onValueChanged,
        string? filePathOverride = null,
        DisplayData? data = null)
    {
        ConfigControl cc = new();
        cc.Initialize(name, modName, null, defaultValue, onValueChanged);
        InitializeConfigBase(cc, data, filePathOverride);
        return cc;
    }

    private static void RegisterDisplayable(IDisplayable displayable, DisplayData data)
    {
        if (data.MenuCategory is Category.Ignore)
            return;
        displayable.InitializeDisplay(data.Name, data.ModName, data.DisplayName, data.DisplayModName, 
            data.DisplayCategory, data.Tooltip, data.ImageIcon, data.MenuCategory);
        Displayables.Add(displayable);
        if (displayable is IConfigControl icc)
            DisplayableControls.Add(new DisplayableControl(displayable, icc));
    }

    private static void RemoveDisplayable(IDisplayable displayable)
    {
        Displayables.RemoveAll(d => d == displayable);
        DisplayableControls.RemoveAll(dc => dc.Displayable == displayable);
    }

    private static void DisposeClient()
    {
        Displayables.Clear();
        DisplayableControls.Clear();
    }

    #endregion

    #region INTERNAL VARS

    private static readonly List<IDisplayable> Displayables = new();
    private static readonly List<DisplayableControl> DisplayableControls = new();

    /// <summary>
    /// A pointer container to reduce type assignment checking at runtime. Both vars point to the same object.
    /// </summary>
    /// <param name="Displayable">The IDisplayable interface.</param>
    /// <param name="Control">The IConfigControl interface.</param>
    public record DisplayableControl(IDisplayable Displayable, IConfigControl Control);

    #endregion
}