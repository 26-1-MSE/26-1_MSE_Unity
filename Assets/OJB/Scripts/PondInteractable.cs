using UnityEngine;


//Interactable component attached to pond objects that allows the player to collect water once per session.
public class PondInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer pondSprite;

    [SerializeField] private int itemTypeId = 5; // Server-side item type ID for water
    [SerializeField] private int acquireCount = 1; // Amount of water collected per interaction

    private bool hasWater = true; // False after water has been collected

    public void Interact(PlayerInteraction player)
    {
        if (!hasWater) return;

        hasWater = false;

        //Debug.Log("[PondInteractable] 물 획득!");

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestAcquireItem(
                itemTypeId,
                acquireCount,
                () =>
                {
                    //Debug.Log("[PondInteractable] 서버 물 획득 저장 성공");
                    AudioManager.SFXInstance?.PlayOneShot(25);
                },
                (error) =>
                {
                    Debug.LogError("[PondInteractable] 서버 물 획득 저장 실패: " + error);
                }
            );
        }

        //Hide the pond sprite after water is collected
        if (pondSprite != null)
            pondSprite.enabled = false;

        //Play the scooping animation on the player
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