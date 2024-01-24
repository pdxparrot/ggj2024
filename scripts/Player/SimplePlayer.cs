using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    public abstract partial class SimplePlayer : Node3D
    {
        public enum PlayerState
        {
            Disconnected,
            Connected,
            Ready,
        }

        public long ClientId { get; set; } = 0;

        public PlayerState State { get; set; } = PlayerState.Disconnected;

        public bool IsReady => State == PlayerState.Ready;
    }
}
