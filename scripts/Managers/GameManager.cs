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

        public int LocalPlayerCount
        {
            get
            {
                int joypadCount = Input.GetConnectedJoypads().Count;
                return Math.Min(joypadCount == 0 ? 1 : joypadCount, MaxPlayers);
            }
        }

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

            var joypads = Input.GetConnectedJoypads();
            if(joypads.Count == 0) {
                PlayerManager.Instance.RegisterLocalPlayer(1, initialState);
                return;
            }

            foreach(var deviceId in joypads) {
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

        public async Task RestartAsync()
        {
            GD.Print("[GameManager] Restarting game ...");

            // this prevents a weird edge case where
            // a server quitting while everyone paused
            // would leave things in a weird state
            if(PartyParrotManager.Instance.IsPaused) {
                PartyParrotManager.Instance.TogglePause();
            }

            NetworkManager.Instance.Disconnect();
            NetworkManager.Instance.StopServer();

            await LevelManager.Instance.LoadMainMenuAsync().ConfigureAwait(false);
        }
    }
}
