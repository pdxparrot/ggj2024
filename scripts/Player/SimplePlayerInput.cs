using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class SimplePlayerInput : MultiplayerSynchronizer
    {
        [Export]
        private SimplePlayer _owner;

        protected SimplePlayer PlayerOwner => _owner;

        public int DeviceId { get; set; }

        #region Godot Lifecycle

        public override void _Ready()
        {
            // multiplayer authority set by parent (sync'd from server)
            bool isAuthority = GetMultiplayerAuthority() == NetworkManager.Instance.UniqueId;
            GD.Print($"[Player {PlayerOwner.ClientId}:{DeviceId}] Input authority: {isAuthority}");
            SetProcess(isAuthority);
        }

        #endregion
    }
}