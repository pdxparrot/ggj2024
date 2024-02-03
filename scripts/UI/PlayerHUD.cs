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

        public void InitializePlayer(int playerSlot)
        {
            _panels[playerSlot].Initialize(playerSlot);
        }
    }
}
