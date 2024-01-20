using Godot;

using pdxpartyparrot.ggj2024.Managers;

namespace pdxpartyparrot.ggj2024
{
    public partial class Boot : Node
    {
        [Export]
        private PackedScene _dedicatedScene;

        public override void _Process(double delta)
        {
            if(PartyParrotManager.Instance.CommandLineArgs.ContainsKey("dedicated")) {
                GD.Print("Starting dedicated server ...");

                var scene = _dedicatedScene.Instantiate();
                GetTree().Root.AddChild(scene);
            } else {
                GD.Print("Starting client ...");

                // ...
            }

            QueueFree();
        }
    }
}
