using Godot;

using pdxpartyparrot.ggj2024.Player;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class LobbyPlayer : Control
    {
        [Export]
        private Label _playerId;

        [Export]
        private Label _color;

        public void Initialize(PlayerInfo player)
        {
            _playerId.Text = player.PlayerId.ToString();
            _color.Text = player.Color.ToString();
        }
    }
}
