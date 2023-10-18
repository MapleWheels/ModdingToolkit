using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

public sealed class NetReadMessage : INetReadMessage
{
    private IReadMessage _message;

    IReadMessage INetReadMessage.Message => _message;

    void INetReadMessage.SetMessage(IReadMessage msg)
    {
        _message = msg;
    }
}