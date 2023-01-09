using Barotrauma.Networking;
using ModdingToolkit.Config;

namespace ModdingToolkit.Networking;

public static partial class NetworkingManager
{
    
    #region INTERNAL_API
    public static bool IsInitialized { get; private set; } = false;
    
#if SERVER
    public static readonly bool IsServer = true;
    public static readonly bool IsClient = false;
#else
    public static readonly bool IsServer = false;
    public static readonly bool IsClient = true;
#endif

    public static bool TryGetNetConfig(string modName, string name, out INetConfigBase? cfg)
    {
        cfg = null;
        if (!Indexer_LocalNetConfigIds.ContainsKey(modName) || !Indexer_LocalNetConfigIds[modName].ContainsKey(name))
            return false;
        Guid cfgId = Indexer_LocalNetConfigIds[modName][name];
        if (!NetConfigRegistry.ContainsKey(cfgId) || NetConfigRegistry[cfgId] is null)
            return false;
        cfg = NetConfigRegistry[cfgId];
        return true;
    }

    public static void Initialize(bool force = false)
    {
        if (IsInitialized)
        {
            if (force)
                Dispose();
            else 
                return;
        }
            
        GameMain.LuaCs.Networking.Receive(
            NetMsgId, 
            args => ReceiveMessage((IReadMessage)args[0], (Barotrauma.Networking.Client)args[1])
            );

        IsInitialized = true;
    }

    public static void Dispose()
    {
        if (!IsInitialized)
            return;
        
        GameMain.LuaCs.Networking.LuaCsNetReceives.Remove(NetMsgId);
        NetConfigRegistry.Clear();
        Indexer_NetConfigIds.Clear();
        foreach (var indexer in Indexer_LocalNetConfigIds)
            indexer.Value.Clear();
        Indexer_LocalNetConfigIds.Clear();
        Indexer_ReverseNetConfigId.Clear();
        UpdaterReadCallback.Clear();
        UpdaterWriteCallback.Clear();
#if SERVER
        // Follow up resync won't occur as we've unsubscribed from the inbound net event.
        SendMsg(WriteMessageHeaders(NetworkEventId.ResetState));            
#endif
        IsInitialized = false;
    }

    public static bool RegisterNetConfigInstance(INetConfigBase config)
    {
        Guid id = Guid.NewGuid();
        if (!RegisterLocalConfig(config, id))
        {
            LuaCsSetup.PrintCsError($"Net..Manager::RegisterNetConfigInstance() | A config with that Guid exists locally. Unable to continue. Modname={config.ModName}, Name={config.Name}");
            return false;
        }
        RegisterCallbacks(id,
            rMessage =>
            {
                config.SetStringValueFromNetwork(Utils.Networking.ReadNetValueFromType<string>(rMessage));
            },
            wMessage =>
            {
                Utils.Networking.WriteNetValueFromType(wMessage, config.GetStringNetworkValue());
            });
#if CLIENT
        SendRequestIdSingle(config.ModName, config.Name);
#else
        if (WriteIdSingleMsg(config.ModName, config.Name, out var msg))
        {
            SendMsg(msg!);
        }
#endif
        return true;
    }

    public static bool RegisterNetConfigInstance<T>(INetConfigEntry<T> config) where T : IConvertible
    {
        Guid id = Guid.NewGuid();
        if (!RegisterLocalConfig(config, id))
        {
            LuaCsSetup.PrintCsError($"Net..Manager::RegisterNetConfigInstance<T>() | A config with that Guid exists locally. Unable to continue. Modname={config.ModName}, Name={config.Name}");
            return false;
        }
        RegisterCallbacks(id,
            rMessage =>
            {
                config.SetNativeValueFromNetwork(Utils.Networking.ReadNetValueFromType<T>(rMessage));
            },
            wMessage =>
            {
                Utils.Networking.WriteNetValueFromType(wMessage, config.GetNetworkValue());
            });
        config.SubscribeToNetEvents(SendNetEvent);
        return true;
    }

    #endregion

