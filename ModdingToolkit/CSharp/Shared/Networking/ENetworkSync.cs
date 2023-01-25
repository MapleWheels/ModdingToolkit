namespace ModdingToolkit.Networking;

public enum NetworkSync
{
    /// <summary>
    /// Does not synchronize between the Client and Server
    /// </summary>
    NoSync, 
    /// <summary>
    /// Only the server or clients with Server Authority can make changes.
    /// </summary>
    ServerAuthority, 
    /// <summary>
    /// The client is allowed to make changes BUT will not be synced to the server. Any changes made by the server are synced to clients.
    /// </summary>
    ClientPermissiveDesync, 
    /// <summary>
    /// Any changes made by either the client or the server will be synced.
    /// </summary>
    TwoWaySync
}