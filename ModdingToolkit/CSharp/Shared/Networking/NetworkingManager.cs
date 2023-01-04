using Barotrauma.Networking;
using ModdingToolkit.Config;

namespace ModdingToolkit.Networking;

internal static partial class NetworkingManager
{
    
    #region INTERNAL_API
    public static bool IsInitialized { get; private set; } = false;
    
    public static INetConfigBase GetNetConfig(string ModName, string Name)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public static bool RegisterNetConfigInstance(INetConfigBase config)
    {
        throw new NotImplementedException();
    }

    public static bool RegisterNetConfigInstance<T>(INetConfigEntry<T> config) where T : IConvertible
    {
        if (ConfigManager.GetConfigMember(config.ModName, config.Name) is null)
            return false;
        uint id = Counter.GetIncrement();
        if (!RegisterOrUpdateNetConfigId(config.ModName, config.Name, id))
        {
            LuaCsSetup.PrintCsError($"Net..Manager::RegisterNetConfigInstance<T>() | The config is not registered with the ConfigManager. Unable to continue. Modname={config.ModName}, Name={config.Name}");
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

    private static void SendNetEvent<T>(uint id, T val) where T : IConvertible
    {
        var msg = WriteMessageHeaders(NetworkEventId.SyncVarSingle);
        msg.WriteUInt32(id);
        Utils.Networking.WriteNetValueFromType(msg, val);
        SendMsg(msg);
    }

    private static IWriteMessage WriteMessageHeaders(NetworkEventId eventId)
    {
        var msg = GameMain.LuaCs.Networking.Start(NetworkingManager.NetMsgId);
        msg.WriteByte((byte)eventId);
        return msg;
    }

    private static bool RegisterOrUpdateNetConfigId(string modName, string name, uint id)
    {
        if (ConfigManager.GetConfigMember(modName, name) is INetConfigBase inb)
        {
            inb.SetNetworkingId(id);
            if (!NetConfigIdRegistry.ContainsKey(modName))
                NetConfigIdRegistry.Add(modName, new Dictionary<string, uint>());
            NetConfigIdRegistry[modName][name] = id;
            Indexer_NetConfigIds[id] = new NetSyncVarIndex(modName, name);
            return true;
        }

        return false;
    }

    private static bool RemoveNetConfigId(string modName, string name)
    {
        if (NetConfigIdRegistry.ContainsKey(modName) 
            && NetConfigIdRegistry[modName].ContainsKey(name))
        {
            uint id = NetConfigIdRegistry[modName][name];
            RemoveNetConfigId(id);
            return true;
        }
        return false;
    }

    private static void RemoveNetConfigId(uint id)
    {
        if (Indexer_NetConfigIds.ContainsKey(id))
        {
            NetSyncVarIndex index = Indexer_NetConfigIds[id];
            if (NetConfigIdRegistry.ContainsKey(index.ModName))
                NetConfigIdRegistry[index.ModName].Remove(index.Name);
            Indexer_NetConfigIds.Remove(id);
        }
    }

    private static void RemoveCallbacks(uint id)
    {
        UpdaterReadCallback.Remove(id);
        UpdaterWriteCallback.Remove(id);
    }

    private static void RegisterCallbacks(uint id, Action<IReadMessage> readHandle, Action<IWriteMessage> writeHandle)
    {
        UpdaterReadCallback[id] = readHandle;
        UpdaterWriteCallback[id] = writeHandle;
    }


    #endregion

    #region CVARDEF

    private static readonly Dictionary<uint, System.Action<IWriteMessage>?> UpdaterWriteCallback = new();
    private static readonly Dictionary<uint, System.Action<IReadMessage>?> UpdaterReadCallback = new();
    private static readonly Dictionary<uint, NetSyncVarIndex> Indexer_NetConfigIds = new();
    private static readonly Dictionary<string, Dictionary<string, uint>> NetConfigIdRegistry = new();
    private static readonly string NetMsgId = "mtk_net";    //keep this short as it gets transmitted with every message.

    #endregion
    
    #region TYPEDEF

    private static class Counter
    {
        private static uint _counter;
        public static uint GetIncrement() => _counter++;
        public static uint Get() => _counter;
        public static void Reset() => _counter = 0;
    }

    public enum NetworkEventId
    {
        Undef = 0,
        RegisterVar,
        DeregisterVar,
        SyncVarSingle
    }

    public record NetSyncVarIndex(string ModName, string Name);

    #endregion
}