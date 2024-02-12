using Godot;

namespace pdxpartyparrot.ggj2024.Camera
{
    // this goes on the viewer's pivot container
    // not the camera itself
    public partial class PhantomCamera : Viewer
    {
        [Export]
        private Node3D _phantomCamera;

        public void Follow(Node3D target)
        {
            GD.Print("[PhantomCamera] Set follow target");
            _phantomCamera.Call("set_follow_target_node", target);
        }

        public void AddToFollowGroup(Node3D target)
        {
            GD.Print("[PhantomCamera] Add to follow group");
            _phantomCamera.Call("append_follow_group_node", target);
        }

        public void RemoveFromFollowGroup(Node3D target)
        {
            GD.Print("[PhantomCamera] Remove from follow group ");
            _phantomCamera.Call("erase_follow_group_node", target);
        }
    }
}
