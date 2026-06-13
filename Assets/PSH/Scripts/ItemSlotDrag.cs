using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles dragging food items from inventory onto a pet to use them.
/// </summary>
public class ItemSlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Sprite foodSprite;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject foodImage;// Food sprite object inside the slot background
    [SerializeField] private PetGrowthManager petGrowthManager;

    private List<int> itemIds = new List<int>();
    private int itemTypeId;
    private int count;
    private GameObject preview;

    // Sets the slot's item data and updates the count display
    public void SetItemData(List<int> ids, int typeId, int itemCount, Sprite sprite)
    {
        itemIds = ids;
        itemTypeId = typeId;
        count = itemCount;
        foodSprite = sprite;

        if (countText != null)
            countText.text = count.ToString();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (count <= 0) return;

        if (petGrowthManager != null && petGrowthManager.IsUsingItem())
        {
            Debug.LogWarning("[ItemSlotDrag] Drag blocked, item use already in progress");
            return;
        }
        // Create drag preview object
        preview = new GameObject("FoodPreview");
        SpriteRenderer sr = preview.AddComponent<SpriteRenderer>();
        sr.sprite = foodSprite;
        sr.sortingOrder = 20;
        
        preview.transform.localScale = new Vector3(3f, 3f, 1f); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (preview == null) return;
        Vector3 worldPos = GetWorldPosition(eventData);
        preview.transform.position = worldPos;
    }

    // Checks drop target, applies item to pet if valid, then removes preview
    public void OnEndDrag(PointerEventData eventData)
    {
        if (preview == null)
            return;

        Vector3 worldPos = GetWorldPosition(eventData);

        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit == null)
        {
            Debug.LogWarning("[ItemSlotDrag] No Collider2D found at drop position");
        }
        else
        {
            Debug.Log($"[ItemSlotDrag] Collider detected at drop position / name:{hit.gameObject.name}, tag:{hit.gameObject.tag}");
        }

        bool isOnPet = hit != null &&
               (hit.CompareTag("Pet") || hit.CompareTag("PetDropArea"));

        if (isOnPet)
        {
            Debug.Log($"[ItemSlotDrag] Item drop succeeded / itemTypeId:{itemTypeId}");

            if (petGrowthManager == null)
            {
                Debug.LogError("[ItemSlotDrag] petGrowthManager is not assigned");
                Destroy(preview);
                preview = null;
                return;
            }

            int petId = petGrowthManager.GetCurrentPetId();

            Debug.Log($"[ItemSlotDrag] Use request / petId:{petId}, itemTypeId:{itemTypeId}, count:{count}");

            int useItemId = itemIds[0];

            petGrowthManager.UseItemOnCurrentPet(
                itemTypeId,
                () =>
                {
                    count--;
                    countText.text = count.ToString();

                    if (count <= 0)
                    {
                        foodImage.SetActive(false);
                        countText.gameObject.SetActive(false);
                    }
                }
            );
        }

        // Preview is always destroyed regardless of success, failure, or outside drop
        Destroy(preview);
        preview = null;
    }

    private Vector3 GetWorldPosition(PointerEventData eventData)
    {
        Vector3 screenPos = eventData.position;
        screenPos.z = Mathf.Abs(mainCamera.transform.position.z);
       
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        
        
        return worldPos;
    }
}