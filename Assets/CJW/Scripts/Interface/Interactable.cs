/// Interface for objects that can interact with the player.
public interface IInteractable
{
    void Interact(PlayerInteraction player);
    string GetInteractMessage();
}