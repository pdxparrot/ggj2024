using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

namespace pdxpartyparrot.ggj2024.Util
{
    public static class RandomNumberGeneratorExtensions
    {
        #region Random Collection Entries

        public static T GetRandomEntry<T>(this RandomNumberGenerator random, IReadOnlyCollection<T> collection)
        {
            if(collection.Count < 1) {
                return default;
            }

            int idx = random.Next(collection.Count);
            return collection.ElementAt(idx);
        }

        public static T RemoveRandomEntry<T>(this RandomNumberGenerator random, IList<T> collection)
        {
            if(collection.Count < 1) {
                return default;
            }

            int idx = random.Next(collection.Count);
            T v = collection.ElementAt(idx);
            collection.RemoveAt(idx);
            return v;
        }

        #endregion

        // 0 or 1
        public static int CoinFlip(this RandomNumberGenerator random)
        {
            return random.NextBool() ? 1 : 0;
        }

        // 1 or -1
        public static float NextSign(this RandomNumberGenerator random)
        {
            return random.NextBool() ? 1 : -1;
        }

        // true or false
        public static bool NextBool(this RandomNumberGenerator random)
        {
            return random.Next(2) != 0;
        }

        // [0, int.MaxValue]
        public static int Next(this RandomNumberGenerator random)
        {
            return random.Next(int.MaxValue);
        }

        // [0, maxValue]
        public static int Next(this RandomNumberGenerator random, int maxValue)
        {
            return random.RandiRange(0, Math.Max(maxValue - 1, 0));
        }

        // [0.0, 1.0]
        public static float NextSingle(this RandomNumberGenerator random)
        {
            return random.NextSingle(1.0f);
        }

        // [0.0, maxValue]
        public static float NextSingle(this RandomNumberGenerator random, float maxValue)
        {
            return random.NextSingle(0.0f, Math.Max(maxValue - float.Epsilon, 0.0f));
        }

        // [minValue, maxValue]
        public static float NextSingle(this RandomNumberGenerator random, float minValue, float maxValue)
        {
            return (float)random.RandfRange(minValue, maxValue);
        }
    }
}
