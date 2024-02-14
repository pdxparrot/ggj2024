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
            public int PlayerSlot { get; set; }
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

        // TODO: the way this is managed right now is pretty sloppy and bad
        private PlayerInfo[] _players;

        public PlayerInfo[] Players => _players;

        public int PlayerCount => _players.Count(player => player != null);

        private SimplePlayer[] _playerObjects;

        public SimplePlayer[] PlayerObjects => _playerObjects;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _players = new PlayerInfo[GameManager.Instance.MaxPlayers];
            _playerObjects = new SimplePlayer[GameManager.Instance.MaxPlayers];
        }

        #endregion

        public string SerializePlayerState()
        {
            return JsonConvert.SerializeObject(_players);
        }

        public void DeserializePlayerState(string playerState)
        {
            var players = JsonConvert.DeserializeObject<PlayerInfo[]>(playerState);
            if(players == null) {
                GD.PushError($"[PlayerManager] failed to deserialize player state!");
                return;
            }

            // this whole thing assumes the deserialized player are in slot order
            for(int i = 0; i < _players.Length; ++i) {
                if(i < players.Length) {
                    var newPlayer = players[i];
                    UpdatePlayerSlot(i, newPlayer);
                } else if(_players[i] != null) {
                    // excess players were removed
                    var player = _players[i];
                    _players[i] = null;
                    PlayerStateChanged(player.PlayerSlot, false);
                }
            }
        }

        private void UpdatePlayerSlot(int playerSlot, PlayerInfo newPlayer)
        {
            var existingPlayer = _players[playerSlot];

            if(newPlayer == null) {
                if(existingPlayer == null) {
                    return;
                }

                // player was removed
                _players[playerSlot] = null;
                PlayerStateChanged(existingPlayer.PlayerSlot, false);
                return;
            }

            // again ... assuming everything this in slot order
            if(playerSlot != newPlayer.PlayerSlot) {
                if(existingPlayer != null) {
                    // player was removed
                    _players[playerSlot] = null;
                    PlayerStateChanged(existingPlayer.PlayerSlot, false);
                }

                GD.PushWarning("[PlayerManager] Skipping missing player slot");
                return;
            }

            if(existingPlayer == null) {
                // player was added
                _players[playerSlot] = newPlayer;
                PlayerStateChanged(newPlayer.PlayerSlot, false);
                return;
            }

            // player didn't change
            if(existingPlayer.Equals(newPlayer)) {
                return;
            }

            _players[playerSlot] = null;
            PlayerStateChanged(existingPlayer.PlayerSlot, false);

            _players[playerSlot] = newPlayer;
            PlayerStateChanged(newPlayer.PlayerSlot, false);
        }

        public int GetPlayersInStateCount(PlayerInfo.PlayerState state)
        {
            return _players.Count(player => player != null && player.State == state);
        }

        public bool AreAllPlayersInState(PlayerInfo.PlayerState state)
        {
            return GetPlayersInStateCount(state) == PlayerCount;
        }

        public PlayerInfo GetPlayer(int playerSlot)
        {
            if(playerSlot < 0 || playerSlot >= _players.Length) {
                return null;
            }
            return _players[playerSlot];
        }

        public PlayerInfo.PlayerState GetPlayerState(int playerSlot)
        {
            if(_players[playerSlot] != null) {
                return _players[playerSlot].State;
            }
            return PlayerInfo.PlayerState.Disconnected;
        }

        public PlayerInfo RegisterLocalPlayer(int deviceId, PlayerInfo.PlayerState initialState)
        {
            GD.Print($"[PlayerManager] Registering local player {deviceId}: {initialState}");

            var player = new PlayerInfo {
                ClientId = 1,
                DeviceId = deviceId,
                State = initialState,
            };
            return RegisterPlayer(player);
        }

        public PlayerInfo RegisterRemotePlayer(long clientId, PlayerInfo.PlayerState initialState)
        {
            GD.Print($"[PlayerManager] Registering remote player {clientId}: {initialState}");

            var player = new PlayerInfo {
                ClientId = clientId,
                State = initialState,
            };
            return RegisterPlayer(player);
        }

        private PlayerInfo RegisterPlayer(PlayerInfo player)
        {
            for(int i = 0; i < _players.Length; ++i) {
                var existingPlayer = _players[i];
                if(existingPlayer != null) {
                    if(existingPlayer.Equals(player)) {
                        GD.PushWarning($"[PlayerManager] Player {player} already registered: {existingPlayer.State}");
                        return existingPlayer;
                    }
                    continue;
                }

                player.PlayerSlot = i;
                _players[i] = player;
                PlayerStateChanged(player.PlayerSlot, true);
                return player;

            }
            return null;
        }

        public int UnRegisterRemotePlayer(long clientId)
        {
            GD.Print($"[PlayerManager] UnRegistering remote player {clientId}...");

            for(int i = 0; i < _players.Length; ++i) {
                var player = _players[i];
                if(player == null || player.ClientId != clientId) {
                    continue;
                }

                _players[i] = null;
                PlayerStateChanged(player.PlayerSlot, true);
                return i;
            }
            return -1;
        }

        public void UnRegisterAllPlayers()
        {
            for(int i = 0; i < _players.Length; ++i) {
                _players[i] = null;
            }
        }

        public void UpdateLocalPlayersState(PlayerInfo.PlayerState state)
        {
            GD.Print($"[PlayerManager] Local players update state: {state}");
            for(int i = 0; i < _players.Length; ++i) {
                var player = _players[i];
                if(player == null || player.IsRemote) {
                    continue;
                }
                player.State = state;

                PlayerStateChanged(player.PlayerSlot, false);
            }

            UpdatePlayerState();
        }

        public void UpdateRemotePlayerState(long clientId, PlayerInfo.PlayerState state)
        {
            for(int i = 0; i < _players.Length; ++i) {
                var player = _players[i];
                if(player.ClientId != clientId) {
                    continue;
                }

                GD.Print($"[PlayerManager] Remote player {clientId} update state: {state}");
                player.State = state;

                PlayerStateChanged(player.PlayerSlot, true);
                return;
            }

            GD.PushWarning($"[PlayerManager] Failed to update remote player {clientId} state!");

        }

        private void PlayerStateChanged(int playerSlot, bool broadcast)
        {
            PlayerStateChangedEvent?.Invoke(this, new PlayerStateEventArgs { PlayerSlot = playerSlot });

            if(broadcast) {
                UpdatePlayerState();
            }
        }

        public SimplePlayer SpawnPlayer(int playerSlot)
        {
            GD.Print($"[PlayerManager] Spawning player {playerSlot}...");

            var player = _players[playerSlot];
            if(player == null) {
                GD.PushError($"Player {playerSlot} not registered!");
                return null;
            }

            var spawnPoint = SpawnManager.Instance.GetPlayerSpawnPoint(playerSlot);
            if(null == spawnPoint) {
                GD.PushError("Failed to get player spawnpoint!");
                return null;
            }

            var playerObject = _playerObjects[playerSlot];
            if(playerObject != null) {
                spawnPoint.ReSpawnPlayer(playerObject);
            } else {
                playerObject = spawnPoint.SpawnPlayer(_playerScene, $"Player {playerSlot}");
                playerObject.ClientId = player.ClientId;
                playerObject.Input.DeviceId = player.DeviceId;
                _playerObjects[playerSlot] = playerObject;
            }

            if(NetworkManager.Instance.IsNetwork) {
                _spawnRoot.AddChild(playerObject);
            } else {
                AddChild(playerObject);
            }

            return playerObject;
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
            for(int i = 0; i < _players.Length; ++i) {
                var player = _players[i];
                if(player == null) {
                    continue;
                }
                DeSpawnPlayer(_playerObjects[i]);
            }
        }

        public void DestroyPlayer(int playerSlot, bool remove = true)
        {
            GD.Print($"[PlayerManager] Destroying player {playerSlot}");

            var player = _players[playerSlot];
            if(player == null) {
                GD.PushError($"Player {playerSlot} not registered!");
                return;
            }

            var playerObject = _playerObjects[playerSlot];
            if(playerObject != null) {
                playerObject.QueueFree();
                _playerObjects[playerSlot] = null;
            }

            if(remove) {
                _players[playerSlot] = null;
            }
        }

        public void DestroyPlayers()
        {
            for(int i = 0; i < _players.Length; ++i) {
                DestroyPlayer(i, false);
            }
        }

        private void UpdatePlayerState()
        {
            string playerState = SerializePlayerState();
            Rpc(nameof(RpcUpdatePlayerState), playerState);
        }

        #region RPCs

        // server broadcast
        [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcUpdatePlayerState(string playerState)
        {
            GD.Print($"[PlayerManager] Server update player state");

            DeserializePlayerState(playerState);
        }

        #endregion
    }
}
