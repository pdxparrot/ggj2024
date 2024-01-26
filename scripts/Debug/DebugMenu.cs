using Godot;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Debug
{
    public partial class DebugMenu : SingletonUI<DebugMenu>
    {
        [Export]
        private CanvasLayer _canvas;

        #region Stat Labels

        // TODO: all of these should be created dynamically
        // so the debug menu can be code-driven

        [Export]
        private Label _fps;

        [Export]
        private Label _staticMemory;

        [Export]
        private Label _staticMemoryMax;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            base._Ready();

            _canvas.Hide();
        }

        public override void _Process(double delta)
        {
            if(!_canvas.Visible) {
                return;
            }

            _fps.Text = $"{Performance.GetMonitor(Performance.Monitor.TimeFps)}";
            //frame time = Performance.GetMonitor(Performance.Monitor.TimeProcess)
            //physics time = Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess)
            //draw calls (3d) = Performance.GetMonitor(Performance.Monitor.RenderDrawCallsInFrame)
            //drawcalls (2d) = Performance.GetMonitor(Performance.Monitor.Render2dDrawCallsInFrame)
            _staticMemory.Text = $"{Performance.GetMonitor(Performance.Monitor.MemoryStatic) / 1048576.0f:0.00}MB";
            _staticMemoryMax.Text = $"({Performance.GetMonitor(Performance.Monitor.MemoryStaticMax) / 1048576.0f:0.00}MB)";
            //video memory used = Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed)
            //node count = Performance.GetMonitor(Performance.Monitor.ObjectNodeCount)
            //orphaned node count = Performance.GetMonitor(Performance.Monitor.ObjectOrphanNodeCount)
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if(@event is InputEventKey eventKey) {
                if(eventKey.Pressed && eventKey.Keycode == Key.Quoteleft) {
                    Toggle();
                }
            }
        }

        #endregion

        public void Toggle()
        {
            if(_canvas.Visible) {
                GD.Print("[DebugMenu] Hide");
                _canvas.Hide();
            } else {
                GD.Print("[DebugMenu] Show");
                _canvas.Show();
            }
        }

        #region Signal Handlers

        private void _on_debug_overlay_pressed()
        {
            DebugOverlay.Instance.Toggle();
        }

        private void _on_quit_pressed()
        {
            PartyParrotManager.Instance.SafeQuit();
        }

        #endregion
    }
}
