using Godot;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Player;

namespace pdxpartyparrot.ggj2024.UI
{
    // this is all kinds of bad and slapped together
    public partial class PlayerHUDPlayerInfoPanel : Panel
    {
        [Export]
        private TextureProgressBar _healthBar;

        private int _playerSlot;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _healthBar.MinValue = 0.0;
        }

        public override void _Process(double delta)
        {
            var player = PlayerManager.Instance.Players[_playerSlot];
            if(player == null) {
                _healthBar.Value = Mathf.Lerp(_healthBar.Value, 0.0, (float)delta);
                return;
            }

            var mecha = (Mecha)player.Player;
            if(mecha == null) {
                GD.PushError("player object is null");
            }
            _healthBar.Value = Mathf.Lerp(_healthBar.Value, mecha.CurrentHealth, (float)delta);
        }

        #endregion

        public void Initialize(int playerSlot)
        {
            _playerSlot = playerSlot;

            var player = PlayerManager.Instance.Players[playerSlot];
            if(player != null) {
                var mecha = (Mecha)player.Player;
                if(mecha == null) {
                    GD.PushError("player object is null");
                }
                _healthBar.MaxValue = _healthBar.Value = mecha.MaxHealth;
            }
        }
    }
}
