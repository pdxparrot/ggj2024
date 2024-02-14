using Godot;

using System;
using System.Threading.Tasks;

using pdxpartyparrot.ggj2024.Util;
using pdxpartyparrot.ggj2024.Player;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class GameManager : SingletonNode<GameManager>
    {
        public enum GameState
        {
            NotStarted,
            Loading,
            GameOn,
            GameOver,
        }

        [Export]
        private int _maxPlayers = 4;

        public int MaxPlayers => _maxPlayers;

        [Export]
        private Color[] _playerColors = new Color[4];

        public Color[] PlayerColors => _playerColors;

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

        public GameState State { get; private set; } = GameState.NotStarted;

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

        public async Task LoadGameAsync()
        {
            GD.Print($"[GameManager] Loading game ...");

            State = GameState.Loading;

            NetworkManager.Instance.LockServer(true);
            await LevelManager.Instance.LoadLevelAsync(_gameScene).ConfigureAwait(false);
        }

        public void StartGame()
        {
            GD.Print($"[GameManager] Starting game ...");

            State = GameState.GameOn;
        }

        public void GameOver()
        {
            GD.Print("[GameManager] Game over!");

            State = GameState.GameOver;
            GameUIManager.Instance.HUD.ShowGameOver();
        }

        public async Task RestartAsync()
        {
            GD.Print("[GameManager] Restarting game ...");

            State = GameState.NotStarted;

            // this prevents a weird edge case where
            // a server quitting while everyone paused
            // would leave things in a weird state
            if(PartyParrotManager.Instance.IsPaused) {
                PartyParrotManager.Instance.TogglePause();
            }

            NetworkManager.Instance.Disconnect();
            NetworkManager.Instance.StopServer();

            PlayerManager.Instance.DestroyPlayers();
            PlayerManager.Instance.UnRegisterAllPlayers();

            GameUIManager.Instance.HUD.HideGameOver();

            await LevelManager.Instance.LoadMainMenuAsync().ConfigureAwait(false);
        }
    }
}
