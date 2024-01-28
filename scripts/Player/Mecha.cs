using Godot;

using System;
using System.Linq;

using pdxpartyparrot.ggj2024.Interactables;
using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class Mecha : SimplePlayer, IInteractable
    {
        public enum Leg
        {
            None,
            Left,
            Right,
        }

        [Export]
        private int _maxHealth = 10;

        // sync'd server -> client
        [Export]
        private int _currentHealth;

        public bool IsDead => _currentHealth <= 0;

        private Leg _lastLeg;

        protected MechaInput MechaInput => (MechaInput)Input;

        public bool CanMove => !IsDead && !IsStunned && !IsThrustering;

        #region Punch

        [Export]
        private int _punchDamage = 1;

        private bool CanPunch => !IsDead && !IsStunned && !IsThrustering;

        [Export]
        private Interactables.Interactables _leftArmInteractables;

        [Export]
        private Interactables.Interactables _rightArmInteractables;

        #endregion

        #region Thrusters

        [Export]
        private float _thrusterModifier = 3.0f;

        [Export]
        private Timer _thrusterTimer;

        // sync'd client -> server
        [Export]
        private bool _thrusters;

        private bool IsThrustering => _thrusters;

        private bool CanThruster => !IsDead && !IsStunned && !IsThrustering;

        #endregion

        // sync'd server -> client
        [Export]
        private bool _move;

        #region Fall

        [Export]
        private Timer _fallTimer;

        // sync'd server -> client
        [Export]
        private bool _stunned;

        public bool IsStunned => _stunned;

        #endregion

        #region IInteractable

        public bool CanInteract => !IsDead;

        public Type InteractableType => GetType();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            base._Ready();

            _currentHealth = _maxHealth;
        }

        // both client and server run physics
        // bespoke because this thing is a goober movement
        public override void _PhysicsProcess(double delta)
        {
            if(PartyParrotManager.Instance.IsPaused) {
                return;
            }

            // TODO: a max turn rate and smoothed heading might make this nicer

            ApplyAcceleration((float)delta);

            if(CanMove) {
                Heading = new Vector3(MechaInput.LookDirection.X, 0.0f, MechaInput.LookDirection.Y);
                if(Heading.LengthSquared() > 0.01) {
                    Heading = Heading.Normalized();

                    // look in the direction we're heading
                    Model.LookAt(GlobalPosition + Heading, Vector3.Up);
                }
            }

            Side = Heading.Perpendicular();

            if(IsThrustering) {
                Velocity = LimitVelocity(new Vector3(Forward.X, 0.0f, Forward.Z) * MaxSpeed * _thrusterModifier);
            } else if(_move) {
                Velocity = LimitVelocity(new Vector3(Forward.X * MaxSpeed, Velocity.Y, Forward.Z * MaxSpeed));
                _move = false;
            } else {
                Stop();
            }

            // move the player
            MoveAndSlide();
        }

        #endregion

        public void MoveLeftLeg()
        {
            Rpc(nameof(RpcLeftLeg));
        }

        public void MoveRightLeg()
        {
            Rpc(nameof(RpcRightLeg));
        }

        public void MoveBothLegs()
        {
            Rpc(nameof(RpcBothLegs));
        }

        public void MoveLeftArm()
        {
            Rpc(nameof(RpcLeftArm));
        }

        public void MoveRightArm()
        {
            Rpc(nameof(RpcRightArm));
        }

        public void Thrusters()
        {
            Rpc(nameof(RpcThrusters));
        }

        private void Damage(Mecha attacker, int amount)
        {
            if(IsDead) {
                return;
            }

            GD.Print($"[Player {ClientId}:{Input.DeviceId}] hit for {amount} by {attacker.ClientId}:{attacker.Input.DeviceId}!");

            _currentHealth = Math.Max(_currentHealth - amount, 0);
            if(IsDead) {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] died!");
            }
        }

        private void Punch(Interactables.Interactables interactables)
        {
            var enemies = interactables.GetInteractables<Mecha>();

            bool hitSomething = false;
            foreach(var enemy in enemies) {
                var enemyMecha = (Mecha)enemy;
                if(enemyMecha == this) {
                    continue;
                }

                hitSomething = true;
                enemyMecha.Damage(this, _punchDamage);
            }

            if(!hitSomething) {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] wiffed!");
            }
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
            if(!CanMove) {
                return;
            }

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
            if(!CanMove) {
                return;
            }

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
            if(!CanMove) {
                return;
            }

            Fall();
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcLeftArm()
        {
            if(!CanPunch) {
                return;
            }

            Punch(_leftArmInteractables);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcRightArm()
        {
            if(!CanPunch) {
                return;
            }

            Punch(_rightArmInteractables);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcThrusters()
        {
            if(!CanThruster) {
                return;
            }

            _thrusters = true;

            _lastLeg = Leg.None;
            _move = false;

            _thrusterTimer.Start();
        }

        #endregion

        #region Signal Handlers

        private void _on_thruster_timer_timeout()
        {
            _thrusters = false;
        }

        private void _on_fall_timer_timeout()
        {
            _stunned = false;
        }

        #endregion
    }
}
