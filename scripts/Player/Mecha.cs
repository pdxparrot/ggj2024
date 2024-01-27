using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class Mecha : SimplePlayer
    {
        public enum Leg
        {
            None,
            Left,
            Right,
        }

        private Leg _lastLeg;

        private bool _thrustersEnabled;

        protected MechaInput MechaInput => (MechaInput)Input;

        private bool _move;

        #region Godot Lifecycle

        // both client and server run physics
        public override void _PhysicsProcess(double delta)
        {
            if(PartyParrotManager.Instance.IsPaused) {
                return;
            }

            // TODO: a max turn rate and smoothed heading might make this nicer

            var heading = new Vector3(MechaInput.LookDirection.X, 0.0f, MechaInput.LookDirection.Y);
            if(heading.LengthSquared() > 0.01) {
                heading = heading.Normalized();

                var lookAt = GlobalPosition + heading;
                Model.LookAt(lookAt, Vector3.Up);
            }

            if(_move) {
                // move in the direction the model is facing
                var forward = -Model.GlobalTransform.Basis.Z;
                Velocity = new Vector3(forward.X, Velocity.Y, forward.Z) * Speed;
                MoveAndSlide();

                _move = false;
            }
        }

        #endregion

        public void MoveLeftLeg()
        {
            RpcId(1, nameof(RpcLeftLeg));
        }

        public void MoveRightLeg()
        {
            RpcId(1, nameof(RpcRightLeg));
        }

        public void MoveBothLegs()
        {
            RpcId(1, nameof(RpcBothLegs));
        }

        public void MoveLeftArm()
        {
            RpcId(1, nameof(RpcLeftArm));
        }

        public void MoveRightArm()
        {
            RpcId(1, nameof(RpcRightArm));
        }

        public void ThrustersOn()
        {
            RpcId(1, nameof(RpcThrusters), true);
        }

        public void ThrustersOff()
        {
            RpcId(1, nameof(RpcThrusters), false);
        }

        private void Fall()
        {
            // TODO:

            _move = false;
        }

        #region RPCs

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcLeftLeg()
        {
            if(_lastLeg == Leg.Left) {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] falls over (left)!");
                _lastLeg = Leg.None;

                Fall();
                return;
            }

            _lastLeg = Leg.Left;
            _move = true;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcRightLeg()
        {
            if(_lastLeg == Leg.Right) {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] falls over (right)!");
                _lastLeg = Leg.None;

                Fall();
                return;
            }

            _lastLeg = Leg.Right;
            _move = true;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcBothLegs()
        {
            GD.Print($"[Player {ClientId}:{Input.DeviceId}] falls over (both)!");
            _lastLeg = Leg.None;

            Fall();
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcLeftArm()
        {
            GD.Print($"[Player {ClientId}:{Input.DeviceId}] punches left!");
            // TODO: punch left
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcRightArm()
        {
            GD.Print($"[Player {ClientId}:{Input.DeviceId}] punches right!");
            // TODO: punch right
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcThrusters(bool enable)
        {
            GD.Print($"[Player {ClientId}:{Input.DeviceId}] thrusters: {enable}!");
            _thrustersEnabled = enable;
        }

        #endregion
    }
}
