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
        private PackedScene _lobbyPlayerScene;

        [Export]
        private Node _lobbyPlayerContainer;

        [Export]
        private UI.Button _startButton;

        [Export]
        private Label _playerCount;

        private int ReadyPlayerCount => PlayerManager.Instance.GetPlayersInStateCount(PlayerInfo.PlayerState.LobbyReady);

        #region Godot Lifecycle

        public override void _Ready()
        {
            if(NetworkManager.Instance.IsServer) {
                foreach(var player in GameManager.Instance.RegisterLocalPlayers(PlayerInfo.PlayerState.LobbyReady)) {
                    AddPlayer(player);
                }

                NetworkManager.Instance.PeerConnectedEvent += PeerConnectEventHandler;
                NetworkManager.Instance.PeerDisconnectedEvent += PeerDisconnectEventHandler;

                _startButton.GrabFocus();
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

        private void AddPlayer(PlayerInfo player)
        {
            var lobbyPlayer = _lobbyPlayerScene.Instantiate<LobbyPlayer>();
            lobbyPlayer.Initialize(player);
            lobbyPlayer.Name = player.PlayerId.ToString();
            _lobbyPlayerContainer.AddChild(lobbyPlayer);
        }

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
            AddPlayer(PlayerManager.Instance.RegisterRemotePlayer(args.Id, PlayerInfo.PlayerState.Connected));

            NetworkManager.Instance.Rpcs.ServerLoadLobby(args.Id);
        }

        private void PeerDisconnectEventHandler(object sender, NetworkManager.PeerEventArgs args)
        {
            PlayerManager.Instance.UnRegisterRemotePlayer(args.Id);

            var child = _lobbyPlayerContainer.FindChild(args.Id.ToString());
            _lobbyPlayerContainer.RemoveChild(child);
        }

        private async void ServerDisconnectedEventHandler(object sender, EventArgs args)
        {
            await GameManager.Instance.RestartAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
