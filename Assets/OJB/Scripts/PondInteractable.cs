using UnityEngine;

public class PondInteractable : MonoBehaviour, IInteractable
{
    private bool hasWater = true; // 물 있는지 여부

    public void Interact(PlayerInteraction player)
    {
        if (!hasWater) return; // 이미 물 뜬 경우 무시

        hasWater = false;
        Debug.Log("물 획득!");

        // 플레이어 물뜨기 애니메이션
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isScooping", true);
            player.Invoke("EndScoop", 0.5f);
        }
    }

    public string GetInteractMessage()
    {
        if (!hasWater) return null; // 물 없으면 텍스트 숨김
        return "상호작용하기";
    }
}