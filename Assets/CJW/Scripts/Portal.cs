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
        return null; // 포탈은 상호작용 텍스트 필요없으니 null 반환
    }
    
}