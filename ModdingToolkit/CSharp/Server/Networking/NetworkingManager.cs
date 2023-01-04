using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

internal static partial class NetworkingManager
{
    public static void SendMsg(IWriteMessage msg) => GameMain.LuaCs.Networking.Send(msg, null, DeliveryMethod.Reliable);
    
    private static void ReceiveMessage(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        
    }
}