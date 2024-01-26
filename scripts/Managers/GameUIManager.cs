using Godot;

using System;

using pdxpartyparrot.ggj2024.UI;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class GameUIManager : SingletonNode<GameUIManager>
    {
        [Export]
        private PackedScene _pauseMenuScene;

        [Export]
        private PackedScene _playerHUDScene;

        private PauseMenu _pauseMenu;

        public PlayerHUD HUD { get; private set; }

        #region Godot Lifecycle

        public override void _Ready()
        {
            base._Ready();

            CreatePauseMenu();
            CreatePlayerHUD();
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            PartyParrotManager.Instance.PauseEvent += PauseEventHandler;
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            if(PartyParrotManager.HasInstance) {
                PartyParrotManager.Instance.PauseEvent -= PauseEventHandler;
            }
        }

        #endregion

        private void CreatePauseMenu()
        {
            if(IsInstanceValid(_pauseMenu)) {
                GD.PushWarning("[GameUIManager] Re-creating pause menu ...");

                _pauseMenu.QueueFree();
            }

            _pauseMenu = _pauseMenuScene.Instantiate<PauseMenu>();
            _pauseMenu.Name = "Pause Menu";
        }

        private void CreatePlayerHUD()
        {
            if(IsInstanceValid(HUD)) {
                GD.PushWarning("[GameUIManager] Re-creating HUD ...");

                HUD.QueueFree();
            }

            HUD = _playerHUDScene.Instantiate<PlayerHUD>();
            HUD.Name = "Player HUD";
        }

        public void ShowHUD()
        {
            AddChild(HUD);
        }

        public void HideHUD()
        {
            RemoveChild(HUD);
        }

        private void TogglePauseMenu()
        {
            if(PartyParrotManager.Instance.IsPaused) {
                AddChild(_pauseMenu);
            } else {
                RemoveChild(_pauseMenu);
            }
        }

        #region Event Handlers

        private void PauseEventHandler(object sender, EventArgs args)
        {
            TogglePauseMenu();
        }

        #endregion
    }
}
