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

        [Export]
        private Timer _gameOverTimer;

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
            if(NetworkManager.Instance.IsServer && !_gameTimer.IsStopped()) {
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

        public void PlayerDied(Mecha mecha)
        {
            var phantom = _viewer as PhantomCamera;
            if(phantom != null) {
                phantom.RemoveFromLookAtGroup(mecha);
                phantom.RemoveFromFollowGroup(mecha);
            }

            if(!NetworkManager.Instance.IsServer) {
                return;
            }

            // TODO: this isn't great
            int aliveCount = 0;
            foreach(var playerObject in PlayerManager.Instance.PlayerObjects) {
                if(playerObject == null) {
                    continue;
                }

                var player = (Mecha)playerObject;
                if(!player.IsDead) {
                    aliveCount++;
                }
            }

            if(aliveCount <= 1) {
                Rpc(nameof(GameOver));
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

            _gameTimer.Stop();

            GameManager.Instance.GameOver();

            _gameOverTimer.Start();

            // TODO: this is terrible

            int highestHealth = int.MinValue;
            int winnerCount = 0;
            int winnerSlot = 0;
            foreach(var playerObject in PlayerManager.Instance.PlayerObjects) {
                if(playerObject == null) {
                    continue;
                }

                var player = (Mecha)playerObject;
                if(player.CurrentHealth > highestHealth) {
                    highestHealth = player.CurrentHealth;
                    winnerCount = 1;
                    winnerSlot = player.PlayerSlot;
                } else if(player.CurrentHealth == highestHealth) {
                    winnerCount++;
                }
            }

            foreach(var playerObject in PlayerManager.Instance.PlayerObjects) {
                if(playerObject == null) {
                    continue;
                }

                var player = (Mecha)playerObject;
                if(!player.IsDead && player.CurrentHealth >= highestHealth) {
                    player.Win();
                } else {
                    player.Lose(winnerCount > 1);
                }
            }

            if(winnerCount > 1) {
                GameUIManager.Instance.HUD.ShowGameOverDraw();
            } else {
                GameUIManager.Instance.HUD.ShowGameOver(winnerSlot);
            }
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

        private async void _on_game_over_timer_timeout()
        {
            await GameManager.Instance.RestartAsync();
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
