namespace ModdingToolkit.Config;

/***
 * Keep this in Shared to allow common typedef between the Client and Server.
 */

/// <summary>
/// 
/// </summary>
/// <param name="Name"></param>
/// <param name="ModName"></param>
/// <param name="DisplayName"></param>
/// <param name="DisplayModName"></param>
/// <param name="DisplayCategory"></param>
/// <param name="Tooltip"></param>
/// <param name="ImageIcon"></param>
/// <param name="MenuCategory"></param>
public record DisplayData(string? Name, string? ModName, string? DisplayName, string? DisplayModName, 
    string? DisplayCategory, string? Tooltip, string? ImageIcon, Category MenuCategory);