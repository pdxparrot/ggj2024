using Godot;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Player;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class PlayerHUDPlayerInfoPanel : Panel
    {
        [Export]
        private TextureProgressBar _healthBar;

        private float _value;

        private int _playerSlot;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _healthBar.MinValue = 0.0;

            SetProcess(false);
        }

        public override void _Process(double delta)
        {
            _healthBar.Value = _value;
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

            _healthBar.MaxValue = mecha.MaxHealth;

            SetProcess(true);

            UpdateHealth(mecha.CurrentHealth);
        }

        public void UpdateHealth(int value)
        {
            var tween = CreateTween().SetEase(Tween.EaseType.In);
            tween.TweenProperty(this, "_value", (float)value, 0.5);
        }
    }
}
