using Barotrauma.Networking;

namespace ModdingToolkit.Networking;

/// <summary>
/// Literally just exists because Barotrauma.IWriteMessage is internal only. 
/// </summary>
public interface INetWriteMessage
{
    internal IWriteMessage Message { get; }
    internal void SetMessage(IWriteMessage msg);

    void WriteBoolean(bool val) => Message.WriteBoolean(val);

    void WritePadBits() => Message.WritePadBits();

    void WriteByte(byte val) => Message.WriteByte(val);

    void WriteInt16(short val) => Message.WriteInt16(val);

    void WriteUInt16(ushort val) => Message.WriteUInt16(val);

    void WriteInt32(int val) => Message.WriteInt32(val);

    void WriteUInt32(uint val) => Message.WriteUInt32(val);

    void WriteInt64(long val) => Message.WriteInt64(val);

    void WriteUInt64(ulong val) => Message.WriteUInt64(val);

    void WriteSingle(float val) => Message.WriteSingle(val);

    void WriteDouble(double val) => Message.WriteDouble(val);

    void WriteColorR8G8B8(Color val) => Message.WriteColorR8G8B8(val);

    void WriteColorR8G8B8A8(Color val) => Message.WriteColorR8G8B8A8(val);

    void WriteVariableUInt32(uint val) => Message.WriteVariableUInt32(val);

    void WriteString(string val) => Message.WriteString(val);

    void WriteIdentifier(Identifier val) => Message.WriteIdentifier(val);

    void WriteRangedInteger(int val, int min, int max) => Message.WriteRangedInteger(val, min, max);

    void WriteRangedSingle(float val, float min, float max, int bitCount) =>
        Message.WriteRangedSingle(val, min, max, bitCount);

    void WriteBytes(byte[] val, int startIndex, int length) => Message.WriteBytes(val, startIndex, length);

    byte[] PrepareForSending(bool compressPastThreshold, out bool isCompressed, out int outLength) =>
        Message.PrepareForSending(compressPastThreshold, out isCompressed, out outLength);

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