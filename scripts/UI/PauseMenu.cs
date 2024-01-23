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

        public override void _EnterTree()
        {
            // TODO: for some reason this isn't working if you
            // play, pause, end the game, play again, pause again
            _resumeButton.GrabFocus();
        }

        #endregion

        private void UpdateFullscreenButtons()
        {
            _windowedButton.Visible = PartyParrotManager.Instance.IsFullscreen;
            _fullscreenButton.Visible = !PartyParrotManager.Instance.IsFullscreen;
        }

        #region Signal Handlers

        private void _on_Resume_pressed()
        {
            PartyParrotManager.Instance.TogglePause();
        }

        private void _on_Windowed_pressed()
        {
            PartyParrotManager.Instance.IsFullscreen = false;

            UpdateFullscreenButtons();

            _fullscreenButton.GrabFocus();
        }

        private void _on_Fullscreen_pressed()
        {
            PartyParrotManager.Instance.IsFullscreen = true;

            UpdateFullscreenButtons();

            _windowedButton.GrabFocus();
        }

        private void _on_Quit_pressed()
        {
            PartyParrotManager.Instance.SafeQuit();
        }

        #endregion
    }
}
