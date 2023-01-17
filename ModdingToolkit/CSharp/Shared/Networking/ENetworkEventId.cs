namespace ModdingToolkit.Networking;

public enum NetworkEventId : byte
{
    Undef = 0,
    SyncVarSingle,
    Client_ResetState,
    Client_RequestIdSingle,
    Client_RequestSyncVarSingle,
    Client_RequestAllIds
}