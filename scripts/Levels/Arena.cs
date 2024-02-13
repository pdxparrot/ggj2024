using Godot;

using System;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Player;
using pdxpartyparrot.ggj2024.Camera;

namespace pdxpartyparrot.ggj2024.Levels
{
    public partial class Arena : Node
    {
        [Export]
        private Timer _gameTimer;

        // sync'd
        [Export]
        private int _timeRemaining;

        [Export]
        private AudioStreamPlayer _musicPlayer;

        [Export]
        private Viewer _viewer;

        #region Godot Lifecycle

        public override void _ExitTree()
        {
            if(PartyParrotManager.HasInstance) {
                PartyParrotManager.Instance.PauseEvent -= PauseEventHandler;
            }

            if(GameUIManager.HasInstance) {
                GameUIManager.Instance.HideHUD();
            }

            PlayerManager.Instance.PlayerStateChangedEvent -= PlayerStateChangedEventHandler;
            NetworkManager.Instance.ServerDisconnectedEvent -= ServerDisconnectedEventHandler;
        }

        public override void _Ready()
        {
            PartyParrotManager.Instance.PauseEvent += PauseEventHandler;

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

        public override void _Process(double delta)
        {
            if(NetworkManager.Instance.IsServer) {
                _timeRemaining = (int)_gameTimer.TimeLeft;
            }
            GameUIManager.Instance.HUD.UpdateTimer(_timeRemaining);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if(@event.IsActionPressed("pause")) {
                if(NetworkManager.Instance.IsNetwork) {
                    NetworkManager.Instance.TogglePause();
                } else {
                    PartyParrotManager.Instance.TogglePause();
                }
            }

            if(!PartyParrotManager.Instance.IsEditor || PartyParrotManager.Instance.IsPaused) {
                return;
            }

            if(@event is InputEventKey eventKey) {
                if(eventKey.Pressed) {
                    if(eventKey.Keycode == Key.K) {
                        ((Mecha)PlayerManager.Instance.PlayerObjects[0]).Damage(null, 1);
                    } else if(eventKey.Keycode == Key.M) {
                        _musicPlayer.Stop();
                    }
                }
            }
        }

        #endregion

        public void PlayerObjectReady(Mecha mecha)
        {
            var phantom = _viewer as PhantomCamera;
            if(phantom != null) {
                phantom.AddToFollowGroup(mecha);
                phantom.AddToLookAtGroup(mecha);
            }
        }

        public void PlayerObjectExitTree(Mecha mecha)
        {
            var phantom = _viewer as PhantomCamera;
            if(phantom != null) {
                phantom.RemoveFromLookAtGroup(mecha);
                phantom.RemoveFromFollowGroup(mecha);
            }
        }

        #region RPCs

        // server broadcast
        [Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void StartGame()
        {
            GD.Print($"Server says start game");

            GameManager.Instance.StartGame();

            _gameTimer.Start();
        }

        [Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void GameOver()
        {
            GD.Print($"Server says game over");

            GameManager.Instance.GameOver();
        }

        // client -> server
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void ArenaLoaded()
        {
            GD.Print($"Client {Multiplayer.GetRemoteSenderId()} says arena loaded");

            PlayerManager.Instance.UpdateRemotePlayerState(Multiplayer.GetRemoteSenderId(), PlayerInfo.PlayerState.ArenaReady);
        }

        #endregion

        #region Signal Handlers

        private void _on_game_timer_timeout()
        {
            if(NetworkManager.Instance.IsServer) {
                Rpc(nameof(GameOver));
            }
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

        private void PauseEventHandler(object sender, EventArgs args)
        {
            _gameTimer.Paused = PartyParrotManager.Instance.IsPaused;
        }

        #endregion
    }
}
