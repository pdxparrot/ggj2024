using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024
{
    public partial class RPC : Node
    {
        #region Server -> Client

        public void ClientLoadLobby(long id)
        {
            RpcId(id, nameof(LoadLobby));
        }

        [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private async void LoadLobby()
        {
            GD.Print($"[RPC] Server says load lobby");

            await GameManager.Instance.StartGameAsync().ConfigureAwait(false);

            Rpc(nameof(LobbyLoaded));
        }

        #endregion

        #region Client -> Server

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void LobbyLoaded()
        {
            GD.Print($"[RPC] Client {Multiplayer.GetRemoteSenderId()} says lobby loaded");

            //NetworkManager.Instance.LevelLoadedEvent?.Invoke(this, new PeerEventArgs { Id = id });
        }

        #endregion
    }
}
