using Godot;

using Newtonsoft.Json;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class PlayerInfo : Resource
    {
        public enum PlayerState
        {
            Disconnected,
            Connected,
            LobbyReady,
            ArenaReady,
        }

        public long ClientId { get; set; } = 0;

        public int DeviceId { get; set; } = -1;

        public PlayerState State { get; set; }

        [JsonIgnore]
        public bool IsHost => ClientId == 1;

        [JsonIgnore]
        public bool IsRemote => !IsHost;

        [JsonIgnore]
        public PlayerManager.PlayerId PlayerId => new PlayerManager.PlayerId {
            ClientId = ClientId,
            DeviceId = DeviceId
        };

        [JsonIgnore]
        public SimplePlayer Player { get; set; }
    }
}
