using Godot;

using System;

namespace pdxpartyparrot.ggj2024
{
    public partial class Model : Node
    {
        [Export]
        private string _motionBlendPath;

        [Export]
        private AnimationTree _animationTree;

        private AnimationNodeStateMachinePlayback _animationStateMachine;

        #region Godot Lifecycle

        public override void _Ready()
        {
            _animationStateMachine = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/playback");
        }

        #endregion

        public void UpdateMotionBlend(float amount)
        {
            _animationTree.Set(_motionBlendPath, Math.Abs(amount));
        }

        public void TriggerOneShot(string property)
        {
            _animationTree.Set(property, true);
        }

        public void ChangeState(string toNode)
        {
            if(_animationStateMachine != null) {
                _animationStateMachine.Travel(toNode);
            }
        }
    }
}
