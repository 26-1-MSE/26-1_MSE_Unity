using UnityEngine;

 //Defines the contract for all interactable objects in the game.
public interface InteractableItem
{
    void Interact(PlayerInteraction player);
    string GetInteractMessage();
}