using Godot;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class PlayerHUDPlayerInfoPanel : Panel
    {
        [Export]
        private TextureProgressBar _healthBar;


        #region Godot Lifecycle

        public override void _Ready()
        {
            _healthBar.Value = 0.0;
        }

        #endregion
    }
}
