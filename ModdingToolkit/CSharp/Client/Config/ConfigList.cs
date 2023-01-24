using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigList : IConfigList, INetConfigBase, IDisplayable
{
    public bool IsNetworked => this.NetSync != NetworkSync.NoSync && GameMain.IsMultiplayer;
    public bool NetAuthorityValidate()
    {
        if (!IsNetworked)
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
        if (this.NetSync == NetworkSync.TwoWaySync)
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

    public virtual DisplayType GetDisplayType() => DisplayType.DropdownList;
}