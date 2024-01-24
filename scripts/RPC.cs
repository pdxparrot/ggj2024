using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024
{
    public partial class RPC : Node
    {
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void LevelLoaded(long id)
        {
            GD.Print($"Client {id} says level loaded");

            //NetworkManager.Instance.LevelLoadedEvent?.Invoke(this, new PeerEventArgs { Id = id });
        }
    }
}
