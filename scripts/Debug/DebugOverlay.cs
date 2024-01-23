using Godot;

using System.Collections.Generic;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Debug
{
    public partial class DebugOverlay : SingletonUI<DebugOverlay>
    {
        [Export]
        private CanvasLayer _canvas;

        private readonly HashSet<IDebugDraw> _draw = new HashSet<IDebugDraw>();

        #region Godot Lifecycle

        public override void _Ready()
        {
            base._Ready();

            _canvas.Hide();
        }

        public override void _Process(double delta)
        {
            QueueRedraw();
        }

        public override void _Draw()
        {
            // TODO 2024:
            /*if(!_canvas.Visible || GameManager.Instance.Level == null || GameManager.Instance.Level.Viewer == null) {
                return;
            }

            var viewer = GameManager.Instance.Level.Viewer;
            foreach(var draw in _draw) {
                draw.DebugDraw(this, viewer.Camera);
            }*/
        }

        #endregion

        public void RegisterDebugDraw(IDebugDraw draw)
        {
            _draw.Add(draw);
        }

        public void UnRegisterDebugDraw(IDebugDraw draw)
        {
            _draw.Remove(draw);
        }

        public void Toggle()
        {
            if(_canvas.Visible) {
                GD.Print("[DebugOverlay] Hide");
                _canvas.Hide();
            } else {
                GD.Print("[DebugOverlay] Show");
                _canvas.Show();
            }
        }
    }
}
