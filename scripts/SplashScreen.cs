using Godot;

using System.Threading.Tasks;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024
{
    public partial class SplashScreen : Node
    {
        [Export]
        private bool _skipSplashImagesInEditor = true;

        private bool SkipSplashImages => _skipSplashImagesInEditor && PartyParrotManager.Instance.IsEditor;

        [Export]
        private Texture2D[] _splashImages = new Texture2D[0];

        [Export]
        private TextureRect _splashImage;

        [Export]
        private Timer _fadeTimer;

        [Export]
        private Timer _displayTimer;

        private int _currentSplashImage = 0;

        #region Godot Lifecycle

        public override async void _Process(double delta)
        {
            if(SkipSplashImages) {
                GD.Print("Skipping splash screen (editor) ...");
                await LoadMainMenuAsync().ConfigureAwait(false);
            }

            if(_fadeTimer.IsStopped() && _displayTimer.IsStopped()) {
                await ShowNextSplashImageAsync().ConfigureAwait(false);
                return;
            }

            if(!_fadeTimer.IsStopped()) {
                _splashImage.Modulate = new Color(1.0f, 1.0f, 1.0f, (float)(_fadeTimer.TimeLeft / _fadeTimer.WaitTime));
            }
        }

        #endregion

        private async Task LoadMainMenuAsync()
        {
            QueueFree();

            await LevelManager.Instance.LoadMainMenuAsync().ConfigureAwait(false);
        }

        private async Task ShowNextSplashImageAsync()
        {
            if(_currentSplashImage >= _splashImages.Length) {
                await LoadMainMenuAsync().ConfigureAwait(false);
                return;
            }

            // TODO: this is just a jank hack for ssj2022, please remove it
            // TODO: lol ggj2024 let's goooooo
            if(_currentSplashImage == 1 && _splashImages.Length > 2) {
                _currentSplashImage = 1 + PartyParrotManager.Instance.Random.CoinFlip();
            } else if(_currentSplashImage == 2 && _splashImages.Length > 2) {
                _currentSplashImage++;
                return;
            }

            _splashImage.Texture = _splashImages[_currentSplashImage];
            _splashImage.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            _displayTimer.Start();

            _currentSplashImage++;
        }

        #region Signals

        private void _on_Timer_timeout()
        {
            _fadeTimer.Start();
        }

        private void _on_Fade_Timer_timeout()
        {
            _splashImage.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }

        #endregion
    }
}
