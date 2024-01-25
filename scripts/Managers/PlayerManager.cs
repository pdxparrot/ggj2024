using Godot;

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using pdxpartyparrot.ggj2024.Player;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class PlayerManager : SingletonNode<PlayerManager>
    {
        [Export]
        private PackedScene _playerScene;

        private List<PlayerInfo> _players = new List<PlayerInfo>();

        public int ReadyPlayerCount => _players.Count(p => p.IsReady);

        public string SerializePlayerState()
        {
            return JsonConvert.SerializeObject(_players);
        }

        public void DeserializePlayerState(string playerState)
        {
            _players = JsonConvert.DeserializeObject<List<PlayerInfo>>(playerState);
        }

        public void RegisterLocalPlayer(int deviceId)
        {
            GD.Print($"[PlayerManager] Registering local player {deviceId}...");

            var player = new PlayerInfo {
                ClientId = 1,
                DeviceId = deviceId,
                State = PlayerInfo.PlayerState.Ready
            };
            RegisterPlayer(player);
        }

        public void RegisterRemotePlayer(long clientId)
        {
            GD.Print($"[PlayerManager] Registering remote player {clientId}...");

            var player = new PlayerInfo {
                ClientId = clientId,
                DeviceId = PlayerInfo.RemotePlayerDeviceId,
                State = PlayerInfo.PlayerState.Connected
            };
            RegisterPlayer(player);
        }

        private void RegisterPlayer(PlayerInfo player)
        {
            var existing = _players.Find(p => p.ClientId == player.ClientId && p.DeviceId == player.DeviceId);
            if(null != existing) {
                GD.PushWarning($"[PlayerManager] Overwriting player {player.ClientId}:{player.DeviceId} already registered!");
                return;
            }
            _players.Add(player);

            NetworkManager.Instance.Rpcs.ServerUpdatePlayerState();
        }

        public void UnRegisterRemotePlayer(long clientId)
        {
            GD.Print($"[PlayerManager] UnRegistering remote player {clientId}...");
            _players.RemoveAll(p => p.ClientId == clientId);

            NetworkManager.Instance.Rpcs.ServerUpdatePlayerState();
        }

        public void RemotePlayerReady(long clientId)
        {
            var player = _players.Find(p => p.ClientId == clientId);
            if(null == player) {
                GD.PushWarning($"[PlayerManager] Failed to ready remote player {clientId}!");
                return;
            }

            GD.Print($"[PlayerManager] Remote player {clientId} ready!");
            player.State = PlayerInfo.PlayerState.Ready;

            NetworkManager.Instance.Rpcs.ServerUpdatePlayerState();
        }
    }
}
