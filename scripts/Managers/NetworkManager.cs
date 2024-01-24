using Godot;

using System;

using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class NetworkManager : SingletonNode<NetworkManager>
    {
        public class PeerEventArgs : EventArgs
        {
            public long Id { get; set; }
        }

        #region Events

        public event EventHandler<EventArgs> ServerStartedEvent;

        public event EventHandler<PeerEventArgs> PeerConnectedEvent;
        public event EventHandler<PeerEventArgs> PeerDisconnectedEvent;

        public event EventHandler<PeerEventArgs> LevelLoadedEvent;

        #endregion

        [Export]
        private RPC _rpc;

        [Export]
        private MultiplayerSpawner _spawner;

        public MultiplayerSpawner Spawner => _spawner;

        [Export]
        private int _listeningPort = 7575;

        public int ListenPort => _listeningPort;

        public string DefaultAddress => $"127.0.0.1:{ListenPort}";

        [Export]
        private ENetConnection.CompressionMode _compressionMode = ENetConnection.CompressionMode.RangeCoder;

        public bool IsServer => Multiplayer.MultiplayerPeer == null ? false : Multiplayer.IsServer();

        #region Godot Lifecycle

        public override void _EnterTree()
        {
            base._EnterTree();

            Multiplayer.MultiplayerPeer = null; // clear the default "offline" peer
        }

        public override void _ExitTree()
        {
            StopServer();

            base._ExitTree();
        }

        #endregion

        public bool StartLocalServer(int maxPlayers)
        {
            GD.Print($"[NetworkManager] Starting local game server ...");

            return StartServer(ListenPort, maxPlayers, false);
        }

        private bool StartServer(int port, int maxPlayers, bool isDedicatedServer)
        {
            GD.Print($"[NetworkManager] Starting game server on {port} ...");

            var peer = new ENetMultiplayerPeer();

            var result = peer.CreateServer(port, maxPlayers);
            if(result != Error.Ok) {
                GD.PrintErr($"[NetworkManager] Failed to create server: {result}");
                return false;
            }

            peer.Host.Compress(_compressionMode);

            Multiplayer.MultiplayerPeer = peer;

            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;

            ServerStartedEvent?.Invoke(this, EventArgs.Empty);

            return true;
        }

        public void StopServer()
        {
            if(Multiplayer.MultiplayerPeer == null) {
                return;
            }

            GD.Print("[NetworkManager] Stopping game server ...");

            Multiplayer.PeerConnected -= OnPeerConnected;
            Multiplayer.PeerDisconnected -= OnPeerDisconnected;

            Multiplayer.MultiplayerPeer = null;
        }

        #region Event Handlers

        private void OnPeerConnected(long id)
        {
            GD.Print($"[NetworkManager] Peer {id} connected");

            PeerConnectedEvent?.Invoke(this, new PeerEventArgs { Id = id });
        }

        private void OnPeerDisconnected(long id)
        {
            GD.Print($"[NetworkManager] Peer {id} disconnected");

            PeerDisconnectedEvent?.Invoke(this, new PeerEventArgs { Id = id });
        }

        #endregion
    }
}
