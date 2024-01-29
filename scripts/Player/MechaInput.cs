using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class MechaInput : SimplePlayerInput
    {
        // sync'd client -> server
        [Export]
        private Vector2 _lookDirection;

        public Vector2 LookDirection => _lookDirection;

        private Mecha Mecha => (Mecha)PlayerOwner;

        private bool IsInputAllowed => !PartyParrotManager.Instance.IsPaused && GameManager.Instance.State == GameManager.GameState.GameOn && DisplayServer.WindowIsFocused();

        #region Godot Lifecycle

        // only multiplayer authority processes input
        public override void _Process(double delta)
        {
            if(!IsInputAllowed) {
                return;
            }

            // TODO: pre-cache these strings

            _lookDirection = Input.GetVector($"{DeviceId}_look_left", $"{DeviceId}_look_right", $"{DeviceId}_look_up", $"{DeviceId}_look_down");

            bool leftLegPressed = Input.IsActionJustPressed($"{DeviceId}_left_leg");
            bool rightLegPressed = Input.IsActionJustPressed($"{DeviceId}_right_leg");

            if(leftLegPressed) {
                if(rightLegPressed) {
                    Mecha.MoveBothLegs();
                } else {
                    Mecha.MoveLeftLeg();
                }
            } else if(rightLegPressed) {
                Mecha.MoveRightLeg();
            }

            if(Input.IsActionJustPressed($"{DeviceId}_left_arm")) {
                Mecha.MoveLeftArm();
            }

            if(Input.IsActionJustPressed($"{DeviceId}_right_arm")) {
                Mecha.MoveRightArm();
            }

            if(Input.IsActionJustPressed($"{DeviceId}_thrusters")) {
                Mecha.Thrusters();
            }
        }

        #endregion
    }
}
