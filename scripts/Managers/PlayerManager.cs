using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using pdxpartyparrot.ggj2024.Player;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class PlayerManager : SingletonNode<PlayerManager>
    {
        public class PlayerStateEventArgs : EventArgs
        {
            public long ClientId { get; set; }

            public int DeviceId { get; set; }
        }

        #region Events

        public event EventHandler<PlayerStateEventArgs> PlayerStateChangedEvent;

        #endregion

        private struct PlayerIndex
        {
            public long ClientId;

            public int DeviceId;
        }

        [Export]
        private PackedScene _playerScene;

        private Dictionary<PlayerIndex, PlayerInfo> _players = new Dictionary<PlayerIndex, PlayerInfo>();

        public int PlayerCount => _players.Count;

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

                PlayerStateChanged(player.ClientId, player.DeviceId, false);
            }
        }

        public int GetPlayersInStateCount(PlayerInfo.PlayerState state)
        {
            return _players.Values.Count(player => player.State == state);
        }

        public bool AreAllPlayersInState(PlayerInfo.PlayerState state)
        {
            return GetPlayersInStateCount(state) == PlayerCount;
        }

        public PlayerInfo.PlayerState GetPlayerState(long clientId, int deviceId)
        {
            if(!_players.TryGetValue(new PlayerIndex {
                ClientId = clientId,
                DeviceId = deviceId,
            }, out var player)) {
                return PlayerInfo.PlayerState.Disconnected;
            }
            return player.State;
        }

        public void RegisterLocalPlayer(int deviceId, PlayerInfo.PlayerState initialState)
        {
            GD.Print($"[PlayerManager] Registering local player {deviceId}: {initialState}");

            var player = new PlayerInfo {
                ClientId = 1,
                DeviceId = deviceId,
                State = initialState
            };
            RegisterPlayer(player);
        }

        public void RegisterRemotePlayer(long clientId, PlayerInfo.PlayerState initialState)
        {
            GD.Print($"[PlayerManager] Registering remote player {clientId}: {initialState}");

            var player = new PlayerInfo {
                ClientId = clientId,
                DeviceId = PlayerInfo.RemotePlayerDeviceId,
                State = initialState
            };
            RegisterPlayer(player);
        }

        private void RegisterPlayer(PlayerInfo player)
        {
            var key = new PlayerIndex {
                ClientId = player.ClientId,
                DeviceId = player.DeviceId,
            };

            if(_players.TryGetValue(key, out var existingPlayer)) {
                GD.PushWarning($"[PlayerManager] Player {player.ClientId}:{player.DeviceId} already registered: {existingPlayer.State}");
                return;
            }
            _players.Add(key, player);

            PlayerStateChanged(player.ClientId, player.DeviceId, true);
        }

        public void UnRegisterRemotePlayer(long clientId)
        {
            GD.Print($"[PlayerManager] UnRegistering remote player {clientId}...");

            _players.Remove(new PlayerIndex {
                ClientId = clientId,
                DeviceId = PlayerInfo.RemotePlayerDeviceId,
            });

            PlayerStateChanged(clientId, PlayerInfo.RemotePlayerDeviceId, true);
        }

        public void UpdateLocalPlayersState(PlayerInfo.PlayerState state)
        {
            GD.Print($"[PlayerManager] Local players update state: {state}");
            foreach(var player in _players.Values) {
                if(player.DeviceId == PlayerInfo.RemotePlayerDeviceId) {
                    continue;
                }
                player.State = state;

                PlayerStateChanged(player.ClientId, player.DeviceId, false);
            }

            NetworkManager.Instance.Rpcs.ServerUpdatePlayerState();
        }

        public void UpdateRemotePlayerState(long clientId, PlayerInfo.PlayerState state)
        {
            if(_players.TryGetValue(new PlayerIndex {
                ClientId = clientId,
                DeviceId = PlayerInfo.RemotePlayerDeviceId,
            }, out var player)) {
                GD.Print($"[PlayerManager] Remote player {clientId} update state: {state}");
                player.State = state;
            } else {
                GD.PushWarning($"[PlayerManager] Failed to update remote player {clientId} state!");
                return;
            }

            PlayerStateChanged(clientId, PlayerInfo.RemotePlayerDeviceId, true);
        }

        private void PlayerStateChanged(long clientId, int deviceId, bool broadcast)
        {
            PlayerStateChangedEvent?.Invoke(this, new PlayerStateEventArgs { ClientId = clientId, DeviceId = deviceId });

            if(broadcast) {
                NetworkManager.Instance.Rpcs.ServerUpdatePlayerState();
            }
        }
    }
}
