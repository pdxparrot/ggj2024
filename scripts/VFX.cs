using Godot;

namespace pdxpartyparrot.ggj2024
{
    public partial class VFX : Node
    {
        [Export]
        private AnimationPlayer _animationPlayer;

        public void Play(string animationName = "")
        {
            _animationPlayer.Play(animationName);
        }

        public void Stop()
        {
            _animationPlayer.Stop();
            _animationPlayer.Seek(0, true);
        }
    }
}
