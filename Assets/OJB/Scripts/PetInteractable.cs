using UnityEngine;

public class PetInteractable : MonoBehaviour, IInteractable
{

    private bool hasInteracted = false;
    public void Interact(PlayerInteraction player)
    {
        if (hasInteracted) return; // 추가: 이미 상호작용했으면 무시
        hasInteracted = true;
        // 미니게임 시작
        OcarinaGameManager.Instance.StartGame(gameObject);
    }

    public string GetInteractMessage()
    {
        if (hasInteracted) return null;
        return "상호작용하기";
    }
    public void ResetInteraction()
    {
        hasInteracted = false;
    }
}