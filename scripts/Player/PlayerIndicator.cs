using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class PlayerIndicator : Node
    {
        [Export]
        private MeshInstance3D _pointer;

        [Export]
        private MeshInstance3D _ring;

        public void SetColor(int playerSlot)
        {
            var color = GameManager.Instance.PlayerColors[playerSlot];
            ((BaseMaterial3D)_pointer.GetActiveMaterial(0)).AlbedoColor = color;
            ((BaseMaterial3D)_ring.GetActiveMaterial(0)).AlbedoColor = color;
        }
    }
}
