using Godot;

using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class NetworkManager : SingletonNode<NetworkManager>
    {
        [Export]
        private RPC _rpc;

        [Export]
        private int _listeningPort = 7777;

        public int ListenPort => _listeningPort;

        public string DefaultAddress => $"127.0.0.1:{ListenPort}";
    }
}
