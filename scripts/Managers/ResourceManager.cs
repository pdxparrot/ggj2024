using Godot;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using pdxpartyparrot.ggj2024.Util;

namespace pdxpartyparrot.ggj2024.Managers
{
    public partial class ResourceManager : SingletonNode<ResourceManager>
    {
        public class ProgressEventArgs : EventArgs
        {
            public float Progress { get; set; }
        }

        public class SuccessEventArgs : EventArgs
        {
            public Resource Resource { get; set; }
        }

        public class FailureEventArgs : EventArgs
        {
            public Error Error { get; set; }
        }

        private class Notifier
        {
            #region Events

            public event EventHandler<ProgressEventArgs> ProgressEvent;

            public event EventHandler<SuccessEventArgs> SuccessEvent;

            public event EventHandler<FailureEventArgs> FailureEvent;

            #endregion

            public void Progress(ResourceManager sender, float progress)
            {
                ProgressEvent?.Invoke(sender, new ProgressEventArgs {
                    Progress = progress,
                });
            }

            public void Success(ResourceManager sender, Resource resource)
            {
                SuccessEvent?.Invoke(sender, new SuccessEventArgs {
                    Resource = resource,
                });
            }

            public void Failure(ResourceManager sender, Error err)
            {
                FailureEvent?.Invoke(sender, new FailureEventArgs {
                    Error = err,
                });
            }
        }

        [Export]
        private bool _verbose = false;

        private readonly Dictionary<string, Notifier> _loadingSet = new Dictionary<string, Notifier>();

        public async Task LoadResourceAsync(string path, EventHandler<SuccessEventArgs> onSuccess = null, EventHandler<FailureEventArgs> onFailure = null, EventHandler<ProgressEventArgs> onProgress = null)
        {
            // if the resource is already loaded, just return it
            if(ResourceLoader.HasCached(path)) {
                if(_verbose) {
                    GD.Print($"[ResourceManager] returning cached resource '{path}'");
                }

                onSuccess?.Invoke(this, new SuccessEventArgs {
                    Resource = ResourceLoader.Load(path),
                });
                return;
            }

            // if we're already loading this resource
            // just subscribe for notifications
            if(_loadingSet.TryGetValue(path, out Notifier notifier)) {
                if(_verbose) {
                    GD.Print($"[ResourceManager] registering listener for resource '{path}'");
                }

                notifier.SuccessEvent += onSuccess;
                notifier.FailureEvent += onFailure;
                notifier.ProgressEvent += onProgress;
                return;
            }

            GD.Print($"[ResourceManager] Loading resource {path}...");
            var err = ResourceLoader.LoadThreadedRequest(path);
            if(err != Error.Ok) {
                onFailure?.Invoke(this, new FailureEventArgs {
                    Error = err,
                });
                return;
            }

            notifier = new Notifier();
            notifier.SuccessEvent += onSuccess;
            notifier.FailureEvent += onFailure;
            notifier.ProgressEvent += onProgress;
            _loadingSet[path] = notifier;

            try {
                Godot.Collections.Array progress = new Godot.Collections.Array();
                notifier.Progress(this, 0.0f);
                while(true) {
                    switch(ResourceLoader.LoadThreadedGetStatus(path, progress)) {
                    case ResourceLoader.ThreadLoadStatus.InvalidResource:
                        notifier.Failure(this, err);
                        return;
                    case ResourceLoader.ThreadLoadStatus.InProgress:
                        notifier.Progress(this, (float)progress[0]);
                        await ToSignal(GetTree(), "idle_frame");
                        break;
                    case ResourceLoader.ThreadLoadStatus.Failed:
                        notifier.Failure(this, err);
                        return;
                    case ResourceLoader.ThreadLoadStatus.Loaded:
                        notifier.Progress(this, 1.0f);
                        notifier.Success(this, ResourceLoader.LoadThreadedGet(path));
                        return;
                    }
                }
            } finally {
                _loadingSet.Remove(path);
            }
        }
    }
}
