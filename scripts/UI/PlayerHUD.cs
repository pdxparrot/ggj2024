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
        private Label _timerLabel;

        [Export]
        private Panel _gameOverPanel;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _gameOverPanel.Visible = false;
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

        public void ShowGameOver()
        {
            _gameOverPanel.Visible = true;
        }

        public void HideGameOver()
        {
            _gameOverPanel.Visible = false;
        }
    }
}
