using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigEntry<T> : IConfigEntry<T>, INetConfigEntry<T> where T : IConvertible
{
    public bool IsNetworked => this.NetSync != NetworkSync.NoSync;
    public bool NetAuthorityValidate() => true;
}