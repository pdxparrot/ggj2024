using Godot;

using System;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Levels
{
    public partial class Arena : Node
    {
        #region Godot Lifecycle

        public override void _Ready()
        {
            NetworkManager.Instance.ServerDisconnectedEvent += ServerDisconnectedEventHandler;
        }

        public override void _ExitTree()
        {
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

        #endregion
    }
}
