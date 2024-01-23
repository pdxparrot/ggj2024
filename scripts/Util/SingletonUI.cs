using Godot;

namespace pdxpartyparrot.ggj2024.Util
{
    public partial class SingletonUI<T> : Control where T : SingletonUI<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if(ExitedTree) {
                    GD.PushError($"[SingletonUI] Instance '{typeof(T)}' already exited SceneTree!");
                    return null;
                }
                return instance;
            }
        }

        public static bool HasInstance => null != instance && !ExitedTree;

        private static bool ExitedTree;

        #region Godot Lifecycle

        public override void _Ready()
        {
            if(HasInstance) {
                GD.PushError($"[SingletonUI] Instance '{typeof(T)}' already exists!");
                return;
            }

            instance = (T)this;
        }

        public override void _ExitTree()
        {
            ExitedTree = true;
        }

        #endregion
    }
}
