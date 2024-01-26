using Godot;

using System;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Camera
{
    // this goes on the viewer's pivot container
    // not the camera itself
    public abstract partial class Viewer : Node3D
    {
        private struct ShakeSettings
        {
            public float amount;

            public float power;

            public uint noise_y;
        }

        [Export]
        Camera3D _camera;

        public Camera3D Camera => _camera;

        [Export]
        private FastNoiseLite _noise = new FastNoiseLite();

        private ShakeSettings _shakeSettings;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _noise.Seed = PartyParrotManager.Instance.Random.Next();
        }

        public override void _Process(double delta)
        {
            if(_shakeSettings.amount > 0.0f) {
                // TODO: fixed rate of decay here isn't great
                _shakeSettings.amount = Math.Max(_shakeSettings.amount - 0.4f * (float)delta, 0.0f);

                DoShake();
            }
        }

        #endregion

        #region Events

        public virtual void OnRelease()
        {
        }

        #endregion

        // TODO: this should take a duration
        // from which we can calculate the rate of decay
        public void Shake(float amount, float power = 2.0f)
        {
            _shakeSettings.amount = Math.Clamp(amount, 0.0f, 1.0f);
            _shakeSettings.power = power;
            _shakeSettings.noise_y = 0;
        }

        private void DoShake()
        {
            float amount = (float)Math.Pow(_shakeSettings.amount, _shakeSettings.power);

            _shakeSettings.noise_y += 1;

            var rotation = Camera.Rotation;
            rotation.Z = 0.1f * amount * _noise.GetNoise2D(_noise.Seed, _shakeSettings.noise_y);
            Camera.Rotation = rotation;

            Camera.HOffset = 100.0f * amount * _noise.GetNoise2D(_noise.Seed * 2, _shakeSettings.noise_y);
            Camera.VOffset = 75.0f * amount * _noise.GetNoise2D(_noise.Seed * 3, _shakeSettings.noise_y);
        }
    }
}
