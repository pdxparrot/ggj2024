using Godot;

using pdxpartyparrot.ggj2024.Managers;

// TODO: move to Network namespace
namespace pdxpartyparrot.ggj2024
{
    // NOTE: RPCs only support primitive types
    // objects need to be serialized across the wire
    public partial class RPC : Node
    {
        #region Server -> Client

        public void ServerLoadLobby(long id)
        {
            RpcId(id, nameof(LoadLobbyAsync));
        }

        [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private async void LoadLobbyAsync()
        {
            GD.Print($"[RPC] Server says load lobby");

            await GameManager.Instance.CreateGameAsync().ConfigureAwait(false);
        }

        #endregion

        #region Server Broadcast

        public void ServerUpdatePlayerState()
        {
            string playerState = PlayerManager.Instance.SerializePlayerState();
            Rpc(nameof(UpdatePlayerState), playerState);
        }

        [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void UpdatePlayerState(string playerState)
        {
            GD.Print($"[RPC] Server update player state");

            PlayerManager.Instance.DeserializePlayerState(playerState);
        }

        public void ServerStartGame()
        {
            Rpc(nameof(StartGameAsync));
        }

        [Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private async void StartGameAsync()
        {
            GD.Print($"[RPC] Server says start game");

            await GameManager.Instance.StartGameAsync().ConfigureAwait(false);
        }

        #endregion

        #region Client -> Server

        public void ClientLobbyLoaded()
        {
            RpcId(1, nameof(LobbyLoaded));
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void LobbyLoaded()
        {
            GD.Print($"[RPC] Client {Multiplayer.GetRemoteSenderId()} says lobby loaded");

            PlayerManager.Instance.RemotePlayerReady(Multiplayer.GetRemoteSenderId());
        }

        #endregion

        #region Client Broadcast

        public void ClientTogglePause()
        {
            Rpc(nameof(TogglePause));
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void TogglePause()
        {
            GD.Print($"[RPC] Client {Multiplayer.GetRemoteSenderId()} says toggle pause");

            PartyParrotManager.Instance.TogglePause();
        }

        #endregion
    }
}
