using Godot;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class Mecha : SimplePlayer
    {
        [Export]
        private Timer _fallTimer;

        public enum Leg
        {
            None,
            Left,
            Right,
        }

        private Leg _lastLeg;

        private bool _thrustersEnabled;

        protected MechaInput MechaInput => (MechaInput)Input;

        // sync'd server -> client
        [Export]
        private bool _move;

        [Export]
        private bool _stunned;

        public bool IsStunned => _stunned;

        #region Godot Lifecycle

        // both client and server run physics
        // bespoke because this thing is a goober movement
        public override void _PhysicsProcess(double delta)
        {
            if(PartyParrotManager.Instance.IsPaused) {
                return;
            }

            // TODO: a max turn rate and smoothed heading might make this nicer

            ApplyAcceleration((float)delta);

            Heading = new Vector3(MechaInput.LookDirection.X, 0.0f, MechaInput.LookDirection.Y);
            if(Heading.LengthSquared() > 0.01) {
                Heading = Heading.Normalized();

                // look in the direction we're heading
                Model.LookAt(GlobalPosition + Heading, Vector3.Up);
            }

            Side = Heading.Perpendicular();

            if(_move) {
                Velocity = LimitVelocity(new Vector3(Forward.X, Velocity.Y, Forward.Z) * MaxSpeed);
                _move = false;
            } else {
                Velocity = LimitVelocity(new Vector3(0.0f, Velocity.Y, 0.0f) * MaxSpeed);
            }

            // move the player
            MoveAndSlide();
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
            GD.Print($"[Player {ClientId}:{Input.DeviceId}] fell over lol");

            _stunned = true;

            _lastLeg = Leg.None;
            _move = false;

            _fallTimer.Start();
        }

        #region RPCs

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcLeftLeg()
        {
            if(_lastLeg == Leg.Left) {
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
                Fall();
                return;
            }

            _lastLeg = Leg.Right;
            _move = true;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcBothLegs()
        {
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

        #region Signal Handlers

        private void _on_fall_timer_timeout()
        {
            _stunned = false;
        }

        #endregion
    }
}
