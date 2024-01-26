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
        private struct PlayerIndex
        {
            public long ClientId;

            public int DeviceId;
        }

        [Export]
        private PackedScene _playerScene;

        private Dictionary<PlayerIndex, PlayerInfo> _players = new Dictionary<PlayerIndex, PlayerInfo>();

        public int ReadyPlayerCount => _players.Values.Count(p => p.IsReady);

        public string SerializePlayerState()
        {
            return JsonConvert.SerializeObject(_players.Values);
        }

        public void DeserializePlayerState(string playerState)
        {
            var players = JsonConvert.DeserializeObject<List<PlayerInfo>>(playerState);
            if(players == null) {
                GD.PrintErr($"[PlayerManager] failed to deserialize player state!");
                return;
            }

            _players.Clear();
            foreach(var player in players) {
                _players.Add(new PlayerIndex {
                    ClientId = player.ClientId,
                    DeviceId = player.DeviceId,
                }, player);
            }
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
            var key = new PlayerIndex {
                ClientId = player.ClientId,
                DeviceId = player.DeviceId,
            };

            if(_players.ContainsKey(key)) {
                GD.PushWarning($"[PlayerManager] Player {player.ClientId}:{player.DeviceId} already registered!");
                return;
            }
            _players.Add(key, player);

            NetworkManager.Instance.Rpcs.ServerUpdatePlayerState();
        }

        public void UnRegisterRemotePlayer(long clientId)
        {
            GD.Print($"[PlayerManager] UnRegistering remote player {clientId}...");

            _players.Remove(new PlayerIndex {
                ClientId = clientId,
                DeviceId = PlayerInfo.RemotePlayerDeviceId,
            });

            NetworkManager.Instance.Rpcs.ServerUpdatePlayerState();
        }

        public void RemotePlayerReady(long clientId)
        {
            if(_players.TryGetValue(new PlayerIndex {
                ClientId = clientId,
                DeviceId = PlayerInfo.RemotePlayerDeviceId,
            }, out var player)) {
                GD.Print($"[PlayerManager] Remote player {clientId} ready!");
                player.State = PlayerInfo.PlayerState.Ready;
            } else {
                GD.PushWarning($"[PlayerManager] Failed to ready remote player {clientId}!");
                return;
            }

            NetworkManager.Instance.Rpcs.ServerUpdatePlayerState();
        }
    }
}
