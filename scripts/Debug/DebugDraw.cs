using Godot;

namespace pdxpartyparrot.ggj2024.Debug
{
    public interface IDebugDraw
    {
        void DebugDraw(CanvasItem canvas, Camera3D camera);
    }
}
