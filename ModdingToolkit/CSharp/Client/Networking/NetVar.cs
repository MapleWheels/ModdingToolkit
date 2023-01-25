namespace ModdingToolkit.Networking;

public sealed partial class NetVar<T> : INetVar<T> where T : IConvertible
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
        if (NetSync is NetworkSync.TwoWaySync)
        {
            this._onNetworkEvent?.Invoke(this);
        } 
    }
}