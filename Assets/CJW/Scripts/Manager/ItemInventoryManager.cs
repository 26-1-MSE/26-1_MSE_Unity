using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInventoryManager : MonoBehaviour
{
    [Header("Item Slot Images")]
    [SerializeField] private Image[] itemImages = new Image[12];

    [Header("Item Count Texts")]
    [SerializeField] private TMP_Text[] countTexts = new TMP_Text[12];

    [Header("Item Sprites")]
    [SerializeField] private Sprite pumpkinSprite;
    [SerializeField] private Sprite bananaSprite;
    [SerializeField] private Sprite appleSprite;
    [SerializeField] private Sprite carrotSprite;
    [SerializeField] private Sprite waterSprite;

    private void Start()
    {
        RefreshItemInventory();
    }

    public void RefreshItemInventory()
    {
        Debug.Log("[ItemInventoryManager] RefreshItemInventory 호출됨");

        if (DataManager.Data == null)
        {
            Debug.LogWarning("[ItemInventoryManager] DataManager.Data 없음");
            return;
        }

        DataManager.OwnedItemSlot[] items = DataManager.Data.OwnedItemSlots;

        Debug.Log($"[ItemInventoryManager] OwnedItemSlots 길이: {items.Length}");

        for (int i = 0; i < itemImages.Length; i++)
        {
            if (itemImages[i] == null)
            {
                Debug.LogWarning($"[ItemInventoryManager] itemImages[{i}] 비어 있음");
                continue;
            }

            if (countTexts[i] == null)
            {
                Debug.LogWarning($"[ItemInventoryManager] countTexts[{i}] 비어 있음");
                continue;
            }

            if (i >= items.Length || items[i].itemTypeId == 0 || items[i].count <= 0)
            {
                itemImages[i].sprite = null;
                itemImages[i].enabled = false;
                itemImages[i].gameObject.SetActive(false);

                countTexts[i].text = "";
                countTexts[i].gameObject.SetActive(false);

                continue;
            }

            Sprite itemSprite = GetItemSprite(items[i].itemTypeId);

            Debug.Log($"[ItemInventoryManager] UI Slot {i} / typeId:{items[i].itemTypeId}, count:{items[i].count}, sprite:{itemSprite}");

            itemImages[i].gameObject.SetActive(true);
            itemImages[i].sprite = itemSprite;
            itemImages[i].enabled = true;

            countTexts[i].gameObject.SetActive(true);
            countTexts[i].text = items[i].count.ToString();

            ItemSlotDrag drag = itemImages[i].GetComponentInParent<ItemSlotDrag>();
            if (drag != null)
            {
                drag.SetItemData(items[i].itemTypeId, items[i].count, itemSprite);
            }
        }
    }

    private Sprite GetItemSprite(int itemTypeId)
    {
        switch (itemTypeId)
        {
            case 1: return pumpkinSprite;
            case 2: return bananaSprite;
            case 3: return appleSprite;
            case 4: return carrotSprite;
            case 5: return waterSprite;
            default: return null;
        }
    }
}