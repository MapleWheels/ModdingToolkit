namespace ModdingToolkit.Networking;

public interface INetConfigBase
{
    uint NetId { get; }
    string ModName { get; }
    string Name { get; }
    public Type NetSyncVarTypeDef { get; }
    void SetNetworkingId(uint id);
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