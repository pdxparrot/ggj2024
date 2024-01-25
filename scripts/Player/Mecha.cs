using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class Mecha : SimplePlayer
    {
        [Export]
        private MechaInput _input;

        public MechaInput Input => _input;
    }
}
