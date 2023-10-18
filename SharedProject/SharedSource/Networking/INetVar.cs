namespace ModdingToolkit.Networking;

public interface INetVar<T> : INetConfigBase where T : IConvertible
{
    public T Value { get; set; }

    public void Initialize(string modName, string name, T value);
}