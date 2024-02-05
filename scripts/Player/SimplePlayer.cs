using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024.Player
{
    public abstract partial class SimplePlayer : SimpleCharacter
    {
        private long _clientId;

        // sync'd server -> client
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

        // sync'd server -> client
        private int _playerSlot;

        // sync'd server -> client
        [Export]
        public int PlayerSlot
        {
            get => _playerSlot;
            set
            {
                _playerSlot = value;

                if(!NetworkManager.Instance.IsServer) {
                    PlayerManager.Instance.PlayerObjects[_playerSlot] = this;
                }
                GameUIManager.Instance.HUD.InitializePlayer(_playerSlot);
            }
        }

        [Export]
        private SimplePlayerInput _input;

        public SimplePlayerInput Input => _input;

        protected virtual void OnPlayerSlotChanged()
        {
        }
    }
}
