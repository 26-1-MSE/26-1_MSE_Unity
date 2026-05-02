using UnityEngine;

public class TreeInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName = "Apple";
    [SerializeField] private int maxItemCount = 3;
    [SerializeField] private Animator treeAnimator;
    [SerializeField] private GameObject[] foodImages;

    private int currentItemCount;

    private void Start()
    {
        currentItemCount = maxItemCount;
    }

    public void Interact(PlayerInteraction player)
    {
        if (currentItemCount <= 0) return;

        currentItemCount--;
        Debug.Log($"{itemName} 획득! 남은 개수: {currentItemCount}");
        
        if (currentItemCount < foodImages.Length)
            foodImages[currentItemCount].SetActive(false);

        // 나무 애니메이션
        if (treeAnimator != null)
            treeAnimator.SetTrigger("chop");

        // 플레이어 Chop 애니메이션
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isChopping", true);
            player.Invoke("EndChop", 0.5f);
        }

        if (currentItemCount <= 0)
            Debug.Log("나무 아이템 소진");
    }

    public string GetInteractMessage()
    {
        if (currentItemCount <= 0) return null;
        return "상호작용하기";
    }
}