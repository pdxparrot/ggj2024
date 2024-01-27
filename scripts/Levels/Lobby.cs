using Godot;

using System;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Player;

namespace pdxpartyparrot.ggj2024.Levels
{
    public partial class Lobby : Node
    {
        [Export]
        private UI.Button _startButton;

        [Export]
        private Label _playerCount;

        private int ReadyPlayerCount => PlayerManager.Instance.GetPlayersInStateCount(PlayerInfo.PlayerState.LobbyReady);

        #region Godot Lifecycle

        public override void _Ready()
        {
            if(NetworkManager.Instance.IsServer) {
                GameManager.Instance.RegisterLocalPlayers(PlayerInfo.PlayerState.LobbyReady);

                NetworkManager.Instance.PeerConnectedEvent += PeerConnectEventHandler;
                NetworkManager.Instance.PeerDisconnectedEvent += PeerDisconnectEventHandler;
            } else {
                NetworkManager.Instance.ServerDisconnectedEvent += ServerDisconnectedEventHandler;

                NetworkManager.Instance.Rpcs.ClientLobbyLoaded();

                _startButton.Hide();
            }

            _playerCount.Text = $"{ReadyPlayerCount}/{GameManager.Instance.MaxPlayers}";
        }

        public override void _ExitTree()
        {
            NetworkManager.Instance.PeerConnectedEvent -= PeerConnectEventHandler;
            NetworkManager.Instance.PeerDisconnectedEvent -= PeerDisconnectEventHandler;
            NetworkManager.Instance.ServerDisconnectedEvent -= ServerDisconnectedEventHandler;
        }

        public override void _Process(double delta)
        {
            // TODO: update this in an event handler, not here
            _playerCount.Text = $"{ReadyPlayerCount}/{GameManager.Instance.MaxPlayers}";
        }

        #endregion

        #region Signal Handlers

        private void _on_start_pressed()
        {
            NetworkManager.Instance.Rpcs.ServerStartGame();
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

            NetworkManager.Instance.Rpcs.ServerLoadLobby(args.Id);
        }

        private void PeerDisconnectEventHandler(object sender, NetworkManager.PeerEventArgs args)
        {
            PlayerManager.Instance.UnRegisterRemotePlayer(args.Id);
        }

        private async void ServerDisconnectedEventHandler(object sender, EventArgs args)
        {
            await GameManager.Instance.RestartAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
