using Godot;

using Newtonsoft.Json;

using pdxpartyparrot.ggj2024.Managers;

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

        [JsonIgnore]
        public bool IsRemote => DeviceId == RemotePlayerDeviceId;

        public PlayerState State { get; set; }

        [JsonIgnore]
        public PlayerManager.PlayerId PlayerId => new PlayerManager.PlayerId {
            ClientId = ClientId,
            DeviceId = DeviceId
        };

        [JsonIgnore]
        public SimplePlayer Player { get; set; }
    }
}
