using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public sealed class NetWriteMessage : INetWriteMessage
{
    private IWriteMessage _message;

    IWriteMessage INetWriteMessage.Message => _message;

    void INetWriteMessage.SetMessage(IWriteMessage msg)
    {
        _message = msg;
    }
}