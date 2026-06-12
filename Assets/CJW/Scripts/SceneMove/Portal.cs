using UnityEngine;

/// Interactable portal that moves the player to another scene.
public class Portal : MonoBehaviour, IInteractable
{
    // Available destination scenes.
    public enum TargetScene
    {
        Lobby,
        PetTown,
        PetRoom,
        Island
    }

    [Header("씬 이동")]
    [SerializeField] private TargetScene targetScene;

    // Plays a portal sound effect and moves the player
    public void Interact(PlayerInteraction player)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance가 없어서 씬 이동을 할 수 없습니다.");
            return;
        }

        int sceneIndex = GameManager.Instance?.CurrentSceneIndex ?? -1;

        switch (sceneIndex)
        {
            case 1:
                AudioManager.PlayOneShotAndDestroy(5);
                break;
            case 3:
                AudioManager.PlayOneShotAndDestroy(24);
                break;
        }
        
        switch (targetScene)
        {
            case TargetScene.Lobby:
                GameManager.Instance.GoToLobby();
                break;
            case TargetScene.PetTown:
                GameManager.Instance.GoToPetTown();
                break;
            case TargetScene.PetRoom:
                GameManager.Instance.GoToPetRoom();
                break;
            case TargetScene.Island:
                GameManager.Instance.GoToIsland();
                break;
        }
    }

    // Returns the interaction prompt displayed to the player.
    public string GetInteractMessage()
    {
        return "E: Enter";
    }
    
}