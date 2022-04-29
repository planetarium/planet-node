using Libplanet.Action;

namespace Libplanet.Headless;
public class Configuration
{
    public string? AppProtocolVersionToken { get; set; }

    public string? Host { get; set; }

    public ushort? Port { get; set; }

    public string? MinerPrivateKeyString { get; set; }

    public string? StorePath { get; set; }

    public string[]? IceServerStrings { get; set; }

    public string[]? PeerStrings { get; set; }

    public string[]? TrustedAppProtocolVersionSigners { get; set; }

    public bool GraphQLServer { get; set; }

    public string? GraphQLHost { get; set; }

    public int? GraphQLPort { get; set; }

    public string? GraphQLSecretTokenPath { get; set; }

    public bool NoCors { get; set; }

    public int Workers { get; set; }

    public int Confirmations { get; set; }

    public bool StrictRendering { get; set; }

    public bool Render { get; set; }

    public bool Preload { get; set; }

    public int TxLifeTime { get; set; }

    public int MessageTimeout { get; set; }

    public int TipTimeout { get; set; }

    public int DemandBuffer { get; set; }

    public int MinimumBroadcastTarget { get; set; }

    public int BucketSize { get; set; }

    public string[]? StaticPeerStrings { get; set; }

    public IEnumerable<IAction>? GenesisActions { get; set; }
}
