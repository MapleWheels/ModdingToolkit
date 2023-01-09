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
    /// Set value without creating network events.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Success</returns>
    bool SetStringValueFromNetwork(string value);

    string GetStringNetworkValue();
}