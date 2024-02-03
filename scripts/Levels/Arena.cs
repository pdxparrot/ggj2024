using Godot;

using System;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Player;

namespace pdxpartyparrot.ggj2024.Levels
{
    public partial class Arena : Node
    {
        [Export]
        private AudioStreamPlayer _musicPlayer;

        #region Godot Lifecycle

        public override void _ExitTree()
        {
            if(GameUIManager.HasInstance) {
                GameUIManager.Instance.HideHUD();
            }

            PlayerManager.Instance.PlayerStateChangedEvent -= PlayerStateChangedEventHandler;
            NetworkManager.Instance.ServerDisconnectedEvent -= ServerDisconnectedEventHandler;
        }

        public override void _Ready()
        {
            if(_musicPlayer != null) {
                _musicPlayer.Play();
            }
            GameUIManager.Instance.ShowHUD();

            SpawnManager.Instance.Initialize();

            if(NetworkManager.Instance.IsServer) {
                PlayerManager.Instance.PlayerStateChangedEvent += PlayerStateChangedEventHandler;

                PlayerManager.Instance.UpdateLocalPlayersState(PlayerInfo.PlayerState.ArenaReady);
            } else {
                NetworkManager.Instance.ServerDisconnectedEvent += ServerDisconnectedEventHandler;

                RpcId(1, nameof(ArenaLoaded));
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if(@event.IsActionPressed("pause") && !PartyParrotManager.Instance.IsPaused) {
                if(NetworkManager.Instance.IsNetwork) {
                    NetworkManager.Instance.TogglePause();
                } else {
                    PartyParrotManager.Instance.TogglePause();
                }
            }
        }

        #endregion

        #region RPCs

        // server broadcast
        [Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void StartGame()
        {
            GD.Print($"Server says start game");

            GameManager.Instance.StartGame();
        }

        // client -> server
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void ArenaLoaded()
        {
            GD.Print($"Client {Multiplayer.GetRemoteSenderId()} says arena loaded");

            PlayerManager.Instance.UpdateRemotePlayerState(Multiplayer.GetRemoteSenderId(), PlayerInfo.PlayerState.ArenaReady);
        }

        #endregion

        #region Event Handlers

        private async void ServerDisconnectedEventHandler(object sender, EventArgs args)
        {
            await GameManager.Instance.RestartAsync().ConfigureAwait(false);
        }

        private void PlayerStateChangedEventHandler(object sender, PlayerManager.PlayerStateEventArgs args)
        {
            if(PlayerManager.Instance.GetPlayerState(args.PlayerSlot) == PlayerInfo.PlayerState.ArenaReady) {
                GD.Print($"Player {args.PlayerSlot} is ready, spawning ...");
                var mecha = (Mecha)PlayerManager.Instance.SpawnPlayer(args.PlayerSlot);
                mecha.PlayerSlot = args.PlayerSlot;
            }

            if(PlayerManager.Instance.AreAllPlayersInState(PlayerInfo.PlayerState.ArenaReady)) {
                GD.Print("All players are ready, starting game ...");

                Rpc(nameof(StartGame));
            }
        }

        #endregion
    }
}
