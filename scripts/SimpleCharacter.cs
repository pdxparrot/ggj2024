using Godot;

using pdxpartyparrot.ggj2024.Debug;
using pdxpartyparrot.ggj2024.World;

namespace pdxpartyparrot.ggj2024
{
    public abstract partial class SimpleCharacter : Node3D, IDebugDraw
    {
        [Export]
        private CharacterBody3D _movement;

        protected CharacterBody3D Movement => _movement;

        [Export]
        private Model _model;

        protected Model Model => _model;

        [Export]
        private CollisionShape3D _collider;

        protected CollisionShape3D Collider => _collider;

        #region Godot Lifecycle

        public override void _EnterTree()
        {
            DebugOverlay.Instance.RegisterDebugDraw(this);
        }

        public override void _Ready()
        {
            Model.UpdateMotionBlend(0.0f);
        }

        public override void _ExitTree()
        {
            if(DebugOverlay.HasInstance) {
                DebugOverlay.Instance.UnRegisterDebugDraw(this);
            }
        }

        public override void _Process(double delta)
        {
            //Model.UpdateMotionBlend(MaxSpeed > 0.0f ? HorizontalSpeed / MaxSpeed : 0.0f);
        }

        #endregion

        public virtual void DebugDraw(CanvasItem cavas, Camera3D camera)
        {
            var start = camera.UnprojectPosition(GlobalTransform.Origin);

            /*var velocity = camera.UnprojectPosition(GlobalTransform.Origin + Velocity);
            cavas.DrawLine(start, velocity, new Color(1.0f, 1.0f, 1.0f), 2.0f);

            var heading = camera.UnprojectPosition(GlobalTransform.Origin + Heading);
            cavas.DrawLine(start, heading, new Color(0.0f, 0.0f, 1.0f), 2.0f);

            var side = camera.UnprojectPosition(GlobalTransform.Origin + Side);
            cavas.DrawLine(start, side, new Color(0.0f, 1.0f, 0.0f), 2.0f);*/
        }

        #region Spawn

        public virtual void OnSpawn(SpawnPoint spawnPoint)
        {
            // spawnpoint rotates the main object
            // but what we actually want to rotate is the pivot
            _movement.Rotation = Rotation;
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
