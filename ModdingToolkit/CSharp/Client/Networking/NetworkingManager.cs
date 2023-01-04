using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public static partial class NetworkingManager
{
    public static void SendMsg(IWriteMessage msg) => GameMain.LuaCs.Networking.Send(msg, DeliveryMethod.Reliable);
    
    private static void ReceiveMessage(IReadMessage msg, Barotrauma.Networking.Client? client)
    {
        try
        {
            
        }
        catch (Exception e)
        {
            LuaCsSetup.PrintCsError($"NetworkingManager::");
        }
    }
}