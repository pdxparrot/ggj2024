using Godot;

namespace pdxpartyparrot.ggj2024.Util
{
    public static class NodeExtensions
    {
        public static void QueueFreeAllChildren(this Node n)
        {
            foreach(Node child in n.GetChildren()) {
                child.QueueFree();
            }
        }
    }
}
