using System;

using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Util
{
    public struct FloatRangeConfig
    {
        [Export]
        private float _min;

        public float Min
        {
            get => _min;
            set => _min = value;
        }

        [Export]
        private float _max;

        public float Max
        {
            get => _max;
            set => _max = value;
        }

        public bool Valid => Min <= Max;

        public FloatRangeConfig(float min, float max)
        {
            _min = min;
            _max = max;
        }

        public float GetRandomValue(float min = 0.0f)
        {
            return Valid ? PartyParrotManager.Instance.Random.NextSingle(Mathf.Max(Min, min), Max) : min;
        }

        public float GetPercentValue(float pct)
        {
            if(!Valid) {
                return 0.0f;
            }

            pct = Math.Clamp(pct, 0.0f, 1.0f);
            return Min + (pct * (Max - Min));
        }
    }
}
