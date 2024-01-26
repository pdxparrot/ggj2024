using Godot;

using System;
using System.Threading.Tasks;

using pdxpartyparrot.ggj2024.Util;
using pdxpartyparrot.ggj2024.Player;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class GameManager : SingletonNode<GameManager>
    {
        [Export]
        private int _maxPlayers = 4;

        public int MaxPlayers => _maxPlayers;

        [Export]
        private PackedScene _lobbyScene;

        [Export]
        private PackedScene _gameScene;

        public int LocalPlayerCount => Math.Min(Input.GetConnectedJoypads().Count, MaxPlayers);

        #region Godot Lifecycle

        public override void _Ready()
        {
            base._Ready();

            GD.Print($"[GameManager] Connected joypads:");
            foreach(var deviceId in Input.GetConnectedJoypads()) {
                GD.Print($"{deviceId}: {Input.GetJoyName(deviceId)}");
            }
        }

        #endregion

        public async Task CreateGameAsync()
        {
            GD.Print($"[GameManager] Creating game ...");

            await LevelManager.Instance.LoadLevelAsync(_lobbyScene).ConfigureAwait(false);
        }

        public async Task<bool> HostGameAsync()
        {
            GD.Print("[GameManager] Hosting game ...");

            if(!NetworkManager.Instance.StartLocalServer(MaxPlayers - LocalPlayerCount)) {
                return false;
            }

            await CreateGameAsync().ConfigureAwait(false);

            return true;
        }

        public void BeginJoinGame(string address)
        {
            GD.Print("[GameManager] Joining game ...");

            NetworkManager.Instance.BeginJoinGameSession(address);
        }

        public void RegisterLocalPlayers(PlayerInfo.PlayerState initialState)
        {
            GD.Print($"[GameManager] Registering {LocalPlayerCount} local players ...");
            foreach(var deviceId in Input.GetConnectedJoypads()) {
                PlayerManager.Instance.RegisterLocalPlayer(deviceId, initialState);
                if(PlayerManager.Instance.PlayerCount >= MaxPlayers) {
                    break;
                }
            }
        }

        public async Task StartGameAsync()
        {
            GD.Print($"[GameManager] Starting game ...");

            NetworkManager.Instance.LockServer(true);
            await LevelManager.Instance.LoadLevelAsync(_gameScene).ConfigureAwait(false);
        }
    }
}
