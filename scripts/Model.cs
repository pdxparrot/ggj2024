using Godot;

using System;

using JetBrains.Annotations;

using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024
{
    public partial class Model : Node3D
    {
        [Export]
        private string _motionBlendPath;

        [Export]
        [CanBeNull]
        private AnimationTree _animationTree;

        private AnimationNodeStateMachinePlayback _animationStateMachine;

        #region Godot Lifecycle

        public override void _Ready()
        {
            OnModelChanged();
        }

        #endregion

        public void InstantiateModel(PackedScene modelScene)
        {
            this.QueueFreeAllChildren();

            var model = modelScene.Instantiate();
            AddChild(model);

            _animationTree = model.GetNode<AnimationTree>("AnimationTree");
            OnModelChanged();
        }

        public void UpdateMotionBlend(float amount)
        {
            if(_animationTree != null) {
                _animationTree.Set(_motionBlendPath, Math.Abs(amount));
            }
        }

        public void TriggerOneShot(string property)
        {
            if(_animationTree != null) {
                _animationTree.Set(property, true);
            }
        }

        public void ChangeState(string toNode)
        {
            if(_animationStateMachine != null) {
                _animationStateMachine.Travel(toNode);
            }
        }

        private void OnModelChanged()
        {
            if(_animationTree != null) {
                _animationStateMachine = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/playback");
            }
            UpdateMotionBlend(0.0f);
        }
    }
}
