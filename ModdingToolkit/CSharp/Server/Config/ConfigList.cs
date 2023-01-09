using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigList : IConfigList, INetConfigEntry<ushort>
{
    public bool IsNetworked => this.NetSync != NetworkSync.NoSync;
    public bool NetAuthorityValidate() => true;
}