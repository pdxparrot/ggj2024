using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    public abstract partial class SimplePlayer : Node3D
    {
        public long ClientId { get; set; } = 0;
    }
}
