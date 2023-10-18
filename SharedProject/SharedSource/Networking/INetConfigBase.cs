using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public interface INetConfigBase
{
    Guid NetId { get; }
    string ModName { get; }
    string Name { get; }
    public Type NetSyncVarTypeDef { get; }
    void SetNetworkingId(Guid id);
    public NetworkSync NetSync { get; }
    bool IsNetworked { get; }
    bool NetAuthorityValidate();
    /// <summary>
    /// Called when the Value is changed. Args: ModName, Name, Value.
    /// </summary>
    /// <param name="evtHandle"></param>
    public bool WriteNetworkValue(INetWriteMessage msg);
    public bool ReadNetworkValue(INetReadMessage msg);
    public void SubscribeToNetEvents(System.Action<INetConfigBase> evtHandle);
    public void UnsubscribeFromNetEvents(System.Action<INetConfigBase> evtHandle);
    void TriggerNetEvent();
    void InitializeNetworking(Guid netId, NetworkSync networkSync);
}