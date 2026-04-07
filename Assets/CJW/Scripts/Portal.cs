using UnityEngine;

public class Portal : MonoBehaviour, IInteractable
{
    public enum TargetScene
    {
        Lobby,
        PetTown,
        PetRoom,
        Island
    }

    [Header("이동할 씬")]
    [SerializeField] private TargetScene targetScene;

    public void Interact(PlayerInteraction player)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance가 없습니다.");
            return;
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
}