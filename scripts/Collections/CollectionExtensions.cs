using Godot;

using System.Collections.Generic;
using System.Linq;

using pdxpartyparrot.ggj2024.Managers;
using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Collections
{
    public static class CollectionExtensions
    {
        public static T GetRandomEntry<T>(this IReadOnlyCollection<T> collection)
        {
            return PartyParrotManager.Instance.Random.GetRandomEntry(collection);
        }

        public static T RemoveRandomEntry<T>(this IList<T> collection)
        {
            return PartyParrotManager.Instance.Random.RemoveRandomEntry(collection);
        }

        public static T PeakFront<T>(this IList<T> list)
        {
            return list.Count < 1 ? default : list[0];
        }

        public static T RemoveFront<T>(this IList<T> list)
        {
            if(list.Count < 1) {
                return default;
            }

            T element = list[0];
            list.RemoveAt(0);
            return element;
        }

        public static T Nearest<T>(this IReadOnlyCollection<T> collection, Vector3 position, out float distanceSquared) where T : Node3D
        {
            int bestIdx = -1;
            distanceSquared = float.PositiveInfinity;

            for(int i = 0; i < collection.Count; ++i) {
                T element = collection.ElementAt(i);
                Vector3 epos = element.GlobalPosition;

                float dist = epos.DistanceSquaredTo(position);
                if(dist < distanceSquared) {
                    distanceSquared = dist;
                    bestIdx = i;
                }
            }

            return bestIdx < 0 ? null : collection.ElementAt(bestIdx);
        }

        public static T NearestManhattan<T>(this IReadOnlyCollection<T> collection, Vector3 position, out float distance) where T : Node3D
        {
            int bestIdx = -1;
            distance = float.PositiveInfinity;

            for(int i = 0; i < collection.Count; ++i) {
                T element = collection.ElementAt(i);
                Vector3 epos = element.GlobalPosition;

                float dist = epos.ManhattanDistanceTo(position);
                if(dist < distance) {
                    distance = dist;
                    bestIdx = i;
                }
            }

            return bestIdx < 0 ? null : collection.ElementAt(bestIdx);
        }

        public static T Furthest<T>(this IReadOnlyCollection<T> collection, Vector3 position, out float distanceSquared) where T : Node3D
        {
            int bestIdx = -1;
            distanceSquared = float.NegativeInfinity;

            for(int i = 0; i < collection.Count; ++i) {
                T element = collection.ElementAt(i);
                Vector3 epos = element.GlobalPosition;

                float dist = epos.DistanceSquaredTo(position);
                if(dist > distanceSquared) {
                    distanceSquared = dist;
                    bestIdx = i;
                }
            }

            return bestIdx < 0 ? null : collection.ElementAt(bestIdx);
        }

        public static T FurthestManhattan<T>(this IReadOnlyCollection<T> collection, Vector3 position, out float distance) where T : Node3D
        {
            int bestIdx = -1;
            distance = float.NegativeInfinity;

            for(int i = 0; i < collection.Count; ++i) {
                T element = collection.ElementAt(i);
                Vector3 epos = element.GlobalPosition;

                float dist = epos.ManhattanDistanceTo(position);
                if(dist > distance) {
                    distance = dist;
                    bestIdx = i;
                }
            }

            return bestIdx < 0 ? null : collection.ElementAt(bestIdx);
        }

        public static void WithinDistance<T>(this IReadOnlyCollection<T> collection, Vector3 position, float distance, IList<T> matches) where T : Node3D
        {
            foreach(T element in collection) {
                Vector3 epos = element.GlobalPosition;

                float dist = epos.ManhattanDistanceTo(position);
                if(dist <= distance) {
                    matches.Add(element);
                }
            }
        }
    }
}
