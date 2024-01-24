using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class JoinMenu : Control
    {
        [Export]
        private LineEdit _addressInput;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _addressInput.PlaceholderText = NetworkManager.Instance.DefaultAddress;
        }

        #endregion

        #region Signal Handlers

        private async void _on_join_pressed()
        {
            var address = _addressInput.Text;
            if(string.IsNullOrWhiteSpace(address)) {
                address = NetworkManager.Instance.DefaultAddress;
            }

            if(!await GameManager.Instance.JoinGameAsync(address).ConfigureAwait(false)) {
                // TODO: show error
            }
        }

        #endregion
    }
}
