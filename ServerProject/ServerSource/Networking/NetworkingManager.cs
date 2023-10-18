using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public static partial class NetworkingManager
{
    private static void SendMsg(IWriteMessage msg, NetworkConnection? conn = null) => GameMain.LuaCs.Networking.Send(msg, conn, DeliveryMethod.Reliable);

    #region NET_OUTBOUND_FROM_INTERNAL

    private static void SynchronizeNewVar(INetConfigBase cfg)
    {
        // was not in local index
        if (!Indexer_LocalToNetIdLookup.ContainsKey(cfg.NetId) || Indexer_LocalToNetIdLookup[cfg.NetId] < Counter.MinValue)
        {
            uint newId = Counter.GetIncrement();
#if DEBUG
            Utils.Logging.PrintMessage($"SynchronizeNewVar() | Registering Lookup for modName={cfg.ModName}, name={cfg.Name}, netId={newId}, guid={cfg.NetId}");
#endif
            if (!RegisterOrUpdateNetConfigId(cfg.ModName, cfg.Name, newId))
            {
                Utils.Logging.PrintError($"SynchronizeNewVar() | Reverse Lookup not registered for modName={cfg.ModName}, name={cfg.Name}, netId={newId}, guid={cfg.NetId}");
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
        Utils.Logging.PrintMessage($"SynchronizeAll().");
#else
        Utils.Logging.PrintMessage($"Client: SynchronizeAll().");
#endif
#endif
        var outmsg = PrepareWriteMessageWithHeaders(NetworkEventId.Client_ResetState);
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
            case NetworkEventId.Client_RequestAllIds: ReceiveRequestIdList(client);
                break;
            case NetworkEventId.Client_RequestIdSingle: ReceiveRequestIdSingle(msg, client);
                break;
            case NetworkEventId.Client_ResetState: ReceiveClientResponseResetState(client);
                break;
            case NetworkEventId.Client_RequestSyncVarSingle: ReceiveRequestForSyncVarSingle(msg, client);
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
            if (TryGetNetConfigInstance(id, out var cfg))
            {
                var outmsg = PrepareWriteMessageWithHeaders(NetworkEventId.SyncVarSingle);
                outmsg.WriteUInt32(id);
                INetWriteMessage n = new NetWriteMessage();
                n.SetMessage(outmsg);
                cfg!.WriteNetworkValue(n);
                SendMsg(outmsg, client.Connection);
            }
        }
        catch (Exception e)
        {
            Utils.Logging.PrintError($"NetworkingManager::ReceiveRequestForSyncVarSingle() | Exception: {e.Message}"); 
        }
    }

    private static void ReceiveRequestIdList(Barotrauma.Networking.Client? client)
    {
        if (!IsInitialized)
            return;
        if (client is null)
            return;
        if (!Indexer_NetToLocalIdLookup.Any())
            return;
        ImmutableDictionary<uint, NetSyncVarIndex> toSync = Indexer_NetToLocalIdLookup
            .Where(kvp =>
                TryGetNetConfigInstance(kvp.Value.ModName, kvp.Value.Name, out var cfg) 
                && cfg is { IsNetworked: true })
            .ToImmutableDictionary();
    #if DEBUG
        Utils.Logging.PrintMessage($"NM::WriteIdListMsg() | SyncVar Count: {toSync.Count}");
    #endif
        foreach (var index in toSync)
        {
            var outmsg = PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestIdSingle);
            outmsg.WriteIdNameInfo(index.Key, index.Value.ModName, index.Value.Name);
            SendMsg(outmsg, client.Connection);
    #if DEBUG
            Utils.Logging.PrintMessage($"NM::WriteIdListMsg() | Writing SyncVar: id={index.Key}, modName={index.Value.ModName}, name={index.Value.Name}");
    #endif
        }
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
    
    private static void SendNetSyncVarEvent(INetConfigBase cfg, Barotrauma.Networking.Client? client = null)
    {
        if (!IsInitialized)
            return;
        if (!cfg.NetAuthorityValidate() || !cfg.IsNetworked)
            return;
        if (TryGetNetId(cfg.NetId, out uint id))
        {
            var msg = PrepareWriteMessageWithHeaders(NetworkEventId.SyncVarSingle);
            msg.WriteUInt32(id);
            INetWriteMessage n = new NetWriteMessage();
            n.SetMessage(msg);
            if (cfg.WriteNetworkValue(n))
            {
                if (client is null)
                    SendMsg(msg, null);
                else
                    SendMsg(msg, client.Connection);
            }
        }
    }

    private static bool ReceiveSyncVarSingle(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        if (client is null)
            return false;
#if DEBUG      
        Utils.Logging.PrintMessage($"ReceiveSyncVarSingle()");
#endif
        try
        {
            uint id = msg.ReadUInt32();
            if (TryGetNetConfigInstance(id, out var cfg))
            {
                // If bad read, retransmit the known good value to all clients.
                INetReadMessage nr = new NetReadMessage();
                nr.SetMessage(msg);
                if (cfg!.NetSync is NetworkSync.NoSync 
                    || (cfg!.NetSync is NetworkSync.ServerAuthority && !client.HasPermission(ClientPermissions.ManageSettings))
                    || !cfg!.ReadNetworkValue(nr))
                {
                    SendNetSyncVarEvent(cfg);
                    return false;
                }
                
                if (client is null)
                {
                    SendNetSyncVarEvent(cfg);
                    return true;
                }
                
                foreach (Barotrauma.Networking.Client cl in GameMain.Server.ConnectedClients.Where(c => c != client))
                {
                    SendNetSyncVarEvent(cfg, cl);
                }
            }
            return false;
        }
        catch (Exception e)
        {
            Utils.Logging.PrintError($"NetworkingManager::ReceiveSyncVarSingle() | Read failure, cannot continue. | Exception: {e.Message}");
            return false;
        }
    }

    #endregion
    
    #region UTIL

    private static bool WriteIdSingleMsg(string modName, string name, out IWriteMessage? message)
    {
        message = null;
        if (TryGetNetConfigInstance(modName, name, out var cfg)
            && TryGetNetId(cfg!.NetId, out uint netId))
        {
            message = PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestIdSingle);
            message.WriteIdNameInfo(netId, cfg.ModName, cfg.Name);
            return true;
        }

        return false;
    }

    private static void WriteIdNameInfo(this IWriteMessage msg, uint netId, string modName, string name)
    {
        msg.WriteUInt32(netId);
        msg.WriteString(modName);
        msg.WriteString(name);
    }

    #endregion
}