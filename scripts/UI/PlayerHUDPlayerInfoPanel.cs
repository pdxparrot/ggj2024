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

        private Tween _healthTween;

        private int _playerSlot;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _healthBar.MinValue = 0.0;

            SetProcess(false);
        }

        // this is pretty bad
        public override void _Process(double delta)
        {
            var mecha = (Mecha)PlayerManager.Instance.PlayerObjects[_playerSlot];
            if(mecha == null) {
                _healthBar.Value = 0.0f;
                return;
            }
            UpdateHealth(mecha.CurrentHealth);
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

            SetProcess(true);
        }

        private void UpdateHealth(float value)
        {
            // not sure what's up with tweening the value not working
            // but in the past we tweened a local float
            // and then assigned that to the progress value

            /*if(_healthTween != null) {
                _healthTween.Kill();
            }

            _healthTween = CreateTween().SetEase(Tween.EaseType.In);
            _healthTween.TweenProperty(_healthBar, "value", value, 0.25);*/

            _healthBar.Value = value;
        }
    }
}
