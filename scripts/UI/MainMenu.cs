using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class MainMenu : Control
    {
        #region Sub-Menus

        [Export]
        private Control _mainMenu;

        [Export]
        private Control _joinMenu;

        [Export]
        private Control _credits;

        #endregion

        [Export]
        private BaseButton _playButton;

        [Export]
        private BaseButton _hostButton;

        [Export]
        private BaseButton _joinButton;

        [Export]
        private BaseButton _creditsButton;

        [Export]
        private BaseButton _windowedButton;

        [Export]
        private BaseButton _fullscreenButton;

        #region Join Menu

        [Export]
        private BaseButton _joinMenuBackButton;

        #endregion

        #region Credits

        [Export]
        private BaseButton _creditsBackButton;

        #endregion

        // TODO: this belongs on the main menu state, not the UI
        [Export]
        private AudioStreamPlayer _musicPlayer;

        #region Godot Lifecycle

        public override void _Ready()
        {
            if(_playButton != null) {
                _playButton.GrabFocus();
            }

            if(_hostButton != null) {
                _hostButton.GrabFocus();
            }

            UpdateFullscreenButtons();

            _mainMenu.Show();
            if(_joinMenu != null) {
                _joinMenu.Hide();
            }
            _credits.Hide();

            if(_musicPlayer != null) {
                _musicPlayer.Play();
            }
        }

        public override void _EnterTree()
        {
            if(_playButton != null) {
                _playButton.GrabFocus();
            }

            if(_hostButton != null) {
                _hostButton.GrabFocus();
            }
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

        private async void _on_Play_pressed()
        {
            // TODO 2024:
            //await GameManager.Instance.StartGameAsync().ConfigureAwait(false);
        }

        private async void _on_Host_pressed()
        {
            // TODO 2024:
            //await GameManager.Instance.StartGameAsync().ConfigureAwait(false);
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

        #region Join

        private void _on_Join_pressed()
        {
            _mainMenu.Hide();
            _joinMenu.Show();

            _joinMenuBackButton.GrabFocus();
        }

        private void _on_Join_Back_pressed()
        {
            _mainMenu.Show();
            _joinMenu.Hide();

            _joinButton.GrabFocus();
        }

        #endregion

        #region Credits

        private void _on_Credits_pressed()
        {
            _mainMenu.Hide();
            _credits.Show();

            _creditsBackButton.GrabFocus();
        }

        private void _on_Credits_Back_pressed()
        {
            _mainMenu.Show();
            _credits.Hide();

            _creditsButton.GrabFocus();
        }

        #endregion

        private void _on_Quit_pressed()
        {
            PartyParrotManager.Instance.SafeQuit();
        }

        #endregion
    }
}
