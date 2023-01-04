using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigList : IConfigList, INetConfigEntry<ushort>
{
    public bool IsNetworked => this.NetSync != IConfigBase.NetworkSync.NoSync;
    public bool NetAuthorityValidate() => true;
}