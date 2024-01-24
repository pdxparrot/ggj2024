using Godot;

using System;
using System.Threading.Tasks;

using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class GameManager : SingletonNode<GameManager>
    {
        [Export]
        private int _maxPlayers = 4;

        public int MaxPlayers => _maxPlayers;

        public async Task StartGameAsync()
        {
            GD.Print("[GameManager] Starting game ...");

            // TODO 2024: move this to the level
            //ViewerManager.Instance.InstanceViewers(1);

            await LevelManager.Instance.LoadInitialLevelAsync().ConfigureAwait(false);
        }

        public async Task<bool> HostGameAsync()
        {
            GD.Print("[GameManager] Hosting game ...");

            if(!NetworkManager.Instance.StartLocalServer(MaxPlayers)) {
                return false;
            }

            // TODO 2024: move this to the level
            //ViewerManager.Instance.InstanceViewers(1);

            await LevelManager.Instance.LoadInitialLevelAsync().ConfigureAwait(false);

            return true;
        }

        public void BeginJoinGame(string address)
        {
            GD.Print("[GameManager] Joining game ...");

            NetworkManager.Instance.ConnectedToServerEvent += OnConnectedToServer;
            NetworkManager.Instance.BeginJoinGameSession(address);

            // TODO 2024: move this to the level
            //ViewerManager.Instance.InstanceViewers(1);

            //await LevelManager.Instance.LoadInitialLevelAsync().ConfigureAwait(false);
        }

        #region Event Handlers

        private async void OnConnectedToServer(object sender, EventArgs e)
        {
            NetworkManager.Instance.ConnectedToServerEvent -= OnConnectedToServer;

            // TODO 2024: move this to the level
            //ViewerManager.Instance.InstanceViewers(1);

            await LevelManager.Instance.LoadInitialLevelAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
