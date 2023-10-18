namespace ModdingToolkit.Networking;

public sealed partial class NetVar<T> : INetVar<T> where T : IConvertible
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