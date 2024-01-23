using Godot;

using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class NetworkManager : SingletonNode<NetworkManager>
    {
        [Export]
        private RPC _rpc;
    }
}
