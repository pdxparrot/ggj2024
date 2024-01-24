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
            _playerCount.Text = "TODO";

            if(!NetworkManager.Instance.IsHost) {
                _startButton.Hide();
            }
        }

        public override void _Process(double delta)
        {
            // TODO: update this in an event handler, not here
            _playerCount.Text = "TODO";
        }

        #endregion

        #region Signal Handlers

        private async void _on_start_pressed()
        {
            await LevelManager.Instance.LoadLevelAsync(_gameLevel).ConfigureAwait(false);
        }

        #endregion
    }
}
