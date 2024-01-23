using Godot;

using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class UIManager : SingletonNode<UIManager>
    {
        [Export]
        private PackedScene _mainMenuScene;

        public PackedScene MainMenuScene => _mainMenuScene;

        [Export]
        private PackedScene _joinGameScene;

        public PackedScene JoinGameScene => _joinGameScene;

        [Export]
        private PackedScene _joiningGameScene;

        public PackedScene JoiningGameScene => _joiningGameScene;

        // TODO 2024:
    }
}
