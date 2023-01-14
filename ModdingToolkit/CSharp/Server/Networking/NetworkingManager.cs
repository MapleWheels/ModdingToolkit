using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public static partial class NetworkingManager
{
    private static void SendMsg(IWriteMessage msg, NetworkConnection? conn = null) => GameMain.LuaCs.Networking.Send(msg, conn, DeliveryMethod.Reliable);

    #region NET_OUTBOUND_FROM_INTERNAL

    private static void SynchronizeNewVar(INetConfigBase cfg)
    {
        SynchronizeCleanLocalNetIndex(cfg);
        // was not in local index
        if (!Indexer_ReverseNetConfigId.ContainsKey(cfg.NetId))
        {
            uint newId = Counter.GetIncrement();
#if DEBUG
            Utils.Logging.PrintMessage($"Server: SynchronizeNewVar() | Registering Lookup for modName={cfg.ModName}, name={cfg.Name}, netId={newId}, guid={cfg.NetId}");
#endif
            if (!RegisterOrUpdateNetConfigId(cfg.ModName, cfg.Name, newId))
            {
                Utils.Logging.PrintError($"Server: SynchronizeNewVar() | Reverse Lookup not registered for modName={cfg.ModName}, name={cfg.Name}, netId={newId}, guid={cfg.NetId}");
            }
        }

        if (WriteIdSingleMsg(cfg.ModName, cfg.Name, out var msg))
        {
            SendMsg(msg!);
        }
    }

    public static void SynchronizeAll()
    {
#if DEBUG
#if SERVER
        Utils.Logging.PrintMessage($"Server: SynchronizeAll().");
#else
        Utils.Logging.PrintMessage($"Client: SynchronizeAll().");
#endif
#endif
        var outmsg = PrepareWriteMessageWithHeaders(NetworkEventId.ResetState);
        SendMsg(outmsg);
    }

    #endregion
    
    #region NET_INBOUND_TRIGGER

    private static void ReceiveMessage(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        if (!IsInitialized)
            return;
        
        NetworkEventId evtId = Utils.Networking.ReadNetValueFromType<NetworkEventId>(msg);
        switch (evtId)
        {
            case NetworkEventId.Undef: return;
            case NetworkEventId.SyncVarSingle: ReceiveSyncVarSingle(msg, client);
                break;
            case NetworkEventId.SyncVarMulti: ReceiveSyncVarMulti(msg, client);
                break;
            case NetworkEventId.Client_RequestIdList: ReceiveRequestIdList(client);
                break;
            case NetworkEventId.Client_RequestIdSingle: ReceiveRequestIdSingle(msg, client);
                break;
            case NetworkEventId.ClientResponse_ResetStateSuccess: ReceiveClientResponseResetState(client);
                break;
            case NetworkEventId.Client_RequestSyncVarSingle: ReceiveRequestForSyncVarSingle(msg, client);
                break;
            case NetworkEventId.Client_RequestSyncVarMulti: ReceiveRequestForSyncVarMulti(client);
                break;
            default:
                return;
        }
    }

    /// <summary>
    /// Follow up to state reset. Sends all ids to the client.
    /// </summary>
    /// <param name="client"></param>
    private static void ReceiveClientResponseResetState(Barotrauma.Networking.Client? client) => ReceiveRequestIdList(client);

    private static void ReceiveRequestForSyncVarSingle(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        if (client is null)
            return;
        try
        {
            uint id = msg.ReadUInt32();
            if (Indexer_NetConfigIds.ContainsKey(id))
            {
                var outmsg = PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestSyncVarSingle);
                if (WriteSyncVarIdValue(id, outmsg))
                    SendMsg(outmsg, client.Connection);
            }
        }
        catch (Exception e)
        {
            Utils.Logging.PrintError($"NetworkingManager::ReceiveRequestForSyncVarSingle() | Exception: {e.Message}"); 
        }
    }

    private static void ReceiveRequestForSyncVarMulti(Barotrauma.Networking.Client? client)
    {
        if (client is null)
            return;
        var outmsg = PrepareWriteMessageWithHeaders(NetworkEventId.SyncVarMulti);
        List<(uint, Action<IWriteMessage>)> idList = new();
        foreach (KeyValuePair<uint,NetSyncVarIndex> index in Indexer_NetConfigIds)
        {
            if (index.Value.localId != Guid.Empty
                && UpdaterWriteCallback.ContainsKey(index.Value.localId)
                && UpdaterWriteCallback[index.Value.localId] is not null)
            {
                idList.Add((index.Key, UpdaterWriteCallback[index.Value.localId]!));
            }
        }

        if (!idList.Any())
            return;
        outmsg.WriteUInt32(Convert.ToUInt32(idList.Count));
        foreach ((uint, Action<IWriteMessage>) tuple in idList)
        {
            outmsg.WriteUInt32(tuple.Item1);
            tuple.Item2.Invoke(outmsg);
        }
        SendMsg(outmsg, client.Connection);
    }
    
    private static void ReceiveSyncVarMulti(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        try
        {
            uint count = msg.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                if (!ReceiveSyncVarSingle(msg, client))
                {
                    Utils.Logging.PrintError(
                        $"NetworkingManager::ReceiveSyncVarMulti() | Was unable to parse data."); 
                    //we can't continue to read since we can't remove the unknown-length value bits from the message.
                    break;
                }            
            }
        }
        catch (Exception e)
        {
            Utils.Logging.PrintError($"NetworkingManager::ReceiveSyncVarMulti() | {e.Message}");
        }
    }

    private static void ReceiveRequestIdList(Barotrauma.Networking.Client? client)
    {
        if (!IsInitialized)
            return;
        if (client is null)
            return;
        var outmsg = WriteIdListMsg();
        SendMsg(outmsg, client.Connection);
    }

    private static void ReceiveRequestIdSingle(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        if (client is null)
            return;
        try
        {
            string modName = msg.ReadString();
            string name = msg.ReadString(); 
            if (WriteIdSingleMsg(modName, name, out var outmsg))
            {
                SendMsg(outmsg!, client.Connection);
            }
        }
        catch (Exception e)
        {
            Utils.Logging.PrintError($"NetworkManager::ReceiveRequestIdSingle() | Cannot read config name or mod name. | Exception: {e.Message}");
        }
    }

    private static bool ReceiveSyncVarSingle(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        uint id = msg.ReadUInt32();
        if (!Indexer_NetConfigIds.ContainsKey(id))
        {
            Utils.Logging.PrintError($"NetworkingManager::ReceiveSyncVarSingle() | The id of {id} is not in the dictionary! Read failure.");
            return false;
        }
        var cfgDat = Indexer_NetConfigIds[id];
        if (cfgDat.localId != Guid.Empty 
            && UpdaterReadCallback.ContainsKey(cfgDat.localId) 
            && NetConfigRegistry.ContainsKey(cfgDat.localId) 
            && NetConfigRegistry[cfgDat.localId] is { IsNetworked: true, NetSync: NetworkSync.TwoWaySync })
        {
            UpdaterReadCallback[cfgDat.localId]?.Invoke(msg);
            //send updates to other clients
            if (client is not null 
                && UpdaterWriteCallback.ContainsKey(cfgDat.localId)
                && UpdaterWriteCallback[cfgDat.localId] is {} callback)
            {
                var outmsg = PrepareWriteMessageWithHeaders(NetworkEventId.SyncVarSingle);
                callback.Invoke(outmsg);
                foreach (NetworkConnection connection in Barotrauma.Networking.Client.ClientList.Where(c => c != client).Select(c => c.Connection))
                {
                    SendMsg(outmsg, connection);
                }
            }
            
            return true;
        }
        return false;
    }

    #endregion
    
    #region UTIL

    private static IWriteMessage WriteIdListMsg()
    {
        var outmsg = PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestIdList);
        ImmutableDictionary<uint, NetSyncVarIndex> toSync = Indexer_NetConfigIds.Where((kvp) =>
        {
            if (TryGetNetConfig(kvp.Value.ModName, kvp.Value.Name, out var cfg) 
                && cfg is
                {
                    IsNetworked: true,
                    NetSync: NetworkSync.ServerAuthority or NetworkSync.ClientPermissiveDesync or NetworkSync.TwoWaySync
                })
            {
                return true;
            }
            return false;
        }).ToImmutableDictionary();
        outmsg.WriteUInt32(Convert.ToUInt32(toSync.Count));
        foreach (var index in toSync)
        {
            outmsg.WriteUInt32(index.Key);
            outmsg.WriteString(index.Value.ModName);
            outmsg.WriteString(index.Value.Name);
        }
        return outmsg;
    }

    private static bool WriteIdSingleMsg(string modName, string name, out IWriteMessage? message)
    {
        message = null;
        if (TryGetNetConfig(modName, name, out var cfg))
        {
            var guid = cfg!.NetId;
            if (Indexer_ReverseNetConfigId.ContainsKey(guid))
            {
                message = PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestIdSingle);
                message.WriteUInt32(Indexer_ReverseNetConfigId[guid]);
                message.WriteString(modName);
                message.WriteString(name);
                return true;
            }
        }

        return false;
    }

    private static bool WriteSyncVarIdValue(uint id, IWriteMessage msg)
    {
        if (Indexer_NetConfigIds.ContainsKey(id)
            && Indexer_NetConfigIds[id].localId != Guid.Empty
            && UpdaterWriteCallback.ContainsKey(Indexer_NetConfigIds[id].localId)
            && UpdaterWriteCallback[Indexer_NetConfigIds[id].localId] is not null)
        {
            msg.WriteUInt32(id);
            UpdaterWriteCallback[Indexer_NetConfigIds[id].localId]!.Invoke(msg);
            return true;
        }
        return false;
    }

    #endregion
}