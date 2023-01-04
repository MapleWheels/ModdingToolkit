using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public static partial class NetworkingManager
{
    public static void SendMsg(IWriteMessage msg, NetworkConnection? conn = null) => GameMain.LuaCs.Networking.Send(msg, conn, DeliveryMethod.Reliable);
    
    private static void ReceiveMessage(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        
    }
}