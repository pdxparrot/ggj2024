using Godot;

using pdxpartyparrot.ggj2024.Player;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class LobbyPlayer : Control
    {
        [Export]
        private Label _playerId;

        [Export]
        private Label _playerState;

        public void UpdatePlayer(PlayerInfo player)
        {
            if(player == null) {
                _playerId.Text = "No Player";
                _playerState.Text = PlayerInfo.PlayerState.Disconnected.ToString();
            } else {
                _playerId.Text = $"{(player.ClientId == Multiplayer.GetUniqueId() ? "* " : "")}Player {player.PlayerSlot}";
                _playerState.Text = player.State.ToString();
            }
        }
    }
}
