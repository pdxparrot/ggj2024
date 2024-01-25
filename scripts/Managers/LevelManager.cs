using Godot;

using System;
using System.Threading.Tasks;

using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class LevelManager : SingletonNode<LevelManager>
    {
        [Export]
        private PackedScene _loadingScreenScene;

        public LoadingScreen LoadingScreen { get; private set; }

        [Export]
        private PackedScene _mainMenuScene;

        private Node _currentLevel;

        #region Godot Lifecycle

        public override void _Ready()
        {
            base._Ready();

            CreateLoadingScreen();
        }

        #endregion

        private void CreateLoadingScreen()
        {
            if(IsInstanceValid(LoadingScreen)) {
                GD.PushWarning("[LevelManager] Re-creating Loading Screen ...");

                LoadingScreen.QueueFree();
            }

            LoadingScreen = _loadingScreenScene.Instantiate<LoadingScreen>();
            LoadingScreen.Name = "Loading Screen";
        }

        public void ShowLoadingScreen()
        {
            GD.Print("[LevelManager] Showing loading screen...");
            AddChild(LoadingScreen);
        }

        public void HideLoadingScreen()
        {
            GD.Print("[LevelManager] Hiding loading screen...");
            RemoveChild(LoadingScreen);
        }

        private void SetCurrentLevel(PackedScene scene)
        {
            if(IsInstanceValid(_currentLevel)) {
                GD.PushWarning("[LevelManager] Overwriting valid level scene pointer!");
            }

            _currentLevel = scene.Instantiate();
            _currentLevel.Name = "Level";
            GetTree().Root.AddChild(_currentLevel);
        }

        private void UpdateProgress(float progress)
        {
            GD.Print($"[LevelManager] {progress * 100.0}%");
            // TODO: update loading screen ?
        }

        private void ShowError(Error err)
        {
            GD.Print($"[LevelManager] Error loading level: {err}");
            // TODO: show error on loading screen?
        }

        public async Task LoadMainMenuAsync()
        {
            GD.Print("[LevelManager] Loading main menu ...");
            await LoadLevelAsync(_mainMenuScene).ConfigureAwait(false);
        }

        public async Task LoadLevelAsync(PackedScene level, Action onSuccess = null)
        {
            GD.Print($"[LevelManager] Loading level {level.ResourcePath}...");

            if(IsInstanceValid(_currentLevel)) {
                _currentLevel.QueueFree();
                _currentLevel = null;
            }

            ShowLoadingScreen();

            await ResourceManager.Instance.LoadResourceAsync(level.ResourcePath,
                (owner, args) => {
                    SetCurrentLevel((PackedScene)args.Resource);

                    onSuccess?.Invoke();

                    HideLoadingScreen();
                },
                (owner, args) => ShowError(args.Error),
                (owner, args) => UpdateProgress(args.Progress)
            ).ConfigureAwait(false);
        }
    }
}
