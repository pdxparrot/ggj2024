using Godot;

using System;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class PlayerHUD : Control
    {
        [Export]
        private CanvasLayer _canvas;

        [Export]
        PlayerHUDPanel[] _panels = Array.Empty<PlayerHUDPanel>();

        [Export]
        Control[] _gameOverControls = Array.Empty<Control>();

        [Export]
        private Label _timerLabel;

        [Export]
        private Panel _gameOverPanel;

        #region Godot Lifecycle

        public override void _EnterTree()
        {
            HideGameOver();
        }

        #endregion

        public void InitializePlayer(int playerSlot)
        {
            _panels[playerSlot].Initialize(playerSlot);
        }

        public void UpdatePlayerHealth(int playerSlot, int value)
        {
            _panels[playerSlot].PlayerPanel.UpdateHealth(value);
        }

        public void UpdateTimer(int timeRemainingInSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(timeRemainingInSeconds);
            _timerLabel.Text = time.ToString(@"mm\:ss");
        }

        public void ShowGameOver(int winnerSlot)
        {
            _gameOverPanel.Visible = true;
            _gameOverControls[winnerSlot].Visible = true;
        }

        public void ShowGameOverDraw()
        {
            _gameOverPanel.Visible = true;
            _gameOverControls[_gameOverControls.Length - 1].Visible = true;
        }

        public void HideGameOver()
        {
            _gameOverPanel.Visible = false;
            foreach(var gameOverControl in _gameOverControls) {
                gameOverControl.Visible = false;
            }
        }
    }
}
