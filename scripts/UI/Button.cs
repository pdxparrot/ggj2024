using Godot;

namespace pdxpartyparrot.ggj2024.UI
{
    public partial class Button : TextureButton
    {
        // TODO: not sure what signal to use here
        [Export]
        private AudioStreamPlayer _hoverAudioStreamPlayer;

        [Export]
        private AudioStreamPlayer _pressedAudioStreamPlayer;

        #region Signal Handlers

        private void _on_Button_pressed()
        {
            _pressedAudioStreamPlayer.Play();
        }

        #endregion
    }
}
