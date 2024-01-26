using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class PlayerInfo : Resource
    {
        public const int RemotePlayerDeviceId = -1;

        public enum PlayerState
        {
            Disconnected,
            Connected,
            LobbyReady,
            ArenaReady,
        }

        public long ClientId { get; set; }

        public int DeviceId { get; set; }

        public PlayerState State { get; set; }
    }
}
