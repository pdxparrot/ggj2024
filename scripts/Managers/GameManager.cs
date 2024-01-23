using Godot;

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

        public async Task HostGameAsync()
        {
            GD.Print("[GameManager] Hosting game ...");

            // TODO 2024: move this to the level
            //ViewerManager.Instance.InstanceViewers(1);

            await LevelManager.Instance.LoadInitialLevelAsync().ConfigureAwait(false);
        }

        public async Task JoinGameAsync(string address)
        {
            GD.Print("[GameManager] Joining game ...");

            // TODO 2024: move this to the level
            //ViewerManager.Instance.InstanceViewers(1);

            await LevelManager.Instance.LoadInitialLevelAsync().ConfigureAwait(false);
        }
    }
}
