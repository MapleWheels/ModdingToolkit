namespace ModdingToolkit.Networking;

public interface INetConfigEntry<T> : INetConfigBase where T : IConvertible
{
    /// <summary>
    /// Set value without creating network events.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Success</returns>
    bool SetNativeValueFromNetwork(T value);
    /// <summary>
    /// Called when the Value is changed. Args: ModName, Name, Value.
    /// </summary>
    /// <param name="evtHandle"></param>
    void SubscribeToNetEvents(System.Action<Guid, T> evtHandle);
    void UnsubscribeFromNetEvents(System.Action<Guid, T> evtHandle);
    T GetNetworkValue();
}