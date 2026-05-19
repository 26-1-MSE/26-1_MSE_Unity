using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Sprite foodSprite;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject foodImage; // BG 안의 음식 스프라이트 오브젝트


    private int count = 3; // 임시 초기값
    private GameObject preview;

    private void Start()
    {
        countText.text = count.ToString();

        if (count <= 0)
        {
            foodImage.SetActive(false);
            countText.gameObject.SetActive(false);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (count <= 0) return;
        // 미리보기 오브젝트 생성
        preview = new GameObject("FoodPreview");
        SpriteRenderer sr = preview.AddComponent<SpriteRenderer>();
        sr.sprite = foodSprite;
        sr.sortingOrder = 10;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (preview == null) return;
        Vector3 worldPos = GetWorldPosition(eventData);
        preview.transform.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (preview == null) return;
        Vector3 worldPos = GetWorldPosition(eventData);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null && hit.CompareTag("PetDropArea"))
        {
            count--;
            countText.text = count.ToString();
            if (count <= 0)
            {
                foodImage.SetActive(false);
                countText.gameObject.SetActive(false);
            }
            Destroy(preview);
        }
        else
        {
            Destroy(preview);
        }
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