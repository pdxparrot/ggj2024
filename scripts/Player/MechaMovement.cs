using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    // bespoke movement because wtf (characater body may not work for this lol)
    public partial class MechaMovement : CharacterBody3D
    {
        [Export]
        private Mecha _owner;

        [Export]
        private float _speed = 5.0f;

        // both client and server run physics
        public override void _PhysicsProcess(double delta)
        {
            var heading = new Vector3(_owner.MechaInput.LookDirection.X, 0.0f, _owner.MechaInput.LookDirection.Y);
            if(heading.LengthSquared() > 0.01) {
                heading = heading.Normalized();

                var lookAt = GlobalPosition + heading;
                _owner.Model.LookAt(lookAt, Vector3.Up);
            }
        }
    }
}
