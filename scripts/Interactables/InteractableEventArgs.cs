

using System;

namespace pdxpartyparrot.ggj2024.Interactables
{
    public class InteractableEventArgs : EventArgs
    {
        public IInteractable Interactable { get; set; }
    }
}
