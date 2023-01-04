using ModdingToolkit.Networking;

namespace ModdingToolkit.Config;

public partial class ConfigList : IConfigList, INetConfigEntry<ushort>
{
    public bool IsNetworked => this.NetSync != IConfigBase.NetworkSync.NoSync && GameMain.IsMultiplayer;
    public bool NetAuthorityValidate()
    {
        if (!IsNetworked)
            return true;
        return this.NetSync switch
        {
            IConfigBase.NetworkSync.NoSync => true,
            IConfigBase.NetworkSync.ClientPermissiveDesync => true,
            IConfigBase.NetworkSync.TwoWaySync => true,
            _ => false
        };
    }
}