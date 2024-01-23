using Godot;

using System;

namespace pdxpartyparrot.ggj2024.Util
{
    public static class Vector3Extensions
    {
        public static float ManhattanDistanceTo(this Vector3 v, Vector3 position)
        {
            return Math.Abs(v.X - position.X) + Math.Abs(v.Y - position.Y) + Math.Abs(v.Z - position.Z);
        }

        public static Vector3 Perpendicular(this Vector3 v)
        {
            return new Vector3(-v.Z, v.Y, v.X);
        }
    }
}
