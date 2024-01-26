using Godot;

using System;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Levels
{
    public partial class Lobby : Node
    {
        [Export]
        private UI.Button _startButton;

        [Export]
        private Label _playerCount;

        #region Godot Lifecycle

        public override void _Ready()
        {
            NetworkManager.Instance.ServerDisconnectedEvent += ServerDisconnectedEventHandler;

            if(NetworkManager.Instance.IsHost) {
                GameManager.Instance.RegisterLocalPlayers();

                NetworkManager.Instance.PeerConnectedEvent += PeerConnectEventHandler;
                NetworkManager.Instance.PeerDisconnectedEvent += PeerDisconnectEventHandler;
            } else {
                _startButton.Hide();
            }

            NetworkManager.Instance.Rpcs.ClientLobbyLoaded();

            _playerCount.Text = $"{PlayerManager.Instance.ReadyPlayerCount}/{GameManager.Instance.MaxPlayers}";
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
            _playerCount.Text = $"{PlayerManager.Instance.ReadyPlayerCount}/{GameManager.Instance.MaxPlayers}";
        }

        #endregion

        #region Signal Handlers

        private void _on_start_pressed()
        {
            NetworkManager.Instance.Rpcs.ServerStartGame();
        }

        #endregion

        #region Event Handlers

        private void PeerConnectEventHandler(object sender, NetworkManager.PeerEventArgs args)
        {
            PlayerManager.Instance.RegisterRemotePlayer(args.Id);

            NetworkManager.Instance.Rpcs.ServerLoadLobby(args.Id);
        }

        private void PeerDisconnectEventHandler(object sender, NetworkManager.PeerEventArgs args)
        {
            PlayerManager.Instance.UnRegisterRemotePlayer(args.Id);
        }

        private async void ServerDisconnectedEventHandler(object sender, EventArgs args)
        {
            await LevelManager.Instance.LoadMainMenuAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
