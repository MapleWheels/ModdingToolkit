using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public static partial class NetworkingManager
{
    public static void SendMsg(IWriteMessage msg) => GameMain.LuaCs.Networking.Send(msg, DeliveryMethod.Reliable);

    private static bool ReadIdSingle(IReadMessage msg, out uint id, out string modName, out string name)
    {
        try
        {
            id = msg.ReadUInt32();
            modName = msg.ReadString();
            name = msg.ReadString();
            return true;
        }
        catch (Exception e)
        {
            LuaCsSetup.PrintCsError($"NetworkingManager::ReadIdSingle() | Unable to continue. Message read error. | Exception: {e.Message}");
            id = 0;
            modName = string.Empty;
            name = string.Empty;
            return false;
        }
    }
    
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
        ClearNetworkData();
        var outMsg = PrepareWriteMessageWithHeaders(NetworkEventId.ClientResponse_ResetStateSuccess);
        SendMsg(outMsg);
    }
    
    private static bool ReceiveIdSingle(IReadMessage msg)
    {
        if (ReadIdSingle(msg, out uint id, out string modName, out string name))
        {
            if (RegisterOrUpdateNetConfigId(modName, name, id))
            {
                SendRequestSyncVarSingle(id);
                return true;
            }
        }

        return false;
    }
    
    private static void ReceiveIdList(IReadMessage msg)
    {
        try
        {
            uint counter = msg.ReadUInt32();
            for (int index = 0; index < counter; index++)
            {
                if (ReadIdSingle(msg, out uint id, out string modName, out string name))
                    RegisterOrUpdateNetConfigId(modName, name, id);
                else
                {
                    LuaCsSetup.PrintCsError("NetworkingManager::ReceiveIdList() | Unable to continue. Read error.");
                    break;
                }
            }
            SendRequestSyncVarMulti();
        }
        catch (Exception e)
        {
            LuaCsSetup.PrintCsError($"NetworkingManager::ReceiveIdList() | Unable to continue. Message read error. | Exception: {e.Message}");
        }
    }

    private static bool ReceiveSyncVarSingle(IReadMessage msg)
    {
        try
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
                && UpdaterReadCallback[cfgDat.localId] is not null
                && NetConfigRegistry.ContainsKey(cfgDat.localId) 
                && NetConfigRegistry[cfgDat.localId] is
                {
                    IsNetworked: true, 
                    NetSync: NetworkSync.TwoWaySync or NetworkSync.ServerAuthority or NetworkSync.ClientPermissiveDesync
                })
            {
                UpdaterReadCallback[cfgDat.localId]!.Invoke(msg);
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            LuaCsSetup.PrintCsError($"NetworkingManager::ReceiveSyncVarSingle() | Read failure, cannot continue.");
            return false;
        }
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

    private static void SynchronizeNewVar(INetConfigBase cfg)
    {
        SynchronizeCleanLocalNetIndex(cfg);
        SendRequestIdSingle(cfg.ModName, cfg.Name);
    }

    private static void SendRequestSyncVarSingle(uint id)
    {
        var outMessage = PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestIdSingle);
        outMessage.WriteUInt32(id);
        SendMsg(outMessage);
    }

    private static void SendRequestSyncVarMulti() =>
        SendMsg(PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestSyncVarMulti));

    private static void SendRequestIdSingle(string modName, string name)
    {
        var message = PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestIdSingle);
        message.WriteString(modName);
        message.WriteString(name);
        SendMsg(message);
    }

    public static void SynchronizeAll()
    {
        ClearNetworkData();
        SendRequestIdList();
    }
    
    private static void SendRequestIdList() => SendMsg(PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestIdList));

    #endregion
}