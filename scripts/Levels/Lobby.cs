using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Levels
{
    public partial class Lobby : Node
    {
        [Export]
        private PackedScene _gameLevel;

        [Export]
        private UI.Button _startButton;

        [Export]
        private Label _playerCount;

        #region Godot Lifecycle

        public override void _Ready()
        {
            if(NetworkManager.Instance.IsHost) {
                PlayerManager.Instance.RegisterPlayer(NetworkManager.Instance.UniqueId);

                NetworkManager.Instance.PeerConnectedEvent += PeerConnectEventHandler;
            } else {
                _startButton.Hide();
            }

            NetworkManager.Instance.Rpcs.ClientLobbyLoaded();

            _playerCount.Text = $"{PlayerManager.Instance.ReadyPlayerCount}/{GameManager.Instance.MaxPlayers}";
        }

        public override void _Process(double delta)
        {
            // TODO: update this in an event handler, not here
            _playerCount.Text = $"{PlayerManager.Instance.ReadyPlayerCount}/{GameManager.Instance.MaxPlayers}";
        }

        #endregion

        #region Signal Handlers

        private async void _on_start_pressed()
        {
            await LevelManager.Instance.LoadLevelAsync(_gameLevel).ConfigureAwait(false);
        }

        #endregion

        #region Event Handlers

        private void PeerConnectEventHandler(object sender, NetworkManager.PeerEventArgs args)
        {
            PlayerManager.Instance.RegisterPlayer(args.Id);

            NetworkManager.Instance.Rpcs.ClientLoadLobby(args.Id);
        }

        #endregion
    }
}
