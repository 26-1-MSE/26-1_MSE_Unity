using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;


/// <summary>
/// Handles player interactions with trees, pets, and other objects
/// that implement the IInteractable interface.
/// Displays interaction prompts and executes interactions using the E key.
/// </summary>

public class PlayerInteraction : MonoBehaviour
{
    // Currently detected interactable object within interaction range.
    private IInteractable currentInteractable;

    [SerializeField] private TextMeshProUGUI interactText; 
    [SerializeField] private Camera mainCamera;


    /// <summary>
    /// Checks for interaction input.
    /// Executes the current interactable object when the player presses E.
    /// Interaction is disabled while the ocarina mini-game is active.
    /// </summary>

    private void Update()
    {

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentInteractable != null &&
                (OcarinaGameManager.Instance == null || !OcarinaGameManager.Instance.IsPlaying))
            {
                currentInteractable.Interact(this);
            }
        }
    }

    /// <summary>
    /// Called when the player enters the interaction range of an object.
    /// Registers the interactable object and displays its interaction message.
    /// </summary>

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            string message = interactable.GetInteractMessage();
            if (interactText != null && message != null)
            {
                interactText.text = message; 
                interactText.gameObject.SetActive(true);
            }
            Debug.Log($"[PlayerInteraction] In Interaction Range : {other.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && currentInteractable == interactable)
        {
            currentInteractable = null;
            if (interactText != null)
                interactText.gameObject.SetActive(false);
            Debug.Log($"[PlayerInteraction] Out of Interaction Range : {other.name}");
        }
    }

    // Ends the chopping animation triggered by resource collection.
    public void EndChop()
    {
        GetComponent<Animator>().SetBool("isChopping", false);
    }

    // Ends the scooping animation triggered by water collection.
    public void EndScoop()
    {
        GetComponent<Animator>().SetBool("isScooping", false);
    }
}