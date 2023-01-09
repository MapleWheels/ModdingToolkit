using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public static partial class NetworkingManager
{
    public static void SendMsg(IWriteMessage msg, NetworkConnection? conn = null) => GameMain.LuaCs.Networking.Send(msg, conn, DeliveryMethod.Reliable);
    
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
        }
    }

    /// <summary>
    /// Follow up to state reset. Sends all ids to the client.
    /// </summary>
    /// <param name="client"></param>
    private static void ReceiveClientResponseResetState(Barotrauma.Networking.Client? client) => ReceiveRequestIdList(client);

    private static void ReceiveSyncVarMulti(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        try
        {
            uint count = msg.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                if (!ReceiveSyncVarSingle(msg, client))
                {
                    LuaCsSetup.PrintCsError(
                        $"NetworkingManager::ReceiveSyncVarMulti() | Was unable to parse data."); 
                    //we can't continue to read since we can't remove the unknown-length value bits from the message.
                    break;
                }            
            }
        }
        catch (Exception e)
        {
            LuaCsSetup.PrintCsError($"NetworkingManager::ReceiveSyncVarMulti() | {e.Message}");
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

    private static IWriteMessage WriteIdListMsg()
    {
        var outmsg = WriteMessageHeaders(NetworkEventId.Client_RequestIdList);
        outmsg.WriteUInt32(Convert.ToUInt32(Indexer_NetConfigIds.Count));
        foreach (var index in Indexer_NetConfigIds)
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
                message = WriteMessageHeaders(NetworkEventId.Client_RequestIdSingle);
                message.WriteUInt32(Indexer_ReverseNetConfigId[guid]);
                message.WriteString(modName);
                message.WriteString(name);
                return true;
            }
        }

        return false;
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
            LuaCsSetup.PrintCsError($"NetworkManager::ReceiveRequestIdSingle() | Cannot read config name or mod name. | Exception: {e.Message}");
        }
    }

    private static bool ReceiveSyncVarSingle(IReadMessage msg, Barotrauma.Networking.Client? client)
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
            && NetConfigRegistry[cfgDat.localId] is { IsNetworked: true, NetSync: NetworkSync.TwoWaySync })
        {
            UpdaterReadCallback[cfgDat.localId]?.Invoke(msg);
            //send updates to other clients
            if (client is not null 
                && UpdaterWriteCallback.ContainsKey(cfgDat.localId)
                && UpdaterWriteCallback[cfgDat.localId] is {} callback)
            {
                var outmsg = WriteMessageHeaders(NetworkEventId.SyncVarSingle);
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
}