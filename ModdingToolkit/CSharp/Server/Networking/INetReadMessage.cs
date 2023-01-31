using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

/// <summary>
/// Literally just exists because Barotrauma.IReadMessage is internal only. 
/// </summary>
public interface INetReadMessage
{
    internal IReadMessage Message { get; }
    internal void SetMessage(IReadMessage msg);

    bool ReadBoolean() => Message.ReadBoolean();
    void ReadPadBits() => Message.ReadPadBits();
    byte ReadByte() => Message.ReadByte();
    byte PeekByte() => Message.PeekByte();
    ushort ReadUInt16() => Message.ReadUInt16();
    short ReadInt16() => Message.ReadInt16();
    uint ReadUInt32() => Message.ReadUInt32();
    int ReadInt32() => Message.ReadInt32();
    ulong ReadUInt64() => Message.ReadUInt64();
    long ReadInt64() => Message.ReadInt64();
    float ReadSingle() => Message.ReadSingle();
    double ReadDouble() => Message.ReadDouble();
    uint ReadVariableUInt32() => Message.ReadVariableUInt32();
    string ReadString() => Message.ReadString();
    Identifier ReadIdentifier() => Message.ReadIdentifier();
    Color ReadColorR8G8B8() => Message.ReadColorR8G8B8();
    Color ReadColorR8G8B8A8() => Message.ReadColorR8G8B8A8();
    int ReadRangedInteger(int min, int max) => Message.ReadRangedInteger(min, max);
    float ReadRangedSingle(float min, float max, int bitCount) => Message.ReadRangedSingle(min, max, bitCount);
    byte[] ReadBytes(int numberOfBytes) => Message.ReadBytes(numberOfBytes);
    int BitPosition
    {
        get => Message.BitPosition;
        set => Message.BitPosition = value;
    }
    int BytePosition => Message.BytePosition;
    byte[] Buffer => Message.Buffer;
    int LengthBits
    {
        get => Message.LengthBits;
        set => Message.LengthBits = value;
    }
    int LengthBytes => Message.LengthBytes;
}