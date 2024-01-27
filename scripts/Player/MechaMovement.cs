using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Player
{
    // bespoke movement because wtf (characater body may not work for this lol)
    public partial class MechaMovement : CharacterBody3D
    {
        [Export]
        private Mecha _owner;

        [Export]
        private float _speed = 100.0f;

        public bool Move { get; set; }

        // both client and server run physics
        public override void _PhysicsProcess(double delta)
        {
            if(PartyParrotManager.Instance.IsPaused) {
                return;
            }

            // TODO: a max turn rate and smoothed heading might make this nicer

            var heading = new Vector3(_owner.MechaInput.LookDirection.X, 0.0f, _owner.MechaInput.LookDirection.Y);
            if(heading.LengthSquared() > 0.01) {
                heading = heading.Normalized();

                var lookAt = GlobalPosition + heading;
                _owner.Model.LookAt(lookAt, Vector3.Up);
            }

            if(Move) {
                // move in the direction the model is facing
                var forward = -_owner.Model.GlobalTransform.Basis.Z;
                Velocity = new Vector3(forward.X, Velocity.Y, forward.Z) * _speed;
                MoveAndSlide();

                Move = false;
            }
        }
    }
}
