using Godot;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Player;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class PlayerHUDPlayerInfoPanel : Panel
    {
        [Export]
        private TextureProgressBar _healthBar;

        [Export]
        private TextureRect _playerIndicator;

        [Export]
        private TextureRect _playerIcon;

        [Export]
        private Texture2D[] _playerIcons = new Texture2D[4];

        private float _healthValue;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _healthBar.MinValue = 0.0;

            SetProcess(false);
        }

        public override void _Process(double delta)
        {
            _healthBar.Value = _healthValue;
        }

        #endregion

        public void Initialize(int playerSlot)
        {
            var mecha = (Mecha)PlayerManager.Instance.PlayerObjects[playerSlot];
            if(mecha == null) {
                GD.PushError("player object is null");
                return;
            }

            _healthBar.MaxValue = mecha.MaxHealth;
            _playerIndicator.Modulate = GameManager.Instance.PlayerColors[playerSlot];
            _playerIcon.Texture = _playerIcons[playerSlot];

            SetProcess(true);

            UpdateHealth(mecha.CurrentHealth);
        }

        public void UpdateHealth(int value)
        {
            var tween = CreateTween().SetEase(Tween.EaseType.In);
            tween.TweenProperty(this, "_healthValue", (float)value, 0.5);
        }
    }
}
