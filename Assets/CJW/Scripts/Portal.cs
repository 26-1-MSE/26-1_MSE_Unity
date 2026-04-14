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

    [Header("�̵��� ��")]
    [SerializeField] private TargetScene targetScene;

    public void Interact(PlayerInteraction player)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance�� �����ϴ�.");
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
    public string GetInteractMessage()
    {
        return null;
    }
    
}