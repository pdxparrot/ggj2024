using Godot;

using System.Threading.Tasks;

using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class GameManager : SingletonNode<GameManager>
    {
        public async Task StartGameAsync()
        {
            GD.Print("[GameManager] Starting game ...");

            // TODO 2024:
            //ViewerManager.Instance.InstanceViewers(1);

            await LevelManager.Instance.LoadInitialLevelAsync().ConfigureAwait(false);
        }
    }
}
