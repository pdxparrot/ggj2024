using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class PlayerHUDPanel : Panel
    {
        [Export]
        private Panel _noPlayerPanel;

        [Export]
        private PlayerHUDPlayerInfoPanel _playerPanel;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _noPlayerPanel.Visible = true;
            _playerPanel.Visible = false;
        }

        #endregion

        public void Initialize(int playerSlot)
        {
            GD.Print($"init player {playerSlot}");

            _noPlayerPanel.Visible = false;
            _playerPanel.Visible = true;

            _playerPanel.Initialize(playerSlot);
        }
    }
}
