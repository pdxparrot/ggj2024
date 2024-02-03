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

        #region Godot Lifecycle

        public override void _Ready()
        {
            HideHUD();

            foreach(PlayerHUDPanel panel in _panels) {
                panel.Visible = false;
            }
        }

        #endregion

        public void HideHUD()
        {
            _canvas.Hide();
        }

        public void ShowHUD()
        {
            _canvas.Show();
        }
    }
}
