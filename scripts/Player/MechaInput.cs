using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class MechaInput : SimplePlayerInput
    {
        // sync'd client -> server
        [Export]
        private Vector2 _lookDirection;

        public Vector2 LookDirection => _lookDirection;

        #region Godot Lifecycle

        // only multiplayer authority processes input
        public override void _Process(double delta)
        {
            // TODO: pre-cache these strings
            _lookDirection = Input.GetVector($"{DeviceId}_look_left", $"{DeviceId}_look_right", $"{DeviceId}_look_up", $"{DeviceId}_look_down");
        }

        #endregion
    }
}
