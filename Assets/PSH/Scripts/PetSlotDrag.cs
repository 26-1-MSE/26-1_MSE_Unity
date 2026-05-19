using UnityEngine;
using UnityEngine.EventSystems;

public class PetSlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject petPrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PetRoomInventoryManager inventoryManager;

    private GameObject previewPet;

    public void OnBeginDrag(PointerEventData eventData)
    {
        previewPet = Instantiate(petPrefab);
        previewPet.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (previewPet == null) return;

        Vector3 worldPos = GetWorldPosition(eventData);
        previewPet.SetActive(true);
        previewPet.transform.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (previewPet == null) return;

        Vector3 worldPos = GetWorldPosition(eventData);

        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null && hit.CompareTag("PetDropArea"))
        {
            previewPet.transform.position = worldPos;
            inventoryManager.OnPetPlaced();
        }
        else
        {
            Destroy(previewPet);
        }

        previewPet = null;
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