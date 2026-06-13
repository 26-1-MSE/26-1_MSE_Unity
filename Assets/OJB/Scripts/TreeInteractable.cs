using System;
using UnityEngine;


//Interactable component attached to tree objects that allows the player to collect food items up to a maximum count.
public class TreeInteractable : MonoBehaviour, IInteractable
{
    [Header("Item Info")]
    [SerializeField] private string itemName = "Apple";
    // 1 = 호박, 2 = 바나나, 3 = 치즈, 4 = 딸기, 5 = 물
    [SerializeField] private int itemTypeId = 1;  //server-side item type ID
    [SerializeField] private int acquireCount = 1; // amount collected per interaction

    [Header("Tree Settings")]
    [SerializeField] private int maxItemCount = 3;  //maximum number of items available on this tree
    [SerializeField] private Animator treeAnimator;
    [SerializeField] private GameObject[] foodImages; //food images disabled one by one as items are collected
    [SerializeField] private GameObject UIManager;

    private int currentItemCount;

    private void Start()
    {
        currentItemCount = maxItemCount;
    }

    public void Interact(PlayerInteraction player)
    {
        if (currentItemCount <= 0) return;

        currentItemCount--;
        // Debug.Log($"{itemName} 획득! 남은 개수: {currentItemCount}");

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestAcquireItem(
                itemTypeId,
                acquireCount,
                () =>
                {
                    //Debug.Log($"[TreeInteractable] 서버 아이템 획득 저장 성공: {itemName}");

                    NetworkManager.Instance.RequestInventoryData(
                        response =>
                        {
                            //Debug.Log("[TreeInteractable] 최신 인벤토리 재조회 성공");

                            if (UIManager == null)
                            {
                                Debug.LogError("[TreeInteractable] uiManager 연결 안 됨");
                                return;
                            }

                            ItemInventoryManager itemUI = UIManager.GetComponent<ItemInventoryManager>();

                            if (itemUI == null)
                            {
                                Debug.LogError("[TreeInteractable] UIManager에 ItemInventoryManager 없음");
                                return;
                            }

                            itemUI.RefreshItemInventory();
                            //Debug.Log("[TreeInteractable] 아이템 인벤토리 UI 갱신 완료");

                            AudioManager.SFXInstance?.PlayOneShot(25);
                        },
                        error =>
                        {
                            Debug.LogError("[TreeInteractable] 인벤토리 재조회 실패: " + error);
                        }
                    );
                }
            );
        }

        // disable the corresponding food image as items are collected
        if (currentItemCount < foodImages.Length)
            foodImages[currentItemCount].SetActive(false);

        // play chop animation on the tree
        if (treeAnimator != null)
            treeAnimator.SetTrigger("chop");

        // play chop animation on the player
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isChopping", true);
            player.Invoke("EndChop", 0.5f);
        }

        //Debug.Log("나무 아이템 소진");
    }

    public string GetInteractMessage()
    {
        if (currentItemCount <= 0) return null;
        return "E: Collect Food";
    }
}