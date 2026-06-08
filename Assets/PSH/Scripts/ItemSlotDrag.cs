using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class ItemSlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Sprite foodSprite;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject foodImage; // BG 안의 음식 스프라이트 오브젝트
    [SerializeField] private PetGrowthManager petGrowthManager;

    private List<int> itemIds = new List<int>();
    private int itemTypeId;
    private int count;
    private GameObject preview;

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
            Debug.LogWarning("[ItemSlotDrag] 아이템 사용 처리 중이라 드래그 막음");
            return;
        }
        // 미리보기 오브젝트 생성
        preview = new GameObject("FoodPreview");
        SpriteRenderer sr = preview.AddComponent<SpriteRenderer>();
        sr.sprite = foodSprite;
        sr.sortingOrder = 20;
        // 크기 조정
        preview.transform.localScale = new Vector3(3f, 3f, 1f); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (preview == null) return;
        Vector3 worldPos = GetWorldPosition(eventData);
        preview.transform.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (preview == null)
            return;

        Vector3 worldPos = GetWorldPosition(eventData);

        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit == null)
        {
            Debug.LogWarning("[ItemSlotDrag] 드롭 위치에 Collider2D 없음");
        }
        else
        {
            Debug.Log($"[ItemSlotDrag] 드롭 위치 Collider 감지 / name:{hit.gameObject.name}, tag:{hit.gameObject.tag}");
        }

        bool isOnPet = hit != null &&
               (hit.CompareTag("Pet") || hit.CompareTag("PetDropArea"));

        if (isOnPet)
        {
            Debug.Log($"[ItemSlotDrag] 아이템 드롭 성공 / itemTypeId:{itemTypeId}");

            if (petGrowthManager == null)
            {
                Debug.LogError("[ItemSlotDrag] petGrowthManager 연결 안 됨");
                Destroy(preview);
                preview = null;
                return;
            }

            int petId = petGrowthManager.GetCurrentPetId();

            Debug.Log($"[ItemSlotDrag] 사용 요청 / petId:{petId}, itemTypeId:{itemTypeId}, count:{count}");

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

        // 성공/실패/바깥 드롭 상관없이 드래그 미리보기는 무조건 제거
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