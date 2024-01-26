using Godot;

using System;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Player;

namespace pdxpartyparrot.ggj2024.Levels
{
    public partial class Arena : Node
    {
        private int ReadyPlayerCount => PlayerManager.Instance.GetPlayersInStateCount(PlayerInfo.PlayerState.ArenaReady);

        #region Godot Lifecycle

        public override void _Ready()
        {
            NetworkManager.Instance.ServerDisconnectedEvent += ServerDisconnectedEventHandler;
            PlayerManager.Instance.PlayerStateChangedEvent += PlayerStateChangedEventHandler;

            if(NetworkManager.Instance.IsHost) {
                PlayerManager.Instance.UpdateLocalPlayersState(PlayerInfo.PlayerState.ArenaReady);
            } else {
                NetworkManager.Instance.Rpcs.ClientArenaLoaded();
            }
        }

        public override void _ExitTree()
        {
            PlayerManager.Instance.PlayerStateChangedEvent -= PlayerStateChangedEventHandler;
            NetworkManager.Instance.ServerDisconnectedEvent -= ServerDisconnectedEventHandler;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if(@event.IsActionPressed("pause") && !PartyParrotManager.Instance.IsPaused) {
                if(NetworkManager.Instance.IsNetwork) {
                    NetworkManager.Instance.Rpcs.ClientTogglePause();
                } else {
                    PartyParrotManager.Instance.TogglePause();
                }
            }
        }

        #endregion

        #region Event Handlers

        private async void ServerDisconnectedEventHandler(object sender, EventArgs args)
        {
            await LevelManager.Instance.LoadMainMenuAsync().ConfigureAwait(false);
        }

        private void PlayerStateChangedEventHandler(object sender, PlayerManager.PlayerStateEventArgs args)
        {
            if(PlayerManager.Instance.GetPlayerState(args.ClientId, args.DeviceId) == PlayerInfo.PlayerState.ArenaReady) {
                GD.Print($"Player {args.ClientId}:{args.DeviceId} is ready, spawning ...");

                // TODO: spawn the player
            }

            if(PlayerManager.Instance.AreAllPlayersInState(PlayerInfo.PlayerState.ArenaReady)) {
                GD.Print("All players are ready, start game ...");

                // TODO: start the game
            }
        }

        #endregion
    }
}
