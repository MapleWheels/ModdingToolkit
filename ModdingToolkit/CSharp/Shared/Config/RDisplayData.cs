namespace ModdingToolkit.Config;

/***
 * Keep this in Shared to allow common typedef and method sig between the Client and Server.
 */

/// <summary>
/// Contains the Display Data for use with Menus. Used by IDisplayable. 
/// </summary>
/// <param name="Name">Internal name of the instance.</param>
/// <param name="ModName">Internal mod name of the instance.</param>
/// <param name="DisplayName">The name to display in GUIs and Menus.</param>
/// <param name="DisplayModName">The mod name to display in GUIs and Menus.</param>
/// <param name="DisplayCategory">Category this instance falls under. Used by menus when filtering by category.</param>
/// <param name="Tooltip">The tooltip shown on hover.</param>
/// <param name="ImageIcon">The image icon to be used by GUIs when referring to this instance.</param>
/// <param name="MenuCategory">The category/section that this will appear in the SettingsMenu.</param>
public record DisplayData(string? Name = null, string? ModName = null, string? DisplayName = null, string? DisplayModName = null, 
    string? DisplayCategory = null, string? Tooltip = null, string? ImageIcon = null, Category MenuCategory = Category.Gameplay);