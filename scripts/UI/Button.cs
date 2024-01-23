using Godot;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class Button : BaseButton
    {
        [Export]
        private AudioStreamPlayer _hoverAudioStreamPlayer;

        [Export]
        private AudioStreamPlayer _pressedAudioStreamPlayer;

        #region Signal Handlers

        private void _on_mouse_entered()
        {
            _hoverAudioStreamPlayer.Play();
        }

        private void _on_mouse_exited()
        {
            _hoverAudioStreamPlayer.Play();
        }

        private void _on_pressed()
        {
            _pressedAudioStreamPlayer.Play();
        }

        #endregion
    }
}
