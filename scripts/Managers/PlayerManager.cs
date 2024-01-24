using Godot;

using System.Collections.Generic;
using System.Linq;

using pdxpartyparrot.ggj2024.Player;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class PlayerManager : SingletonNode<PlayerManager>
    {
        [Export]
        private PackedScene _playerScene;

        private readonly Dictionary<long, SimplePlayer> _players = new Dictionary<long, SimplePlayer>();

        public IReadOnlyDictionary<long, SimplePlayer> Players => _players;

        public int ReadyPlayerCount => _players.Values.Count(p => p.IsReady);

        public void RegisterPlayer(long clientId)
        {
            GD.Print($"[PlayerManager] Registering player {clientId}...");

            var player = _playerScene.Instantiate<SimplePlayer>();
            player.ClientId = clientId;
            player.State = SimplePlayer.PlayerState.Connected;
            _players.Add(player.ClientId, player);
        }

        public void PlayerReady(long clientId)
        {
            if(_players.TryGetValue(clientId, out SimplePlayer player)) {
                player.State = SimplePlayer.PlayerState.Ready;
            }
        }
    }
}
