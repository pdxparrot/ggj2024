using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    public abstract partial class SimplePlayer : SimpleCharacter
    {
        [Export]
        private long _clientId;

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
