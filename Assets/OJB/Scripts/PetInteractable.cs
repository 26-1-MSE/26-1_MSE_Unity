using UnityEngine;


/// Interactable component attached to wild pet objects that triggers the rhythm mini-game when interacted with.
public class PetInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private int petTypeId; // Server-side pet type identifier
    private bool hasInteracted = false;// Prevents re-triggering after interaction

    public void Interact(PlayerInteraction player)
    {
        if (hasInteracted) return;
        hasInteracted = true;
        OcarinaGameManager.Instance.StartGame(gameObject, petTypeId);
    }

    public string GetInteractMessage()
    {
        if (hasInteracted) return null;
        return "E: Tame Pet";
    }

    // Resets the interaction state to allow retry after a failed mini-game
    public void ResetInteraction()
    {
        hasInteracted = false;
    }
}