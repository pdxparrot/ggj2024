using Godot;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using pdxpartyparrot.ggj2024.Collections;

namespace pdxpartyparrot.ggj2024.Interactables
{
    public partial class Interactables : Area3D, IEnumerable<IInteractable>
    {
        #region Events

        public event EventHandler<InteractableEventArgs> AddingInteractableEvent;

        public event EventHandler<InteractableEventArgs> InteractableAddedEvent;

        public event EventHandler<InteractableEventArgs> RemovingInteractableEvent;

        public event EventHandler<InteractableEventArgs> InteractableRemovedEvent;

        #endregion

        private readonly Dictionary<Type, HashSet<IInteractable>> _interactables = new Dictionary<Type, HashSet<IInteractable>>();

        // NOTE: this doesn't check to see if the interactable actually collides with us
        public bool AddInteractable(IInteractable interactable)
        {
            if(null == interactable || !interactable.CanInteract) {
                return false;
            }

            //GD.Print($"Adding interactable of type {interactable.InteractableType}");
            AddingInteractableEvent?.Invoke(this, new InteractableEventArgs {
                Interactable = interactable
            });

            var interactables = _interactables.GetOrAdd(interactable.InteractableType);
            return interactables.Add(interactable);
        }

        public bool RemoveInteractable(IInteractable interactable)
        {
            if(null == interactable) {
                return false;
            }

            //GD.Print($"Removing interactable of type {interactable.InteractableType}");
            RemovingInteractableEvent?.Invoke(this, new InteractableEventArgs {
                Interactable = interactable
            });

            var interactables = _interactables.GetOrAdd(interactable.InteractableType);
            return interactables.Remove(interactable);
        }

        public IReadOnlyCollection<IInteractable> GetInteractables<T>() where T : IInteractable
        {
            return _interactables.GetOrAdd(typeof(T));
        }

        public T GetRandomInteractable<T>() where T : class, IInteractable
        {
            return GetInteractables<T>().GetRandomEntry() as T;
        }

        public T GetFirstInteractable<T>() where T : class, IInteractable
        {
            if(!HasInteractables<T>()) {
                return null;
            }
            return GetInteractables<T>().ElementAt(0) as T;
        }

        public void GetInteractables<T>(ICollection<T> interactables) where T : class, IInteractable
        {
            IReadOnlyCollection<IInteractable> n = GetInteractables<T>();
            foreach(IInteractable interactable in n) {
                if(interactable is T scratch) {
                    interactables.Add(scratch);
                }
            }
        }

        public IEnumerator<IInteractable> GetEnumerator()
        {
            foreach(var kvp in _interactables) {
                foreach(IInteractable interactable in kvp.Value) {
                    yield return interactable;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool HasInteractables<T>() where T : IInteractable
        {
            var interactables = _interactables.GetValueOrDefault(typeof(T));
            return null != interactables && interactables.Count > 0;
        }

        public bool HasInteractable<T>(T interactable) where T : IInteractable
        {
            var interactables = _interactables.GetValueOrDefault(typeof(T));
            if(null == interactables) {
                return false;
            }
            return interactables.Contains(interactable);
        }

        protected void AddInteractable(Area3D other)
        {
            if(!(other.Owner is IInteractable interactable)) {
                return;
            }

            if(AddInteractable(interactable)) {
                InteractableAddedEvent?.Invoke(this, new InteractableEventArgs {
                    Interactable = interactable
                });
            }
        }

        protected void RemoveInteractable(Area3D other)
        {
            if(!(other.Owner is IInteractable interactable)) {
                return;
            }

            if(RemoveInteractable(interactable)) {
                InteractableRemovedEvent?.Invoke(this, new InteractableEventArgs {
                    Interactable = interactable
                });
            }
        }

        #region Signal Handlers

        private void _on_area_entered(Area3D other)
        {
            AddInteractable(other);
        }

        private void _on_area_exited(Area3D other)
        {
            RemoveInteractable(other);
        }

        #endregion
    }
}
