using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Player
{
    // bespoke input because wtf
    public partial class MechaInput : MultiplayerSynchronizer
    {
        // sync'd client -> server
        [Export]
        private Vector2 _lookDirection;

        public Vector2 LookDirection => _lookDirection;

        public int DeviceId { get; set; }

        #region Godot Lifecycle

        public override void _EnterTree()
        {
            // multiplayer authority set by parent (sync'd from server)
            bool isAuthority = GetMultiplayerAuthority() == NetworkManager.Instance.UniqueId;
            GD.Print($"Player {GetParent<Mecha>().ClientId} input authority: {isAuthority}");
            SetProcess(isAuthority);
        }

        // only multiplayer authority processes input
        public override void _Process(double delta)
        {
            // TODO: pre-cache these strings
            _lookDirection = Input.GetVector($"{DeviceId}_look_left", $"{DeviceId}_look_right", $"{DeviceId}_look_up", $"{DeviceId}_look_down");
        }

        #endregion
    }
}
