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

        #region Server Events

        public event EventHandler<EventArgs> ServerStartedEvent;

        public event EventHandler<PeerEventArgs> PeerConnectedEvent;
        public event EventHandler<PeerEventArgs> PeerDisconnectedEvent;

        #endregion

        #region Client Events

        public event EventHandler ConnectedToServerEvent;
        public event EventHandler ConnectionFailedEvent;
        public event EventHandler ServerDisconnectedEvent;

        #endregion

        #endregion

        [Export]
        private RPC _rpc;

        public RPC Rpcs => _rpc;

        [Export]
        private MultiplayerSpawner _spawner;

        public MultiplayerSpawner Spawner => _spawner;

        [Export]
        private Node _spawnRoot;

        public Node SpawnRoot => _spawnRoot;

        [Export]
        private int _listeningPort = 7575;

        public int ListenPort => _listeningPort;

        public string DefaultAddress => $"127.0.0.1:{ListenPort}";

        [Export]
        private ENetConnection.CompressionMode _compressionMode = ENetConnection.CompressionMode.RangeCoder;

        public bool IsNetwork => Multiplayer.MultiplayerPeer != null;

        public bool IsServer => IsNetwork ? Multiplayer.IsServer() : false;

        public long UniqueId => Multiplayer.GetUniqueId();

        #region Godot Lifecycle

        public override void _EnterTree()
        {
            base._EnterTree();

            Multiplayer.MultiplayerPeer = null; // clear the default "offline" peer
        }

        public override void _ExitTree()
        {
            Disconnect();
            StopServer();

            base._ExitTree();
        }

        #endregion

        #region Server

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
            if(Multiplayer.MultiplayerPeer == null || !IsServer) {
                return;
            }

            GD.Print("[NetworkManager] Stopping game server ...");

            Multiplayer.PeerConnected -= OnPeerConnected;
            Multiplayer.PeerDisconnected -= OnPeerDisconnected;

            // TODO: this is not actually closing the connection ....
            // but it works fine in other test apps, soooooo .... ???
            Multiplayer.MultiplayerPeer.Close();
            Multiplayer.MultiplayerPeer = null;
        }

        public void LockServer(bool locked)
        {
            if(IsServer) {
                GD.Print($"[NetworkManager] {(locked ? "locking" : "unlocking")} server ...");
                Multiplayer.MultiplayerPeer.RefuseNewConnections = locked;
            }
        }

        #endregion

        #region Client

        public void BeginJoinLocalGameSession()
        {
            BeginJoinGameSession(DefaultAddress);
        }

        public void BeginJoinGameSession(string address)
        {
            var parts = address.Split(':');
            BeginJoinGameSession(parts[0], int.Parse(parts[1]));
        }

        public void BeginJoinGameSession(string address, int port)
        {
            GD.Print($"[NetworkManager] Joining game session at {address}:{port} ...");

            var peer = new ENetMultiplayerPeer();

            var result = peer.CreateClient(address, port);
            if(result != Error.Ok) {
                GD.PrintErr($"[NetworkManager] Failed to create client: {result}");
                OnConnectionFailed();
                return;
            }

            peer.Host.Compress(_compressionMode);

            Multiplayer.MultiplayerPeer = peer;

            Multiplayer.ConnectedToServer += OnConnectedToServer;
            Multiplayer.ConnectionFailed += OnConnectionFailed;
            Multiplayer.ServerDisconnected += OnServerDisconnected;
        }

        public void Disconnect(bool silent = false)
        {
            if(Multiplayer.MultiplayerPeer == null || IsServer) {
                return;
            }

            if(!silent) {
                GD.Print("[NetworkManager] Disconnecting from game session ...");
            }

            Multiplayer.ConnectedToServer -= OnConnectedToServer;
            Multiplayer.ConnectionFailed -= OnConnectionFailed;
            Multiplayer.ServerDisconnected -= OnServerDisconnected;

            Multiplayer.MultiplayerPeer.Close();
            Multiplayer.MultiplayerPeer = null;
        }

        #endregion

        #region Event Handlers

        #region Server

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

        #region Client

        private void OnConnectedToServer()
        {
            GD.Print($"[NetworkManager] Peer {UniqueId} connected to server!");

            ConnectedToServerEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnConnectionFailed()
        {
            GD.PrintErr($"[NetworkManager] Failed to connect to server!");

            ConnectionFailedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnServerDisconnected()
        {
            GD.Print("[NetworkManager] Server disconnected!");

            ServerDisconnectedEvent?.Invoke(this, EventArgs.Empty);

            Disconnect(true);
        }

        #endregion

        #endregion
    }
}
