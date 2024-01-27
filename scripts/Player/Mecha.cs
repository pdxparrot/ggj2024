using Godot;

namespace pdxpartyparrot.ggj2024.Player
{
    public partial class Mecha : SimplePlayer
    {
        public enum Leg
        {
            None,
            Left,
            Right,
        }

        private Leg _lastLeg;

        private bool _thrustersEnabled;

        public void MoveLeftLeg()
        {
            RpcId(1, nameof(RpcLeftLeg));
        }

        public void MoveRightLeg()
        {
            RpcId(1, nameof(RpcRightLeg));
        }

        public void MoveBothLegs()
        {
            RpcId(1, nameof(RpcBothLegs));
        }

        public void MoveLeftArm()
        {
            RpcId(1, nameof(RpcLeftArm));
        }

        public void MoveRightArm()
        {
            RpcId(1, nameof(RpcRightArm));
        }

        public void ThrustersOn()
        {
            RpcId(1, nameof(RpcThrusters), true);
        }

        public void ThrustersOff()
        {
            RpcId(1, nameof(RpcThrusters), false);
        }

        #region RPCs

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcLeftLeg()
        {
            if(_lastLeg == Leg.Left) {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] falls over (left)!");
                // TODO: fall over
                _lastLeg = Leg.None;
                return;
            }

            GD.Print($"[Player {ClientId}:{Input.DeviceId}] moves forward (left)!");
            // TODO: move forward
            _lastLeg = Leg.Left;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcRightLeg()
        {
            if(_lastLeg == Leg.Right) {
                GD.Print($"[Player {ClientId}:{Input.DeviceId}] falls over (right)!");
                // TODO: fall over
                _lastLeg = Leg.None;
                return;
            }

            GD.Print($"[Player {ClientId}:{Input.DeviceId}] moves forward (right)!");
            // TODO: move forward
            _lastLeg = Leg.Right;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcBothLegs()
        {
            GD.Print($"[Player {ClientId}:{Input.DeviceId}] falls over (both)!");
            // TODO: fall over
            _lastLeg = Leg.None;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcLeftArm()
        {
            GD.Print($"[Player {ClientId}:{Input.DeviceId}] punches left!");
            // TODO: punch left
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcRightArm()
        {
            GD.Print($"[Player {ClientId}:{Input.DeviceId}] punches right!");
            // TODO: punch right
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RpcThrusters(bool enable)
        {
            GD.Print($"[Player {ClientId}:{Input.DeviceId}] thrusters: {enable}!");
            _thrustersEnabled = enable;
        }

        #endregion
    }
}