    #region INTERNAL_OPS

    private static void SendNetEvent<T>(Guid id, T val) where T : IConvertible
    {
        if (!Indexer_ReverseNetConfigId.ContainsKey(id) || Indexer_ReverseNetConfigId[id] == 0)
        {
            LuaCsSetup.PrintCsError($"NetworkManager::SendNetEvent<{typeof(T)}>() | No network id exists for the Guid {id.ToString()}");
            return;
        }
        var msg = WriteMessageHeaders(NetworkEventId.SyncVarSingle);
        uint netId = Indexer_ReverseNetConfigId[id];
        msg.WriteUInt32(netId);
        Utils.Networking.WriteNetValueFromType(msg, val);
        SendMsg(msg);
    }

    private static IWriteMessage WriteMessageHeaders(NetworkEventId eventId)
    {
        var msg = GameMain.LuaCs.Networking.Start(NetworkingManager.NetMsgId);
        Utils.Networking.WriteNetValueFromType(msg, eventId);
        return msg;
    }

    private static bool RegisterLocalConfig(INetConfigBase cfg, Guid localId)
    {
        if (NetConfigRegistry.ContainsKey(localId) && NetConfigRegistry[localId] is not null)
            return false;
        NetConfigRegistry[localId] = cfg;
        if (!Indexer_LocalNetConfigIds.ContainsKey(cfg.ModName))
            Indexer_LocalNetConfigIds.Add(cfg.ModName, new Dictionary<string, Guid>());
        Indexer_LocalNetConfigIds[cfg.ModName][cfg.Name] = localId;
        return true;
    }

    private static bool RegisterOrUpdateNetConfigId(string modName, string name, uint id)
    {
        Guid guid = Guid.Empty; //empty
        if (Indexer_LocalNetConfigIds.ContainsKey(modName) && Indexer_LocalNetConfigIds[modName].ContainsKey(name))
            guid = Indexer_LocalNetConfigIds[modName][name];
        Indexer_NetConfigIds[id] = new NetSyncVarIndex(modName, name, guid);
        if (guid != Guid.Empty)
            Indexer_ReverseNetConfigId[guid] = id;
        return false;
    }

    private static void RemoveCallbacks(Guid id)
    {
        UpdaterReadCallback.Remove(id);
        UpdaterWriteCallback.Remove(id);
    }

    private static void RegisterCallbacks(Guid id, Action<IReadMessage> readHandle, Action<IWriteMessage> writeHandle)
    {
        UpdaterReadCallback[id] = readHandle;
        UpdaterWriteCallback[id] = writeHandle;
    }


    #endregion

    #region IVARDEF

    private static readonly Dictionary<Guid, INetConfigBase?> NetConfigRegistry = new();
    private static readonly Dictionary<Guid, System.Action<IWriteMessage>?> UpdaterWriteCallback = new();
    private static readonly Dictionary<Guid, System.Action<IReadMessage>?> UpdaterReadCallback = new();
    private static readonly Dictionary<uint, NetSyncVarIndex> Indexer_NetConfigIds = new();
    private static readonly Dictionary<Guid, uint> Indexer_ReverseNetConfigId = new();
    private static readonly Dictionary<string, Dictionary<string, Guid>> Indexer_LocalNetConfigIds = new();
    // ReSharper disable once StringLiteralTypo
    private static readonly string NetMsgId = "MTKNET";    //keep this short as it gets transmitted with every message.

    #endregion
    
    #region TYPEDEF

    private static class Counter
    {
        private static uint _counter;
        public static uint GetIncrement() => _counter++;
        public static uint Get() => _counter;
        public static void Reset() => _counter = 10;    //values below 10 are reserved.
    }

    public enum NetworkEventId
    {
        Undef = 0,
        ResetState,
        SyncVarSingle,
        SyncVarMulti,
        Client_RequestIdSingle,
        Client_RequestIdList,
        ClientResponse_ResetStateSuccess
    }

    public record NetSyncVarIndex(string ModName, string Name, Guid localId);

    #endregion
}