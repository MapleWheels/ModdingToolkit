using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public sealed partial class NetVar<T> : INetVar<T> where T : IConvertible
{
    private System.Action<INetConfigBase>? _onNetworkEvent;
    
    public Guid NetId { get; private set; }
    public string ModName { get; private set; }
    public string Name { get; private set; }
    
    public Type NetSyncVarTypeDef => typeof(T);
    
    public void SetNetworkingId(Guid id) => this.NetId = id;

    public NetworkSync NetSync { get; private set; }

    private T _value;

    public T Value
    {
        get => this._value;
        set
        {
            if (NetAuthorityValidate())
            {
                this._value = value;
                this.TriggerNetEvent();
            }
        }
    }
    
    bool INetConfigBase.WriteNetworkValue(INetWriteMessage msg)
    {
        Utils.Networking.WriteNetValueFromType(msg, this.Value);
        return true;
    }

    bool INetConfigBase.ReadNetworkValue(INetReadMessage msg)
    {
        try
        {
            this._value = Utils.Networking.ReadNetValueFromType<T>(msg);
            return true;
        }
        catch (Exception e)
        {
            Utils.Logging.PrintError($"NetVar::ReadNetworkValue() | Bad read. ModName={this.ModName}, Name={this.Name}");
            return false;
        }
    }

    void INetConfigBase.SubscribeToNetEvents(Action<INetConfigBase> evtHandle) => this._onNetworkEvent += evtHandle;

    void INetConfigBase.UnsubscribeFromNetEvents(Action<INetConfigBase> evtHandle) => this._onNetworkEvent -= evtHandle;

    public void InitializeNetworking(Guid netId, NetworkSync networkSync)
    {
        this.NetSync = networkSync;
        this.NetId = netId;
    }
    
    public void Initialize(string modName, string name, T value)
    {
        this.ModName = modName;
        this.Name = name;
        this._value = value;
    }
}