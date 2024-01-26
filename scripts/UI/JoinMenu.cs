using Godot;

using System;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class JoinMenu : Control
    {
        [Export]
        private LineEdit _addressInput;

        [Export]
        private Button[] _buttons = Array.Empty<Button>();

        #region Godot Lifecycle

        public override void _Ready()
        {
            _addressInput.PlaceholderText = NetworkManager.Instance.DefaultAddress;
        }

        #endregion

        private void EnableControls(bool enabled)
        {
            foreach(var button in _buttons) {
                button.Disabled = !enabled;
            }
            _addressInput.Editable = enabled;
        }

        #region Signal Handlers

        private void _on_join_pressed()
        {
            var address = _addressInput.Text;
            if(string.IsNullOrWhiteSpace(address)) {
                address = NetworkManager.Instance.DefaultAddress;
            }

            NetworkManager.Instance.ConnectionFailedEvent += OnConnectionFailed;
            GameManager.Instance.BeginJoinGame(address);

            EnableControls(false);
        }

        #endregion

        #region Event Handlers

        private void OnConnectionFailed(object sender, EventArgs e)
        {
            NetworkManager.Instance.ConnectionFailedEvent -= OnConnectionFailed;

            EnableControls(true);
        }

        #endregion
    }
}
