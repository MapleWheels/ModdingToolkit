namespace ModdingToolkit.Networking;

public enum NetworkEventId : byte
{
    Undef = 0,
    ResetState,
    SyncVarSingle,
    SyncVarMulti,
    Client_RequestIdSingle,
    Client_RequestIdList,
    Client_RequestSyncVarSingle,
    Client_RequestSyncVarMulti,
    ClientResponse_ResetStateSuccess
}