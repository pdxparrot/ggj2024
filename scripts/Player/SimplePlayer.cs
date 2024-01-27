using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    public abstract partial class SimplePlayer : SimpleCharacter
    {
        public long ClientId { get; set; }

        [Export]
        private SimplePlayerInput _input;

        public SimplePlayerInput Input => _input;
    }
}
