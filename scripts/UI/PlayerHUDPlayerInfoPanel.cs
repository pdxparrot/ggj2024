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
            var mecha = (Mecha)PlayerManager.Instance.PlayerObjects[_playerSlot];
            if(mecha == null) {
                //_healthBar.Value = Mathf.Lerp(_healthBar.Value, 0.0, (float)delta);
                _healthBar.Value = 0.0f;
                return;
            }

            //_healthBar.Value = Mathf.Lerp(_healthBar.Value, mecha.CurrentHealth, (float)delta);
            _healthBar.Value = mecha.CurrentHealth;
        }

        #endregion

        public void Initialize(int playerSlot)
        {
            _playerSlot = playerSlot;

            var mecha = (Mecha)PlayerManager.Instance.PlayerObjects[_playerSlot];
            if(mecha == null) {
                GD.PushError("player object is null");
                return;
            }
            _healthBar.MaxValue = _healthBar.Value = mecha.MaxHealth;
        }
    }
}
