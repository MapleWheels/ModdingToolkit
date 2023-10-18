using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigList : IConfigList, INetConfigBase
{
    public bool IsNetworked => this.NetSync != NetworkSync.NoSync;
    public bool NetAuthorityValidate() => true;
    public void TriggerNetEvent()
    {
        if (this.NetSync is NetworkSync.TwoWaySync or NetworkSync.ServerAuthority or NetworkSync.ClientPermissiveDesync)
        {
            this._onNetworkEvent?.Invoke(this);
        } 
    }
}