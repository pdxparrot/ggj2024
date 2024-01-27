using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    public abstract partial class SimplePlayer : SimpleCharacter
    {
        private long _clientId;

        // sync'd client -> server
        [Export]
        public long ClientId
        {
            get => _clientId;
            set
            {
                _clientId = value;
                Input.SetMultiplayerAuthority((int)_clientId);
            }
        }

        [Export]
        private SimplePlayerInput _input;

        public SimplePlayerInput Input => _input;
    }
}
