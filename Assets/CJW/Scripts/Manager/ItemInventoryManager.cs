using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays owned item data from DataManager in the item inventory UI.
/// Also connects each item slot to ItemSlotDrag for drag-and-drop item usage.
/// </summary>

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

    /// <summary>
    /// Refreshes the item inventory UI using the latest item data from DataManager.
    /// Items with the same itemTypeId are grouped and displayed as one slot with a count.
    /// </summary>
    public void RefreshItemInventory()
    {
        Debug.Log("[ItemInventoryManager] RefreshItemInventory 호출됨");

        if (DataManager.Data == null)
        {
            Debug.LogWarning("[ItemInventoryManager] DataManager.Data 없음");
            return;
        }

        DataManager.OwnedItemSlot[] items = DataManager.Data.OwnedItemSlots;

        if (items == null)
        {
            Debug.LogWarning("[ItemInventoryManager] OwnedItemSlots 없음");
            ClearAllSlots();
            return;
        }

        Debug.Log($"[ItemInventoryManager] OwnedItemSlots 길이: {items.Length}");

        Dictionary<int, List<int>> itemIdMap = new Dictionary<int, List<int>>();

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemId == 0 || items[i].itemTypeId == 0)
                continue;

            if (!itemIdMap.ContainsKey(items[i].itemTypeId))
                itemIdMap[items[i].itemTypeId] = new List<int>();

            for (int countIndex = 0; countIndex < items[i].count; countIndex++)
            {
                itemIdMap[items[i].itemTypeId].Add(items[i].itemId);
            }
        }

        Debug.Log($"[ItemInventoryManager] itemIdMap 개수: {itemIdMap.Count}");

        foreach (var pair in itemIdMap)
        {
            Debug.Log($"[ItemInventoryManager] 묶음 확인 / typeId:{pair.Key}, itemIds:{string.Join(",", pair.Value)}");
        }

        int slotIndex = 0;

        foreach (var pair in itemIdMap)
        {
            if (slotIndex >= itemImages.Length)
                break;

            if (itemImages[slotIndex] == null || countTexts[slotIndex] == null)
            {
                slotIndex++;
                continue;
            }

            int itemTypeId = pair.Key;
            List<int> ids = new List<int>(pair.Value);
            Sprite itemSprite = GetItemSprite(itemTypeId);

            Debug.Log($"[ItemInventoryManager] UI 세팅 / slot:{slotIndex}, typeId:{itemTypeId}, count:{ids.Count}, sprite:{itemSprite}");

            bool hasItem = ids.Count > 0 && itemSprite != null;

            itemImages[slotIndex].gameObject.SetActive(hasItem);
            itemImages[slotIndex].sprite = itemSprite;
            itemImages[slotIndex].enabled = hasItem;

            countTexts[slotIndex].gameObject.SetActive(hasItem);
            countTexts[slotIndex].text = hasItem ? ids.Count.ToString() : "";

            ItemSlotDrag drag = itemImages[slotIndex].GetComponentInParent<ItemSlotDrag>();
            if (drag != null)
            {
                drag.SetItemData(ids, itemTypeId, ids.Count, itemSprite);
            }

            slotIndex++;
        }

        for (int i = slotIndex; i < itemImages.Length; i++)
        {
            if (itemImages[i] != null)
            {
                itemImages[i].sprite = null;
                itemImages[i].enabled = false;
                itemImages[i].gameObject.SetActive(false);
            }

            if (countTexts[i] != null)
            {
                countTexts[i].text = "";
                countTexts[i].gameObject.SetActive(false);
            }
        }
    }

    private void ClearAllSlots()
    {
        for (int i = 0; i < itemImages.Length; i++)
        {
            if (itemImages[i] != null)
            {
                itemImages[i].sprite = null;
                itemImages[i].enabled = false;
                itemImages[i].gameObject.SetActive(false);
            }

            if (countTexts[i] != null)
            {
                countTexts[i].text = "";
                countTexts[i].gameObject.SetActive(false);
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