using Godot;

using System;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Player;
using pdxpartyparrot.ggj2024.UI;

namespace pdxpartyparrot.ggj2024.Levels
{
    public partial class Lobby : Node
    {
        [Export]
        private Node _lobbyPlayerContainer;

        [Export]
        private UI.Button _startButton;

        [Export]
        private AudioStreamPlayer _musicPlayer;

        #region Godot Lifecycle

        public override void _Ready()
        {
            if(_musicPlayer != null) {
                _musicPlayer.Play();
            }

            for(int i = 0; i < GameManager.Instance.MaxPlayers; ++i) {
                UpdatePlayerSlot(i);
            }

            PlayerManager.Instance.PlayerStateChangedEvent += PlayerStateChangedEventHandler;

            if(NetworkManager.Instance.IsServer) {
                GameManager.Instance.RegisterLocalPlayers(PlayerInfo.PlayerState.LobbyReady);

                NetworkManager.Instance.PeerConnectedEvent += PeerConnectEventHandler;
                NetworkManager.Instance.PeerDisconnectedEvent += PeerDisconnectEventHandler;

                _startButton.GrabFocus();
            } else {
                NetworkManager.Instance.ServerDisconnectedEvent += ServerDisconnectedEventHandler;

                RpcId(1, nameof(LobbyLoaded));

                _startButton.Hide();
            }
        }

        public override void _ExitTree()
        {
            PlayerManager.Instance.PlayerStateChangedEvent -= PlayerStateChangedEventHandler;

            NetworkManager.Instance.PeerConnectedEvent -= PeerConnectEventHandler;
            NetworkManager.Instance.PeerDisconnectedEvent -= PeerDisconnectEventHandler;
            NetworkManager.Instance.ServerDisconnectedEvent -= ServerDisconnectedEventHandler;
        }

        #endregion

        private void UpdatePlayerSlot(int playerSlot)
        {
            var lobbyPlayer = (LobbyPlayer)_lobbyPlayerContainer.GetChild(playerSlot);

            var player = PlayerManager.Instance.GetPlayer(playerSlot);
            if(null == player) {
                lobbyPlayer.UpdatePlayer(null);
                lobbyPlayer.Visible = false;
                return;
            }

            lobbyPlayer.UpdatePlayer(player);
            lobbyPlayer.Visible = true;
        }

        #region RPCs

        [Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private async void LoadGameAsync()
        {
            GD.Print($"[RPC] Server says load game");

            await GameManager.Instance.LoadGameAsync().ConfigureAwait(false);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void LobbyLoaded()
        {
            GD.Print($"[RPC] Client {Multiplayer.GetRemoteSenderId()} says lobby loaded");

            PlayerManager.Instance.UpdateRemotePlayerState(Multiplayer.GetRemoteSenderId(), PlayerInfo.PlayerState.LobbyReady);
        }

        #endregion

        #region Signal Handlers

        private void _on_start_pressed()
        {
            Rpc(nameof(LoadGameAsync));
        }

        private async void _on_cancel_pressed()
        {
            await GameManager.Instance.RestartAsync().ConfigureAwait(false);
        }

        #endregion

        #region Event Handlers

        private void PeerConnectEventHandler(object sender, NetworkManager.PeerEventArgs args)
        {
            PlayerManager.Instance.RegisterRemotePlayer(args.Id, PlayerInfo.PlayerState.Connected);

            NetworkManager.Instance.LoadLobby(args.Id);
        }

        private void PeerDisconnectEventHandler(object sender, NetworkManager.PeerEventArgs args)
        {
            PlayerManager.Instance.UnRegisterRemotePlayer(args.Id);
        }

        private async void ServerDisconnectedEventHandler(object sender, EventArgs args)
        {
            await GameManager.Instance.RestartAsync().ConfigureAwait(false);
        }

        private void PlayerStateChangedEventHandler(object sender, PlayerManager.PlayerStateEventArgs args)
        {
            UpdatePlayerSlot(args.PlayerSlot);
        }

        #endregion
    }
}
