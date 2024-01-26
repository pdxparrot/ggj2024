using Godot;

using System;
using System.Collections.Generic;
using System.Globalization;

using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public sealed partial class PartyParrotManager : SingletonNode<PartyParrotManager>
    {
        #region Events

        public event EventHandler<EventArgs> PauseEvent;

        #endregion

        // initializes the default cultures to be invariant
        // https://twitter.com/OwenGoss/status/1413576850715156484
        private static void InitCulture()
        {
            GD.Print("[Engine] Setting invariant culture");

            // TODO: is this actually safe?
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }

        public string LogPath => ProjectSettings.GlobalizePath(ProjectSettings.GetSetting("debug/file_logging/log_path").AsString());

        // TODO: this should be network sync'd and server-authoritative
        #region Game State

        [Export]
        private ulong _randomSeed;

        public ulong RandomSeed => _randomSeed;

        #region Godot Feature Helpers

        public bool IsEditor => OS.HasFeature("editor");

        public bool IsHeadless => DisplayServer.GetName() == "headless";

        // TODO: move to network manager
        public bool IsDedicatedServer => OS.HasFeature("dedicated_server");

        public bool IsDebug => OS.HasFeature("debug");

        public bool IsRelease => OS.HasFeature("release");

        #endregion

        public bool IsFullscreen
        {
            get => DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
            set => DisplayServer.WindowSetMode(value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        }

        // TODO: pause management should move to the game manager
        // I don't know why it was ever put into the engine manager

        private bool _isPaused;

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                bool wasPaused = _isPaused;
                _isPaused = value;

                if(wasPaused != _isPaused) {
                    GD.Print($"[Engine] Pause: {_isPaused}");

                    PauseEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        public RandomNumberGenerator Random { get; private set; }

        private readonly Dictionary<string, string> _commandLineArgs = new Dictionary<string, string>();

        public Dictionary<string, string> CommandLineArgs => _commandLineArgs;

        private readonly List<Func<bool>> _quitHandlers = new List<Func<bool>>();

        #region Godot Lifecycle

        public override void _Ready()
        {
            base._Ready();

            string build = IsRelease ? "release" : IsDebug ? "debug" : "unknown";
            GD.Print($"Party Parrot Engine ({build} build) starting up...");
            GD.Print($"Godot version: {Engine.GetVersionInfo()["string"]}");
            GD.Print($".NET version: {System.Environment.Version}");
            GD.Print($"Log path: {LogPath}");

            if(IsEditor) {
                GD.Print("Detected editor run");
            }

            if(IsHeadless) {
                GD.Print("Detected headless run");
            }

            InitCulture();

            GetTree().AutoAcceptQuit = false;

            ParseCommandLineArgs();

            DumpInfo();

            SetRandomSeed(RandomSeed);
        }

        public override void _Notification(int what)
        {
            if(what == NotificationWMCloseRequest) {
                GD.Print("[Engine] Received quit notification, running handlers ...");

                // TODO: what if a handler needs to prompt the user?
                foreach(var handler in _quitHandlers) {
                    if(!handler()) {
                        return;
                    }
                }

                GD.Print("[Engine] Quitting!");
                GetTree().Quit();
            }
        }

        #endregion

        private void ParseCommandLineArgs()
        {
            var commandLineArgs = OS.GetCmdlineArgs();
            GD.Print($"[Engine] Command line args: [{string.Join(", ", commandLineArgs)}]");
            foreach(var arg in commandLineArgs) {
                var parts = arg.Split('=');
                if(parts.Length == 1) {
                    _commandLineArgs[parts[0].TrimStart('-')] = string.Empty;
                } else if(parts.Length == 2) {
                    _commandLineArgs[parts[0].TrimStart('-')] = parts[1];
                } else {
                    GD.PrintErr($"Invalid command line argument: {arg}");
                }
            }
        }

        private void DumpInfo()
        {
            GD.Print($"[Engine] OS: {OS.GetName()}");

            GD.Print($"[Engine] Max FPS: {Engine.MaxFps}");
            GD.Print($"[Engine] Vsync: {DisplayServer.WindowGetVsyncMode()}");

            GD.Print($"[Engine] Physics FPS: {Engine.PhysicsTicksPerSecond}");

            float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
            var gravityVector = (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector");
            GD.Print($"[Engine] Gravity: {gravityVector * gravity}");
        }

        public void RegisterQuitHandler(Func<bool> handler)
        {
            _quitHandlers.Add(handler);
        }

        public void SafeQuit()
        {
            GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
        }

        public void SetRandomSeed(ulong seed)
        {
            Random = new RandomNumberGenerator();

            _randomSeed = seed;
            if(RandomSeed > 0) {
                GD.Print($"[Engine] Seeding RNG {RandomSeed}...");
                Random.Seed = RandomSeed;
            } else {
                GD.Print($"[Engine] Randomizing...");
                Random.Randomize();
            }
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
        }
    }
}
