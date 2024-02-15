using Godot;

using System;

using pdxpartyparrot.ggj2024.Interactables;
using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Util;
using pdxpartyparrot.ggj2024.Levels;

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

        protected MechaInput MechaInput => (MechaInput)Input;

        [Export]
        private PackedScene[] _playerModelScenes = Array.Empty<PackedScene>();

        [Export]
        private int _maxHealth = 10;

        public int MaxHealth => _maxHealth;

        // sync'd
        [Export]
        private int _currentHealth;

        public int CurrentHealth => _currentHealth;

        public bool IsDead => _currentHealth <= 0;

        [Export]
        private PlayerIndicator _playerIndicator;

        [Export]
        private AudioStreamPlayer _impactAudioPlayer;

        #region Walk

        [Export]
        private AudioStreamPlayer _stepAudioPlayer;

        private Leg _lastLeg;

        // sync'd
        [Export]
        private bool _move;

        public bool CanMove => CanAction;

        #endregion

        #region Punch

        [Export]
        private int _punchDamage = 1;

        [Export]
        private Timer _leftPunchCooldownTimer;

        // sync'd
        [Export]
        private bool _leftPunchCooldown;

        private bool IsLeftPunchCooldown => _leftPunchCooldown;

        private bool CanLeftPunch => CanAction && !IsLeftPunchCooldown;

        [Export]
        private Timer _rightPunchCooldownTimer;

        // sync'd
        [Export]
        private bool _rightPunchCooldown;

        private bool IsRightPunchCooldown => _rightPunchCooldown;

        private bool CanRightPunch => CanAction && !IsRightPunchCooldown;

        [Export]
        private Interactables.Interactables _leftArmInteractables;

        [Export]
        private Interactables.Interactables _rightArmInteractables;

        [Export]
        private AudioStreamPlayer _punchAudioPlayer;

        #endregion

        #region Thrusters

        [Export]
        private float _thrusterModifier = 3.0f;

        [Export]
        private Timer _thrusterTimer;

        [Export]
        private Timer _thrusterCooldown/*Timer*/;

        // sync'd
        [Export]
        private bool _thrusters;

        private bool IsThrustering => _thrusters;

        // sync'd
        [Export]
        private bool _thrustersCooldown;

        private bool IsThrusterCooldown => _thrustersCooldown;

        private bool CanThruster => CanMove && !IsThrusterCooldown;

        #endregion

        #region Fall

        [Export]
        private Timer _fallTimer;

        // sync'd
        [Export]
        private bool _stunned;

        public bool IsStunned => _stunned;

        private bool CanAction => GameManager.Instance.IsGameOn && !IsDead && !IsStunned && !IsThrustering;

        #endregion

        #region IInteractable

        public bool CanInteract => !IsDead;

        public Type InteractableType => GetType();

        #endregion

        #region Godot Lifecycle

        public override void _ExitTree()
        {
            if(LevelManager.HasInstance) {
                var arena = LevelManager.Instance.CurrentLevel as Arena;
                if(arena != null) {
                    arena.PlayerObjectExitTree(this);
                }
            }

            base._ExitTree();
        }

        public override void _Ready()
        {
            base._Ready();

            _currentHealth = _maxHealth;

            ((Arena)LevelManager.Instance.CurrentLevel).PlayerObjectReady(this);
        }

        // both client and server run physics
        // bespoke because this thing is a goober movement
        public override void _PhysicsProcess(double delta)
        {
            if(PartyParrotManager.Instance.IsPaused || GameManager.Instance.State != GameManager.GameState.GameOn) {
                return;
            }

            // TODO: a max turn rate and smoothed heading might make this nicer

            ApplyAcceleration((float)delta);

            if(CanMove) {
                Heading = new Vector3(MechaInput.LookDirection.X, 0.0f, MechaInput.LookDirection.Y);
                if(Heading.LengthSquared() > 0.01) {
                    Heading = Heading.Normalized();

                    // look in the direction we're heading
                    Pivot.LookAt(GlobalPosition + Heading, Vector3.Up);
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

        public void Win()
        {
            GD.Print($"[Player {ClientId}:{Input.DeviceId}] won!");

            Model.ChangeState("Win");
        }

        public void Lose(bool draw)
        {
            if(draw) {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] drew!");
            } else {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] lost!");
            }

            Model.ChangeState("Lose");
        }

        public void Damage(Mecha attacker, int amount)
        {
            if(IsDead) {
                return;
            }

            if(attacker != null) {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] hit for {amount} by {attacker.ClientId}:{attacker.Input.DeviceId}!");
            } else {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] hit for {amount} by system!");
            }

            _impactAudioPlayer.Play();

            _currentHealth = Math.Max(_currentHealth - amount, 0);
            GameUIManager.Instance.HUD.UpdatePlayerHealth(PlayerSlot, _currentHealth);

            if(IsDead) {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] died!");
                Model.ChangeState("Death");

                ((Arena)LevelManager.Instance.CurrentLevel).PlayerDied(this);
            } else {
                Model.ChangeState("Hit");
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

            Model.ChangeState("Fall");
        }

        protected override void OnPlayerSlotChanged()
        {
            _playerIndicator.SetColor(PlayerSlot);

            Model.InstantiateModel(_playerModelScenes[PlayerSlot]);
        }

        #region RPCs

        // client broadcast
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

            Model.ChangeState("StepLeft");
            _stepAudioPlayer.Play();
        }

        // client broadcast
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

            Model.ChangeState("StepRight");
            _stepAudioPlayer.Play();
        }

        // client broadcast
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcBothLegs()
        {
            if(!CanMove) {
                return;
            }

            Fall();
        }

        // client broadcast
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcLeftArm()
        {
            if(!CanLeftPunch) {
                return;
            }

            Model.ChangeState("PunchLeft");
            _punchAudioPlayer.Play();

            Punch(_leftArmInteractables);

            _leftPunchCooldown = true;
            _leftPunchCooldownTimer.Start();
        }

        // client broadcast
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcRightArm()
        {
            if(!CanRightPunch) {
                return;
            }

            Model.ChangeState("PunchRight");
            _punchAudioPlayer.Play();

            Punch(_rightArmInteractables);

            _rightPunchCooldown = true;
            _rightPunchCooldownTimer.Start();
        }

        // client broadcast
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcThrusters()
        {
            if(!CanThruster) {
                return;
            }

            _thrusters = true;

            _lastLeg = Leg.None;
            _move = false;

            Model.ChangeState("Dash");
            _thrusterTimer.Start();
        }

        #endregion

        #region Signal Handlers

        private void _on_left_punch_cooldown_timeout()
        {
            _leftPunchCooldown = false;
        }

        private void _on_right_punch_cooldown_timeout()
        {
            _rightPunchCooldown = false;
        }

        private void _on_thruster_timer_timeout()
        {
            _thrusters = false;

            _thrustersCooldown = true;
            _thrusterCooldown.Start();
        }

        private void _on_thruster_cooldown_timeout()
        {
            _thrustersCooldown = false;
        }

        private void _on_fall_timer_timeout()
        {
            _stunned = false;
        }

        #endregion

        #region Events

        public override void OnIdle()
        {
            base.OnIdle();

            Model.ChangeState("Idle");
        }

        #endregion
    }
}
