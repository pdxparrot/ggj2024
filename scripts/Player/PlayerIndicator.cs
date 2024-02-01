using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class PlayerIndicator : Node
    {
        [Export]
        private MeshInstance3D _pointer;

        [Export]
        private MeshInstance3D _ring;

        [Export]
        private Color[] _colors = new Color[4];

        public void SetColor(int playerSlot)
        {
            var color = _colors[playerSlot];
            ((BaseMaterial3D)_pointer.GetActiveMaterial(0)).AlbedoColor = color;
            ((BaseMaterial3D)_ring.GetActiveMaterial(0)).AlbedoColor = color;
        }
    }
}
