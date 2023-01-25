using Barotrauma.Networking;
using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigEntry<T> : IConfigEntry<T>, INetConfigBase, IDisplayable where T : IConvertible
{
    public bool IsNetworked => this.NetSync != NetworkSync.NoSync && GameMain.IsMultiplayer;
    public bool NetAuthorityValidate()
    {
        if (!IsNetworked)
            return true;
        if (NetSync is NetworkSync.ServerAuthority && GameMain.Client.HasPermission(ClientPermissions.ManageSettings))
            return true;
        
        return this.NetSync switch
        {
            NetworkSync.NoSync => true,
            NetworkSync.ClientPermissiveDesync => true,
            NetworkSync.TwoWaySync => true,
            _ => false
        };
    }
    
    public void TriggerNetEvent()
    {
        if (NetSync is NetworkSync.TwoWaySync
            || (NetSync is NetworkSync.ServerAuthority && GameMain.Client.HasPermission(ClientPermissions.ManageSettings)))
        {
            this._onNetworkEvent?.Invoke(this);
        } 
    }

    public string DisplayName { get; private set; }
    public string DisplayModName { get; private set; }
    public string DisplayCategory { get; private set; }
    public string Tooltip { get; private set; }
    public string ImageIcon { get; private set; }
    public Category MenuCategory { get; private set; }

    public void InitializeDisplay(string? name = "", string? modName = "", string? displayName = "", string? displayModName = "",
        string? displayCategory = "", string? tooltip = "", string? imageIcon = "", Category menuCategory = Category.Gameplay)
    {
        if (!displayName.IsNullOrWhiteSpace())
            this.DisplayName = displayName;
        if (!displayModName.IsNullOrWhiteSpace())
            this.DisplayModName = displayModName;
        if (!displayCategory.IsNullOrWhiteSpace())
            this.DisplayCategory = displayCategory;
        if (!tooltip.IsNullOrWhiteSpace())
            this.Tooltip = tooltip;
        if (!imageIcon.IsNullOrWhiteSpace())
            this.ImageIcon = imageIcon;
        this.MenuCategory = menuCategory;
        if (this.MenuCategory is Category.Controls)
            this.MenuCategory = Category.Gameplay;
    }
    
    public virtual DisplayType GetDisplayType() =>
        typeof(T) switch
        {
            { IsEnum: true } => DisplayType.DropdownEnum,
            { Name: nameof(Boolean) } => DisplayType.Tickbox,
            { IsPrimitive: true } => DisplayType.Number,
            _ => DisplayType.Standard
        };
}