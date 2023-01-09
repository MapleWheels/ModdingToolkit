using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public static partial class NetworkingManager
{
    public static void SendMsg(IWriteMessage msg) => GameMain.LuaCs.Networking.Send(msg, DeliveryMethod.Reliable);
    
    #region NET_INBOUND

    private static void ReceiveMessage(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        if (!IsInitialized)
            return;
        
        NetworkEventId evtId = Utils.Networking.ReadNetValueFromType<NetworkEventId>(msg);
        switch (evtId)
        {
            case NetworkEventId.Undef: return;
            case NetworkEventId.SyncVarSingle: ReceiveSyncVarSingle(msg);
                break;
            case NetworkEventId.SyncVarMulti: ReceiveSyncVarMulti(msg);
                break;
            case NetworkEventId.Client_RequestIdList: ReceiveIdList(msg);
                break;
            case NetworkEventId.Client_RequestIdSingle: ReceiveIdSingle(msg);
                break;
            case NetworkEventId.ResetState: ReceiveResetNetworkState();
                break;
        }
    }

    private static void ReceiveResetNetworkState()
    {
        Initialize(true);
    }
    
    private static bool ReceiveIdSingle(IReadMessage msg)
    {
        try
        {
            uint id = msg.ReadUInt32();
            string modName = msg.ReadString();
            string name = msg.ReadString();

            RegisterOrUpdateNetConfigId(modName, name, id);
            return true;
        }
        catch(Exception e)
        {
            LuaCsSetup.PrintCsError($"NetworkingManager::ReceiveIdSingle() | Unable to process incoming Id. Bad format. Exception: {e.Message}");
            return false;
        }
    }
    
    private static void ReceiveIdList(IReadMessage msg)
    {
        try
        {
            uint counter = msg.ReadUInt32();
            for (int index = 0; index < counter; index++)
            {
                if (!ReceiveIdSingle(msg))
                {
                    LuaCsSetup.PrintCsError("NetworkingManager::ReceiveIdList() | Unable to continue. Message read error.");
                    return;
                }
            }
        }
        catch
        {
            LuaCsSetup.PrintCsError("NetworkingManager::ReceiveIdList() | Unable to continue. Message read error.");
        }
    }

    private static bool ReceiveSyncVarSingle(IReadMessage msg)
    {
        uint id = msg.ReadUInt32();
        if (!Indexer_NetConfigIds.ContainsKey(id))
        {
            LuaCsSetup.PrintCsError($"NetworkingManager::ReceiveSyncVarSingle() | The id of {id} is not in the dictionary! Read failure.");
            return false;
        }
        var cfgDat = Indexer_NetConfigIds[id];
        if (cfgDat.localId != Guid.Empty 
            && UpdaterReadCallback.ContainsKey(cfgDat.localId) 
            && NetConfigRegistry.ContainsKey(cfgDat.localId) 
            && NetConfigRegistry[cfgDat.localId] is
            {
                IsNetworked: true, 
                NetSync: NetworkSync.TwoWaySync or NetworkSync.ServerAuthority or NetworkSync.ClientPermissiveDesync
            })
        {
            UpdaterReadCallback[cfgDat.localId]?.Invoke(msg);
            return true;
        }
        return false;
    }
    
    private static void ReceiveSyncVarMulti(IReadMessage msg)
    {
        try
        {
            uint counter = msg.ReadUInt32();
            for (int index = 0; index < counter; index++)
            {
                if (!ReceiveSyncVarSingle(msg))
                {
                    LuaCsSetup.PrintCsError($"NetworkingManager::ReceiveSyncVarMulti() | Read failure, cannot continue.");
                    return;
                }
            }
        }
        catch (Exception e)
        {
            LuaCsSetup.PrintCsError($"NetworkingManager::ReceiveSyncVarMulti() | Exception: {e.Message}.");
            return;
        }
    }

    #endregion

    #region NET_OUTBOUND

    private static void SendRequestIdSingle(string modName, string name)
    {
        var message = WriteMessageHeaders(NetworkEventId.Client_RequestIdSingle);
        message.WriteString(modName);
        message.WriteString(name);
        SendMsg(message);
    }

    private static void SendRequestIdList() => SendMsg(WriteMessageHeaders(NetworkEventId.Client_RequestIdList));

    #endregion
}