using UnityEngine;

public class PondInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer pondSprite;
    private bool hasWater = true;

    public void Interact(PlayerInteraction player)
    {
        if (!hasWater) return;

        hasWater = false;
        Debug.Log("[ITEM_COLLECT] 물 획득!");

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