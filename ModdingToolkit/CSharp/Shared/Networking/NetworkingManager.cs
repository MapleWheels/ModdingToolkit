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

    public static bool TryGetNetConfigInstance(uint netId, out INetConfigBase? cfg)
    {
        cfg = null;
        if (netId < Counter.MinValue || !Indexer_NetToLocalIdLookup.ContainsKey(netId))
            return false;
        var index = Indexer_NetToLocalIdLookup[netId];
        return TryGetNetConfigInstance(index.ModName, index.Name, out cfg);
    }
    
    public static bool TryGetNetConfigInstance(string modName, string name, out INetConfigBase? cfg)
    {
        cfg = null;
        if (modName.IsNullOrWhiteSpace())
        {
#if DEBUG
            Utils.Logging.PrintError($"NetworkingManager::TryGetNetConfigInstance() | Modname is null or whitespace.");
#endif
            return false;
        }
        if (name.IsNullOrWhiteSpace())
        {
#if DEBUG
            Utils.Logging.PrintError($"NetworkingManager::TryGetNetConfigInstance() | Name is null or whitespace.");
#endif
            return false;
        }
        
        cfg = null;
        if (!Indexer_LocalGuidsLookup.ContainsKey(modName) || !Indexer_LocalGuidsLookup[modName].ContainsKey(name))
            return false;
        Guid cfgId = Indexer_LocalGuidsLookup[modName][name];
        if (!NetConfigRegistry.ContainsKey(cfgId) || NetConfigRegistry[cfgId] is null)
            return false;
        cfg = NetConfigRegistry[cfgId];
        return true;
    }

    public static bool TryGetNetId(Guid localid, out uint netId)
    {
        netId = 0;
        if (!Indexer_LocalToNetIdLookup.ContainsKey(localid))
            return false;
        if (Indexer_LocalToNetIdLookup[localid] < Counter.MinValue)
        {
            Indexer_LocalToNetIdLookup.Remove(localid);
            return false;
        }

        netId = Indexer_LocalToNetIdLookup[localid];
        return true;
    }


    public static void Initialize(bool force = false)
    {
#if CLIENT
        if (!GameMain.IsMultiplayer)
            return;        
#endif
        Counter.Reset();
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
        NetConfigRegistry.Clear();
        foreach (var indexer in Indexer_LocalGuidsLookup)
            indexer.Value.Clear();
        Indexer_LocalGuidsLookup.Clear();
#if SERVER
        // Follow up resync won't occur as we've unsubscribed from the inbound net event.
        SendMsg(PrepareWriteMessageWithHeaders(NetworkEventId.Client_ResetState));            
#endif
        IsInitialized = false;
    }

    public static bool RegisterNetConfigInstance(INetConfigBase config, NetworkSync syncMode)
    {
        Guid id = Guid.NewGuid();
        if (!RegisterLocalConfig(config, id, syncMode))
        {
            Utils.Logging.PrintError($"NetworkingManager::RegisterNetConfigInstance() | A config with that Guid exists locally. Unable to continue. Modname={config.ModName}, Name={config.Name}");
            return false;
        }
        config.SubscribeToNetEvents(SendNetSyncVarEventRedirect);
        SynchronizeNewVar(config);
        return true;
    }

    private static void SendNetSyncVarEventRedirect(INetConfigBase cfg)
    {
        // wrapper for specific implementation
        SendNetSyncVarEvent(cfg);
    }

    #endregion

    #region INTERNAL_OPS

    private static void ClearNetworkData()
    {
        Indexer_NetToLocalIdLookup.Clear();
        Indexer_LocalToNetIdLookup.Clear();
    }
    
    private static IWriteMessage PrepareWriteMessageWithHeaders(NetworkEventId eventId)
    {
        var msg = GameMain.LuaCs.Networking.Start(NetworkingManager.NetMsgId);
        Utils.Networking.WriteNetValueFromType(msg, eventId);
        return msg;
    }

    private static bool RegisterLocalConfig(INetConfigBase cfg, Guid localId, NetworkSync syncMode)
    {
        if (NetConfigRegistry.ContainsKey(localId) && NetConfigRegistry[localId] is not null)
            return false;
        cfg.InitializeNetworking(localId, syncMode);
        NetConfigRegistry[localId] = cfg;
        if (!Indexer_LocalGuidsLookup.ContainsKey(cfg.ModName))
            Indexer_LocalGuidsLookup.Add(cfg.ModName, new Dictionary<string, Guid>());
        Indexer_LocalGuidsLookup[cfg.ModName][cfg.Name] = localId;
        return true;
    }

    private static bool RegisterOrUpdateNetConfigId(string modName, string name, uint id)
    {
        if (TryGetNetConfigInstance(modName, name, out var cfg))
        {
            Indexer_LocalToNetIdLookup[cfg!.NetId] = id;
            Indexer_NetToLocalIdLookup[id] = new NetSyncVarIndex(cfg.ModName, cfg.Name);
            return true;
        }

        return false;
    }


    #endregion

    #region IVARDEF

    // local
    private static readonly Dictionary<string, Dictionary<string, Guid>> Indexer_LocalGuidsLookup = new();
    private static readonly Dictionary<Guid, INetConfigBase> NetConfigRegistry = new(); // net
    private static readonly Dictionary<Guid, uint> Indexer_LocalToNetIdLookup = new();
    private static readonly Dictionary<uint, NetSyncVarIndex> Indexer_NetToLocalIdLookup = new();
    
    // ReSharper disable once StringLiteralTypo
    private static readonly string NetMsgId = "MTKNET";    //keep this short as it gets transmitted with every message.

    #endregion
    
    #region TYPEDEF

    private static class Counter
    {
        public static uint MinValue { get; private set; } = 10;
        private static uint _counter = 10;
        public static uint GetIncrement() => _counter++;
        public static uint Get() => _counter;
        public static void Reset() => _counter = MinValue;    //values below 10 are reserved.
    }

    public record NetSyncVarIndex(string ModName, string Name);

    #endregion
}