using UnityEngine;

public class PetInteractable : MonoBehaviour, IInteractable
{
    public void Interact(PlayerInteraction player)
    {
        // 미니게임 시작
        OcarinaGameManager.Instance.StartGame();
    }

    public string GetInteractMessage()
    {
        return "상호작용하기";
    }
}