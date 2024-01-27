using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class PauseMenu : Control
    {
        [Export]
        private BaseButton _resumeButton;

        [Export]
        private BaseButton _windowedButton;

        [Export]
        private BaseButton _fullscreenButton;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _resumeButton.GrabFocus();


            UpdateFullscreenButtons();
        }

        #endregion

        private void UpdateFullscreenButtons()
        {
            if(_windowedButton != null) {
                _windowedButton.Visible = PartyParrotManager.Instance.IsFullscreen;
            }
            if(_fullscreenButton != null) {
                _fullscreenButton.Visible = !PartyParrotManager.Instance.IsFullscreen;
            }
        }

        #region Signal Handlers

        private void _on_resume_pressed()
        {
            if(NetworkManager.Instance.IsNetwork) {
                NetworkManager.Instance.Rpcs.ClientTogglePause();
            } else {
                PartyParrotManager.Instance.TogglePause();
            }
        }

        private void _on_windowed_pressed()
        {
            PartyParrotManager.Instance.IsFullscreen = false;

            UpdateFullscreenButtons();

            _fullscreenButton.GrabFocus();
        }

        private void _on_fullscreen_pressed()
        {
            PartyParrotManager.Instance.IsFullscreen = true;

            UpdateFullscreenButtons();

            _windowedButton.GrabFocus();
        }

        private async void _on_exit_pressed()
        {
            if(NetworkManager.Instance.IsNetwork) {
                NetworkManager.Instance.Rpcs.ClientTogglePause();
            } else {
                PartyParrotManager.Instance.TogglePause();
            }

            await GameManager.Instance.RestartAsync().ConfigureAwait(false);
        }

        private void _on_quit_pressed()
        {
            PartyParrotManager.Instance.SafeQuit();
        }

        #endregion
    }
}
