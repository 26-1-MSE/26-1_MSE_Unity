using UnityEngine;

public interface InteractableItem
{
    void Interact(PlayerInteraction player);
    string GetInteractMessage();
}
