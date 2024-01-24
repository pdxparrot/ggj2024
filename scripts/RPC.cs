using System.Threading;

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
        }

        #endregion

        #region Client -> Server

        public void ClientLobbyLoaded()
        {
            Rpc(nameof(LobbyLoaded));
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void LobbyLoaded()
        {
            GD.Print($"[RPC] Client {Multiplayer.GetRemoteSenderId()} says lobby loaded");

            PlayerManager.Instance.PlayerReady(Multiplayer.GetRemoteSenderId());
        }

        #endregion
    }
}
