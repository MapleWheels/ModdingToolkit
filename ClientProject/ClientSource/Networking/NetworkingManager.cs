using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public static partial class NetworkingManager
{
    private static void SendMsg(IWriteMessage msg) => GameMain.LuaCs.Networking.Send(msg, DeliveryMethod.Reliable);

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
            Utils.Logging.PrintError($"NetworkingManager::ReadIdSingle() | Unable to continue. Message read error. | Exception: {e.Message}");
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
            case NetworkEventId.Client_RequestIdSingle: ReceiveIdSingle(msg);
                break;
            case NetworkEventId.Client_ResetState: ReceiveResetNetworkState();
                break;
        }
    }
    
    private static void SendNetSyncVarEvent(INetConfigBase cfg)
    {
        if (!IsInitialized)
            return;
        if (TryGetNetId(cfg.NetId, out uint id))
        {
            var msg = PrepareWriteMessageWithHeaders(NetworkEventId.SyncVarSingle);
            msg.WriteUInt32(id);
            INetWriteMessage n = new NetWriteMessage();
            n.SetMessage(msg);
            cfg.WriteNetworkValue(n);
            SendMsg(msg);
        }
    }

    private static void ReceiveResetNetworkState()
    {
        ClearNetworkData();
        SendRequestIdList();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="requestSyncVar"></param>
    /// <returns>Returns whether the message buffer was read successfully.</returns>
    private static bool ReceiveIdSingle(IReadMessage msg, bool requestSyncVar = true)
    {
        if (ReadIdSingle(msg, out uint id, out string modName, out string name))
        {
#if DEBUG
            Utils.Logging.PrintMessage($"ReceiveIdSingle: id={id}, modName={modName}, name={name}");
#endif
            if (RegisterOrUpdateNetConfigId(modName, name, id)
                && requestSyncVar
                && TryGetNetConfigInstance(modName, name, out var cfg)
                && cfg is
                {
                    IsNetworked: true,
                    NetSync: NetworkSync.ServerAuthority or NetworkSync.ClientPermissiveDesync or NetworkSync.TwoWaySync
                })
            {
#if DEBUG
                Utils.Logging.PrintMessage($"Receive ID Single: Sending SyncVar Request.");
#endif
                SendRequestSyncVarSingle(id);
            }
            return true;
        }

        return false;
    }

    private static bool ReceiveSyncVarSingle(IReadMessage msg)
    {
#if DEBUG      
        Utils.Logging.PrintMessage($"Client: ReceiveSyncVarSingle()");
#endif
        try
        {
            uint id = msg.ReadUInt32();
            if (TryGetNetConfigInstance(id, out var cfg)
                && cfg!.NetSync is not NetworkSync.NoSync)
            {
                INetReadMessage n = new NetReadMessage();
                n.SetMessage(msg);
                return cfg!.ReadNetworkValue(n);
            };
            return false;
        }
        catch (Exception e)
        {
            Utils.Logging.PrintError($"NetworkingManager::ReceiveSyncVarSingle() | Read failure, cannot continue. | Exception: {e.Message}");
            return false;
        }
    }

    #endregion

    #region NET_OUTBOUND

    private static void SynchronizeNewVar(INetConfigBase cfg)
    {
        SendRequestIdSingle(cfg.ModName, cfg.Name);
    }

    private static void SendRequestSyncVarSingle(uint id)
    {
        var outMessage = PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestIdSingle);
        outMessage.WriteUInt32(id);
        SendMsg(outMessage);
    }

    private static void SendRequestIdSingle(string modName, string name)
    {
        var message = PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestIdSingle);
        message.WriteString(modName);
        message.WriteString(name);
        SendMsg(message);
    }

    public static void SynchronizeAll()
    {
        if (!GameMain.IsMultiplayer)
            return;

        ClearNetworkData();
        SendRequestIdList();
    }
    
    private static void SendRequestIdList() => SendMsg(PrepareWriteMessageWithHeaders(NetworkEventId.Client_RequestAllIds));

    #endregion
}