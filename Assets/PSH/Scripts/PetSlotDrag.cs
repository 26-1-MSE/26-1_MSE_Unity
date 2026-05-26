using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PetSlotDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject rabbitPrefab;
    [SerializeField] private GameObject foxPrefab;
    [SerializeField] private GameObject deerPrefab;
    [SerializeField] private GameObject boarPrefab;

    [SerializeField] private Collider2D petDropArea;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PetRoomInventoryManager inventoryManager;

    private int petId;
    private int petTypeId;
    private GameObject previewPet;
    private bool isDragging;

    public void SetPetData(int id, int typeId)
    {
        petId = id;
        petTypeId = typeId;
    }

    public void OnPointerDown(PointerEventData eventData)
    {

        GameObject prefab = GetPetPrefab(petTypeId);

        if (prefab == null)
        {
            return;
        }

        previewPet = Instantiate(prefab);
        //���� �̹��� ũ�� ����
        previewPet.transform.localScale = new Vector3(10f, 10f, 2f);
        isDragging = true;

    }

    private void Update()
    {
        if (!isDragging || previewPet == null) return;

        Vector3 worldPos = GetMouseWorldPosition();
        previewPet.transform.position = worldPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"[PetSlotDrag] PointerUp ���� / slot:{gameObject.name}");

        if (previewPet == null)
        {
            isDragging = false;
            return;
        }

        Vector3 worldPos = GetMouseWorldPosition();

        bool isInsideDropArea = petDropArea != null && petDropArea.OverlapPoint(worldPos);


        if (isInsideDropArea)
        {
            previewPet.transform.position = worldPos;

            Transform placedPetTransform = previewPet.transform;

            if (NetworkManager.Instance != null && petId > 0)
            {
                NetworkManager.Instance.RequestPetData(
                    petId,
                    response =>
                    {
                        if (inventoryManager != null)
                        {
                            inventoryManager.OnPetPlaced(response, placedPetTransform);
                        }
                    },
                    error =>
                    {
                        Debug.LogError("[PetSlotDrag] �� ������ ��û ����: " + error);
                    }
                );
            }
        }
        else
        {
            Debug.LogWarning("[PetSlotDrag] DropArea ���̶� previewPet ����");
            Destroy(previewPet);
        }

        previewPet = null;
        isDragging = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            return Vector3.zero;
        }

        Vector3 screenPos = Mouse.current.position.ReadValue();
        screenPos.z = Mathf.Abs(mainCamera.transform.position.z);

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        return worldPos;
    }

    private GameObject GetPetPrefab(int typeId)
    {
        switch (typeId)
        {
            case 1: return rabbitPrefab;
            case 2: return foxPrefab;
            case 3: return deerPrefab;
            case 4: return boarPrefab;
            default: return null;
        }
    }
}