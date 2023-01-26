using Barotrauma.Networking;
using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public static partial class ConfigManager
{
    #region PUBLIC_API

    /// <summary>
    /// Creates a Config var for binding key or mouse buttons to.
    /// </summary>
    /// <param name="name">Name of your config variable</param>
    /// <param name="modName">The name of your Mod. Acts a collection everything with the same ModName.</param>
    /// <param name="defaultValue">The default key or mouse binding.</param>
    /// <param name="onValueChanged">Called whenever the value has been successfully changed.</param>
    /// <param name="filePathOverride">Use if you want to load this variable from another config file on disk. Takes an absolute path.</param>
    /// <param name="displayData">Contains data used for Settings Menu entries or other GUI functions. Not used on server.</param>
    /// <returns></returns>
    public static IConfigControl AddConfigKeyOrMouseBind(
        string name,
        string modName,
        KeyOrMouse defaultValue,
        Action? onValueChanged = null,
        string? filePathOverride = null,
        DisplayData? displayData = null
    )
    {
        return CreateIConfigControl(name, modName, defaultValue, onValueChanged, filePathOverride, displayData ?? new DisplayData());
    }

    public static IEnumerable<IDisplayable> GetDisplayableConfigs() => Displayables.ToImmutableList();
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
        #warning TODO: Implement display data verification
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (displayable is null)
            return;
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

    #endregion
}