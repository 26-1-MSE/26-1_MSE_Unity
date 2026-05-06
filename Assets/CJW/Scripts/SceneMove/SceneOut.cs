using UnityEngine;

public class SceneOut : MonoBehaviour
{
    public enum TargetScene
    {
        Lobby,
        PetTown,
        PetRoom,
        Island
    }

    [Header("TargetScene")]
    [SerializeField] private TargetScene targetScene;

    public void OnClickMove()
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