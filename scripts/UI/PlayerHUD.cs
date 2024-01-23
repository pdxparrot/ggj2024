using Godot;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class PlayerHUD : Control
    {
        [Export]
        private CanvasLayer _canvas;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _canvas = GetNode<CanvasLayer>("CanvasLayer");
        }

        public override void _Process(double delta)
        {
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
