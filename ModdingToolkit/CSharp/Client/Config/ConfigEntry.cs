using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigEntry<T> : IConfigEntry<T>, INetConfigBase where T : IConvertible
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
        if (this.NetSync is NetworkSync.TwoWaySync)
        {
            this._onNetworkEvent?.Invoke(this);
        } 
    }
}