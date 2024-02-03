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

        public void Initialize()
        {
            for(int i = 0; i < _panels.Length; ++i) {
                _panels[i].Initialize(i);
            }
        }
    }
}
