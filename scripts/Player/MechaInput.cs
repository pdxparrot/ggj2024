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

        public override void _Input(InputEvent @event)
        {
            if(!DisplayServer.WindowIsFocused()) {
                return;
            }

            if(@event.IsActionPressed($"{DeviceId}_left_leg")) {
                GD.Print($"[Player {PlayerOwner.ClientId}:{DeviceId}] TODO: left leg");
            } else if(@event.IsActionPressed($"{DeviceId}_right_leg")) {
                GD.Print($"[Player {PlayerOwner.ClientId}:{DeviceId}] TODO: right leg");
            } else if(@event.IsActionPressed($"{DeviceId}_left_arm")) {
                GD.Print($"[Player {PlayerOwner.ClientId}:{DeviceId}] TODO: left arm");
            } else if(@event.IsActionPressed($"{DeviceId}_right_arm")) {
                GD.Print($"[Player {PlayerOwner.ClientId}:{DeviceId}] TODO: right arm");
            } else if(@event.IsActionPressed($"{DeviceId}_thrusters")) {
                GD.Print($"[Player {PlayerOwner.ClientId}:{DeviceId}] TODO: thrusters on");
            } else if(@event.IsActionReleased($"{DeviceId}_thrusters")) {
                GD.Print($"[Player {PlayerOwner.ClientId}:{DeviceId}] TODO: thrusters off");
            }
        }

        // only multiplayer authority processes input
        public override void _Process(double delta)
        {
            if(!DisplayServer.WindowIsFocused()) {
                return;
            }

            // TODO: pre-cache these strings
            _lookDirection = Input.GetVector($"{DeviceId}_look_left", $"{DeviceId}_look_right", $"{DeviceId}_look_up", $"{DeviceId}_look_down");
        }

        #endregion
    }
}
