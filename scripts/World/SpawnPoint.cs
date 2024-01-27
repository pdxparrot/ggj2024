using Godot;

using System;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Player;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.World
{
    public partial class SpawnPoint : Node3D
    {
        [Export]
        private string[] _tags = Array.Empty<string>();

        public string[] Tags => _tags;

        [Export]
        private float _minXSpawnRange;

        [Export]
        private float _maxXSpawnRange;

        [Export]
        private float _minZSpawnRange;

        [Export]
        private float _maxZSpawnRange;

        [Export]
        private bool _randomizeRotation;

        private GodotObject _owner;

        private Action _onRelease;

        #region Godot Lifecycle

        public override void _EnterTree()
        {
            Register();
        }

        public override void _ExitTree()
        {
            Release();
            UnRegister();
        }

        #endregion

        private void Register()
        {
            if(SpawnManager.HasInstance) {
                SpawnManager.Instance.RegisterSpawnPoint(this);
            }
        }

        private void UnRegister()
        {
            if(SpawnManager.HasInstance) {
                SpawnManager.Instance.UnregisterSpawnPoint(this);
            }
        }

        protected void InitSpatial(Node3D spatial)
        {
            var xSpawnRange = new FloatRangeConfig {
                Min = _minXSpawnRange,
                Max = _maxXSpawnRange,
            };

            var zSpawnRange = new FloatRangeConfig {
                Min = _minZSpawnRange,
                Max = _maxZSpawnRange,
            };

            var offset = new Vector3(
                xSpawnRange.GetRandomValue() * PartyParrotManager.Instance.Random.NextSign(),
                0.0f,
                zSpawnRange.GetRandomValue() * PartyParrotManager.Instance.Random.NextSign()
            );

            spatial.Position = GlobalPosition + offset;

            var rotation = Rotation;
            if(_randomizeRotation) {
                float angle = PartyParrotManager.Instance.Random.NextSingle((float)Math.PI * 2.0f);
                rotation = rotation.Rotated(Vector3.Up, angle);
            }
            spatial.Rotation = rotation;
        }

        #region Spawn

        public T SpawnFromScene<T>(PackedScene scene, string name) where T : Node3D
        {
            var spawned = scene.Instantiate<T>();
            spawned.Name = name;

            InitSpatial(spawned);

            return spawned;
        }

        #region Players

        public SimplePlayer SpawnPlayer(PackedScene playerScene, string name)
        {
            var player = SpawnFromScene<SimplePlayer>(playerScene, name);

            player.OnSpawn(this);

            return player;
        }

        public void ReSpawnPlayer(SimplePlayer player)
        {
            InitSpatial(player);

            player.OnReSpawn(this);
        }

        #endregion

        #endregion

        #region Acquire

        public bool Acquire(GodotObject owner, Action onRelease = null, bool force = false)
        {
            if(!force && null != _owner) {
                return false;
            }

            Release();

            _owner = owner;
            _onRelease = onRelease;

            UnRegister();

            return true;
        }

        public void Release()
        {
            if(null == _owner) {
                return;
            }

            _onRelease?.Invoke();
            _owner = null;

            Register();
        }

        #endregion
    }
}
