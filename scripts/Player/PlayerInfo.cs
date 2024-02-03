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

        // assigned on registration
        public int PlayerSlot { get; set; }

        public PlayerState State { get; set; }

        [JsonIgnore]
        public bool IsHost => ClientId == 1;

        [JsonIgnore]
        public bool IsRemote => !IsHost;

        public override bool Equals(object obj)
        {
            var player = obj as PlayerInfo;
            if(player == null) {
                return false;
            }
            return player.ClientId == ClientId && player.DeviceId == DeviceId && player.State == State;
        }

        public override int GetHashCode()
        {
            // idk man
            return $"{ClientId}:{DeviceId}:{State}".GetHashCode();
        }

        public override string ToString()
        {
            return $"[{PlayerSlot}] {ClientId}:{DeviceId} {State}";
        }
    }
}
