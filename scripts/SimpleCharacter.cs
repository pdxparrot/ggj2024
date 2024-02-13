using Godot;

using JetBrains.Annotations;

using pdxpartyparrot.ggj2024.Debug;
using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Util;
using pdxpartyparrot.ggj2024.World;

namespace pdxpartyparrot.ggj2024
{
    public abstract partial class SimpleCharacter : CharacterBody3D, IDebugDraw
    {
        // the pivot is used for rotation
        // anything that should rotate with the character should be a child of the pivot
        // (hit/hurt boxes, meshes, etc)
        [Export]
        private Node3D _pivot;

        protected Node3D Pivot => _pivot;

        [Export]
        [CanBeNull]
        private Model _model;

        [CanBeNull]
        protected Model Model
        {
            get => _model;
            set
            {
                _model = value;

                if(_model != null) {
                    _model.UpdateMotionBlend(0.0f);
                    SetProcess(true);
                } else {
                    SetProcess(false);
                }
            }
        }

        [Export]
        private CollisionShape3D _collider;

        protected CollisionShape3D Collider => _collider;

        [Export]
        private float _mass = 1.0f;

        public float Mass => _mass;

        [Export]
        private float _maxSpeed = 14.0f;

        public float MaxSpeed
        {
            get => _maxSpeed;
            set => _maxSpeed = value;
        }

        [Export]
        private float _gravityMultiplier = 5.0f;

        public float Speed => Velocity.Length();

        public float HorizontalSpeed => new Vector3(Velocity.X, 0.0f, Velocity.Z).Length();

        public Vector3 Forward => -Pivot.Transform.Basis.Z;

        public Vector3 Heading { get; protected set; }

        public Vector3 Side { get; protected set; } = Vector3.Right;

        private float _gravity;

        protected float Gravity => _gravity;

        private Vector3 _acceleration;

        protected Vector3 Acceleration => _acceleration;

        #region Godot Lifecycle

        public override void _EnterTree()
        {
            DebugOverlay.Instance.RegisterDebugDraw(this);
        }

        public override void _ExitTree()
        {
            if(DebugOverlay.HasInstance) {
                DebugOverlay.Instance.UnRegisterDebugDraw(this);
            }
        }

        public override void _Ready()
        {
            _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

            if(Model != null) {
                Model.UpdateMotionBlend(0.0f);
                SetProcess(true);
            } else {
                SetProcess(false);
            }
        }

        public override void _Process(double delta)
        {
            // process should be disbled if the model is null
            // so no need to check for null here
            Model.UpdateMotionBlend(MaxSpeed > 0.0f ? HorizontalSpeed / MaxSpeed : 0.0f);
        }

        // both client and server run physics
        public override void _PhysicsProcess(double delta)
        {
            if(PartyParrotManager.Instance.IsPaused || GameManager.Instance.State != GameManager.GameState.GameOn) {
                return;
            }

            // TODO: a max turn rate and smoothed heading might make this nicer

            ApplyAcceleration((float)delta);

            // calculate horizontal heading
            Heading = new Vector3(Velocity.X, 0.0f, Velocity.Z);
            if(Heading.LengthSquared() > 0.01) {
                Heading = Heading.Normalized();

                // look in the direction we're heading
                Pivot.LookAt(GlobalPosition + Heading, Vector3.Up);
            } else {
                Heading = Forward;
            }

            Side = Heading.Perpendicular();

            // move the player
            MoveAndSlide();
        }

        #endregion

        public virtual void DebugDraw(CanvasItem canvas, Camera3D camera)
        {
            var start = camera.UnprojectPosition(GlobalTransform.Origin);

            var velocity = camera.UnprojectPosition(GlobalTransform.Origin + Velocity);
            canvas.DrawLine(start, velocity, new Color(1.0f, 1.0f, 1.0f), 2.0f);

            var heading = camera.UnprojectPosition(GlobalTransform.Origin + Heading);
            canvas.DrawLine(start, heading, new Color(0.0f, 0.0f, 1.0f), 2.0f);

            var side = camera.UnprojectPosition(GlobalTransform.Origin + Side);
            canvas.DrawLine(start, side, new Color(0.0f, 1.0f, 0.0f), 2.0f);
        }

        protected void ApplyAcceleration(float delta)
        {
            // apply
            if(!IsOnFloor()) {
                _acceleration += Vector3.Down * _gravity * _gravityMultiplier;
            }

            // apply acceleration
            Velocity += _acceleration * delta;
            _acceleration = Vector3.Zero;

            // cap horizontal speed
            Velocity = LimitVelocity(Velocity);
        }

        public virtual void Stop()
        {
            Velocity = new Vector3(0.0f, Velocity.Y, 0.0f);
        }

        public void ApplyForce(Vector3 force)
        {
            if(_mass > 0.0f) {
                force /= _mass;
            }

            _acceleration += force;
        }

        protected Vector3 LimitVelocity(Vector3 velocity)
        {
            float y = velocity.Y;
            velocity.Y = 0.0f;
            velocity = velocity.LimitLength(_maxSpeed);
            velocity.Y = y;
            return velocity;
        }

        #region Spawn

        public virtual void OnSpawn(SpawnPoint spawnPoint)
        {
            // spawnpoint rotates the main object
            // but what we actually want to rotate is the pivot
            Pivot.Rotation = Rotation;
            Rotation = Vector3.Zero;

            OnIdle();
        }

        public virtual void OnReSpawn(SpawnPoint spawnPoint)
        {
            OnIdle();
        }

        public virtual void OnDeSpawn()
        {
        }

        #endregion

        #region Events

        public virtual void OnIdle()
        {
        }

        #endregion
    }
}
