using UnityEngine;

public class PetInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private int petTypeId;
    private bool hasInteracted = false;
    public void Interact(PlayerInteraction player)
    {
        if (hasInteracted) return; // 추가: 이미 상호작용했으면 무시
        hasInteracted = true;
        // 미니게임 시작
        OcarinaGameManager.Instance.StartGame(gameObject, petTypeId);
    }

    public string GetInteractMessage()
    {
        if (hasInteracted) return null;
        return "E: Tame Pet";
    }
    public void ResetInteraction()
    {
        hasInteracted = false;
    }
}