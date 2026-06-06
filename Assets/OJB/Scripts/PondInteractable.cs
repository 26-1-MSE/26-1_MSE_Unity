using UnityEngine;

public class PondInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer pondSprite;

    // 물
    [SerializeField] private int itemTypeId = 5;
    [SerializeField] private int acquireCount = 1;

    private bool hasWater = true;

    public void Interact(PlayerInteraction player)
    {
        if (!hasWater) return;

        hasWater = false;

        Debug.Log("[PondInteractable] 물 획득!");

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestAcquireItem(
                itemTypeId,
                acquireCount,
                () =>
                {
                    Debug.Log("[PondInteractable] 서버 물 획득 저장 성공");
                    AudioManager.SFXInstance?.PlayOneShot(25);
                },
                (error) =>
                {
                    Debug.LogError("[PondInteractable] 서버 물 획득 저장 실패: " + error);
                }
            );
        }

        if (pondSprite != null)
            pondSprite.enabled = false;

        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isScooping", true);
            player.Invoke("EndScoop", 0.5f);
        }
    }

    public string GetInteractMessage()
    {
        if (!hasWater) return null;
        return "E: Collect Water";
    }
}