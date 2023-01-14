using System.Diagnostics;
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
        if (!Indexer_LocalNetConfigGuids.ContainsKey(modName) || !Indexer_LocalNetConfigGuids[modName].ContainsKey(name))
            return false;
        Guid cfgId = Indexer_LocalNetConfigGuids[modName][name];
        if (!NetConfigRegistry.ContainsKey(cfgId) || NetConfigRegistry[cfgId] is null)
            return false;
        cfg = NetConfigRegistry[cfgId];
        return true;
    }

    public static void Initialize(bool force = false)
    {
#if CLIENT
        if (!GameMain.IsMultiplayer)
            return;        
#endif
        
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
        ClearNetworkData();
        UpdaterReadCallback.Clear();
        UpdaterWriteCallback.Clear();
        NetConfigRegistry.Clear();
        foreach (var indexer in Indexer_LocalNetConfigGuids)
            indexer.Value.Clear();
        Indexer_LocalNetConfigGuids.Clear();
        UpdaterReadCallback.Clear();
        UpdaterWriteCallback.Clear();
#if SERVER
        // Follow up resync won't occur as we've unsubscribed from the inbound net event.
        SendMsg(PrepareWriteMessageWithHeaders(NetworkEventId.ResetState));            
#endif
        IsInitialized = false;
    }

    public static bool RegisterNetConfigInstance(INetConfigBase config)
    {
        Guid id = Guid.NewGuid();
        if (!RegisterLocalConfig(config, id))
        {
            Utils.Logging.PrintError($"Net..Manager::RegisterNetConfigInstance() | A config with that Guid exists locally. Unable to continue. Modname={config.ModName}, Name={config.Name}");
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
        SynchronizeNewVar(config);
        return true;
    }

    public static bool RegisterNetConfigInstance<T>(INetConfigEntry<T> config) where T : IConvertible
    {
        Guid id = Guid.NewGuid();
        if (!RegisterLocalConfig(config, id))
        {
            Utils.Logging.PrintError($"Net..Manager::RegisterNetConfigInstance<T>() | A config with that Guid exists locally. Unable to continue. Modname={config.ModName}, Name={config.Name}");
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
        SynchronizeNewVar(config);
        return true;
    }

    /// <summary>
    /// For use by custom INetConfigBase implementations.
    /// Allows you to manually send a network event to update a sync var.
    /// </summary>
    /// <param name="netId"></param>
    /// <param name="val"></param>
    public static void SendStringNetVarUpdate(Guid netId, string val)
    {
        SendNetEvent(netId, val);
    }

    #endregion

    #region INTERNAL_OPS

    private static void ClearNetworkData()
    {
        Indexer_NetConfigIds.Clear();
        Indexer_ReverseNetConfigId.Clear();
    }
    
    private static void SendNetEvent<T>(Guid id, T val) where T : IConvertible
    {
        if (!Indexer_ReverseNetConfigId.ContainsKey(id) || Indexer_ReverseNetConfigId[id] == 0)
        {
            Utils.Logging.PrintError($"NetworkManager::SendNetEvent<{typeof(T)}>() | No network id exists for the Guid {id.ToString()}");
            return;
        }
        var msg = PrepareWriteMessageWithHeaders(NetworkEventId.SyncVarSingle);
        msg.WriteUInt32(Indexer_ReverseNetConfigId[id]);
        Utils.Networking.WriteNetValueFromType(msg, val);
        SendMsg(msg);
    }

    private static IWriteMessage PrepareWriteMessageWithHeaders(NetworkEventId eventId)
    {
        var msg = GameMain.LuaCs.Networking.Start(NetworkingManager.NetMsgId);
        Utils.Networking.WriteNetValueFromType(msg, eventId);
        return msg;
    }

    private static bool RegisterLocalConfig(INetConfigBase cfg, Guid localId)
    {
        if (NetConfigRegistry.ContainsKey(localId) && NetConfigRegistry[localId] is not null)
            return false;
        cfg.SetNetworkingId(localId);
        NetConfigRegistry[localId] = cfg;
        if (!Indexer_LocalNetConfigGuids.ContainsKey(cfg.ModName))
            Indexer_LocalNetConfigGuids.Add(cfg.ModName, new Dictionary<string, Guid>());
        Indexer_LocalNetConfigGuids[cfg.ModName][cfg.Name] = localId;
        return true;
    }

    private static void SynchronizeCleanLocalNetIndex(INetConfigBase cfg)
    {
        if (cfg.NetId == Guid.Empty)
            return;
        List<(uint, NetSyncVarIndex)> toAssign = new();
        foreach (KeyValuePair<uint,NetSyncVarIndex> pair in Indexer_NetConfigIds)
        {
            // setup network index
            if (pair.Value.ModName.Equals(cfg.ModName)
                && pair.Value.Name.Equals(cfg.Name))
            {
                toAssign.Add((pair.Key, new NetSyncVarIndex(cfg.ModName, cfg.Name, cfg.NetId)));
            }
        }

        if (!toAssign.Any())
            return;
        (uint, NetSyncVarIndex)? tupleA = null;
        // cleanup to ensure there are never multiple entries
        foreach ((uint, NetSyncVarIndex) tuple in toAssign)
        {
            Indexer_NetConfigIds.Remove(tuple.Item1);
            if (tuple.Item1 >= Counter.MinValue && tupleA is null)
            {
                tupleA = (tuple.Item1, tuple.Item2);
            }
        }
        
        if (tupleA is null)
            return;
        Indexer_NetConfigIds[tupleA.Value.Item1] = tupleA.Value.Item2;
        Indexer_ReverseNetConfigId[tupleA.Value.Item2.localId] = tupleA.Value.Item1;
    }

    private static bool RegisterOrUpdateNetConfigId(string modName, string name, uint id)
    {
        if (!Indexer_LocalNetConfigGuids.ContainsKey(modName) ||
            !Indexer_LocalNetConfigGuids[modName].ContainsKey(name))
            return false;
        Guid guidIndex = Indexer_LocalNetConfigGuids[modName][name];
        if (!NetConfigRegistry.ContainsKey(guidIndex) || NetConfigRegistry[guidIndex] is null)
            return false;
        var cfg = NetConfigRegistry[guidIndex];
        Debug.Assert(cfg is not null);
        if (cfg.NetId != guidIndex)
        {
            //sync callbacks and cfg net id
            Guid oldId = cfg.NetId;
            cfg.SetNetworkingId(guidIndex);
            if (oldId != Guid.Empty)
            {
                if (UpdaterReadCallback.ContainsKey(oldId))
                {
                    var readCallback = UpdaterReadCallback[oldId];
                    UpdaterReadCallback.Remove(oldId);
                    UpdaterReadCallback[guidIndex] = readCallback;
                }
                if (UpdaterWriteCallback.ContainsKey(oldId))
                {
                    var writeCallback = UpdaterWriteCallback[oldId];
                    UpdaterWriteCallback.Remove(oldId);
                    UpdaterWriteCallback[guidIndex] = writeCallback;
                }
            }
        }
        Indexer_ReverseNetConfigId[guidIndex] = id;
        Indexer_NetConfigIds[id] = new NetSyncVarIndex(modName, name, guidIndex);
        return true;
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
    private static readonly Dictionary<string, Dictionary<string, Guid>> Indexer_LocalNetConfigGuids = new();
    // ReSharper disable once StringLiteralTypo
    private static readonly string NetMsgId = "MTKNET";    //keep this short as it gets transmitted with every message.

    #endregion
    
    #region TYPEDEF

    private static class Counter
    {
        public static uint MinValue { get; private set; } = 10;
        private static uint _counter;
        public static uint GetIncrement() => _counter++;
        public static uint Get() => _counter;
        public static void Reset() => _counter = MinValue;    //values below 10 are reserved.
    }

    public record NetSyncVarIndex(string ModName, string Name, Guid localId);

    #endregion
}