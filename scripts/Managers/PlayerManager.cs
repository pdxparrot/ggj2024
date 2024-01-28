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
        public struct PlayerId
        {
            public long ClientId = 0;

            public int DeviceId = -1;

            public bool IsRemote => ClientId != 1;

            public PlayerId()
            {
            }
            public override string ToString()
            {
                return $"{ClientId}:{DeviceId}";
            }
        }

        public class PlayerStateEventArgs : EventArgs
        {
            public PlayerId PlayerId { get; set; }
        }

        #region Events

        public event EventHandler<PlayerStateEventArgs> PlayerStateChangedEvent;

        #endregion

        [Export]
        private PackedScene _playerScene;

        [Export]
        private MultiplayerSpawner _spawner;

        [Export]
        private Node _spawnRoot;

        private Dictionary<PlayerId, PlayerInfo> _players = new Dictionary<PlayerId, PlayerInfo>();

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
                var playerId = player.PlayerId;

                _players.Add(playerId, player);
                PlayerStateChanged(playerId, false);
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

        public PlayerInfo.PlayerState GetPlayerState(PlayerId playerId)
        {
            if(!_players.TryGetValue(playerId, out var player)) {
                return PlayerInfo.PlayerState.Disconnected;
            }
            return player.State;
        }

        public PlayerInfo RegisterLocalPlayer(int deviceId, PlayerInfo.PlayerState initialState)
        {
            GD.Print($"[PlayerManager] Registering local player {deviceId}: {initialState}");

            var player = new PlayerInfo {
                ClientId = 1,
                DeviceId = deviceId,
                State = initialState,
                Color = (PlayerInfo.PlayerColor)_players.Count,
            };
            return RegisterPlayer(player);
        }

        public PlayerInfo RegisterRemotePlayer(long clientId, PlayerInfo.PlayerState initialState)
        {
            GD.Print($"[PlayerManager] Registering remote player {clientId}: {initialState}");

            var player = new PlayerInfo {
                ClientId = clientId,
                State = initialState,
                Color = (PlayerInfo.PlayerColor)_players.Count,
            };
            return RegisterPlayer(player);
        }

        private PlayerInfo RegisterPlayer(PlayerInfo player)
        {
            var playerId = player.PlayerId;

            if(_players.TryGetValue(playerId, out var existingPlayer)) {
                GD.PushWarning($"[PlayerManager] Player {playerId} already registered: {existingPlayer.State}");
                return existingPlayer;
            }
            _players.Add(playerId, player);

            PlayerStateChanged(playerId, true);

            return player;
        }

        public void UnRegisterRemotePlayer(long clientId)
        {
            GD.Print($"[PlayerManager] UnRegistering remote player {clientId}...");

            var playerId = new PlayerId {
                ClientId = clientId,
            };

            _players.Remove(playerId);

            PlayerStateChanged(playerId, true);
        }

        public void UnRegisterAllPlayers()
        {
            _players.Clear();
        }

        public void UpdateLocalPlayersState(PlayerInfo.PlayerState state)
        {
            GD.Print($"[PlayerManager] Local players update state: {state}");
            foreach(var kvp in _players) {
                if(kvp.Value.IsRemote) {
                    continue;
                }
                kvp.Value.State = state;

                PlayerStateChanged(kvp.Key, false);
            }

            NetworkManager.Instance.Rpcs.ServerUpdatePlayerState();
        }

        public void UpdateRemotePlayerState(long clientId, PlayerInfo.PlayerState state)
        {
            var playerId = new PlayerId {
                ClientId = clientId,
            };

            if(!_players.TryGetValue(playerId, out var player)) {
                GD.PushWarning($"[PlayerManager] Failed to update remote player {playerId} state!");
                return;
            }

            GD.Print($"[PlayerManager] Remote player {clientId} update state: {state}");
            player.State = state;

            PlayerStateChanged(playerId, true);
        }

        private void PlayerStateChanged(PlayerId playerId, bool broadcast)
        {
            PlayerStateChangedEvent?.Invoke(this, new PlayerStateEventArgs { PlayerId = playerId });

            if(broadcast) {
                NetworkManager.Instance.Rpcs.ServerUpdatePlayerState();
            }
        }

        public SimplePlayer SpawnPlayer(PlayerId playerId)
        {
            GD.Print($"[PlayerManager] Spawning player {playerId}...");

            if(!_players.TryGetValue(playerId, out var player)) {
                GD.PushError("Player not registered!");
                return null;
            }

            var spawnPoint = SpawnManager.Instance.GetPlayerSpawnPoint(playerId);
            if(null == spawnPoint) {
                GD.PushError("Failed to get player spawnpoint!");
                return null;
            }

            if(player.Player != null) {
                spawnPoint.ReSpawnPlayer(player.Player);
            } else {
                player.Player = spawnPoint.SpawnPlayer(_playerScene, $"Player {playerId}");
                player.Player.ClientId = playerId.ClientId;
                player.Player.Input.DeviceId = playerId.DeviceId;
            }

            if(NetworkManager.Instance.IsNetwork) {
                _spawnRoot.AddChild(player.Player);
            } else {
                AddChild(player.Player);
            }

            return player.Player;
        }

        public void DeSpawnPlayer(SimplePlayer player)
        {
            GD.Print($"[PlayerManager] Despawning player {player.Name}");

            player.OnDeSpawn();

            if(NetworkManager.Instance.IsNetwork) {
                _spawnRoot.RemoveChild(player);
            } else {
                RemoveChild(player);
            }
        }

        public void DeSpawnPlayers()
        {
            if(PlayerCount < 1) {
                return;
            }

            foreach(var kvp in _players) {
                DeSpawnPlayer(kvp.Value.Player);
            }
        }

        public void DestroyPlayer(PlayerId playerId, bool remove = true)
        {
            GD.Print($"[PlayerManager] Destroying player {playerId}");

            if(!_players.TryGetValue(playerId, out var player)) {
                GD.PushError("Player not registered!");
                return;
            }

            if(remove) {
                _players.Remove(playerId);
            }

            if(player.Player != null) {
                player.Player.QueueFree();
                player.Player = null;
            }
        }

        public void DestroyPlayers()
        {
            if(PlayerCount < 1) {
                return;
            }

            foreach(var kvp in _players) {
                DestroyPlayer(kvp.Key, false);
            }
        }
    }
}
