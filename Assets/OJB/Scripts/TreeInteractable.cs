using System;
using UnityEngine;

public class TreeInteractable : MonoBehaviour, IInteractable
{
    [Header("Item Info")]
    [SerializeField] private string itemName = "Apple";
    /// 1 = 사과, 2 = 바나나, 3 = 치즈, 4 = 딸기, 5= 물
    [SerializeField] private int itemTypeId = 1;
    // 한 번 상호작용 시 획득하는 개수
    [SerializeField] private int acquireCount = 1;

    [Header("Tree Settings")]
    // 나무에서 획득 가능한 최대 아이템 개수
    [SerializeField] private int maxItemCount = 3;
    [SerializeField] private Animator treeAnimator;
    // 나무에 달린 음식 이미지 배열
    // currentItemCount 감소에 따라 순서대로 비활성화됨
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

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestAcquireItem(
                itemTypeId,
                acquireCount,
                () =>
                {
                    Debug.Log($"[TreeInteractable] 서버 아이템 획득 저장 성공: {itemName}");
                },
                (error) =>
                {
                    Debug.LogError($"[TreeInteractable] 서버 아이템 획득 저장 실패: {error}");
                }
            );
        }

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