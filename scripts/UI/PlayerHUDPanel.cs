using Godot;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class PlayerHUDPanel : Control
    {
        [Export]
        private Panel _noPlayerPanel;

        [Export]
        private Panel _playerPanel;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _noPlayerPanel.Visible = true;
            _playerPanel.Visible = false;
        }

        #endregion
    }
}
