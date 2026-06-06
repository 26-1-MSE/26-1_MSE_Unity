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
        // 생성된 펫 크기 조정
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
        Debug.Log($"[PetSlotDrag] PointerUp 감지 / slot:{gameObject.name}");


        if (previewPet == null)
        {
            isDragging = false;
            return;
        }

        Vector3 worldPos = GetMouseWorldPosition();
        bool isInsideDropArea = petDropArea != null && petDropArea.OverlapPoint(worldPos);

        if (!isInsideDropArea)
        {
            Debug.LogWarning("[PetSlotDrag] DropArea 밖이라 previewPet 삭제");
            Destroy(previewPet);

            previewPet = null;
            isDragging = false;
            return;
        }

        previewPet.transform.position = worldPos;
        Transform placedPetTransform = previewPet.transform;

        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[PetSlotDrag] NetworkManager.Instance 없음");
            Destroy(placedPetTransform.gameObject);

            previewPet = null;
            isDragging = false;
            return;
        }

        if (petId <= 0)
        {
            Debug.LogError($"[PetSlotDrag] 유효하지 않은 petId:{petId}");
            Destroy(placedPetTransform.gameObject);

            previewPet = null;
            isDragging = false;
            return;
        }

        NetworkManager.Instance.RequestPetData(
            petId,
            response =>
            {
                Debug.Log("[PetSlotDrag] 펫 데이터 수신 성공");

                if (inventoryManager != null)
                {
                    inventoryManager.OnPetPlaced(response, placedPetTransform);
                }
                else
                {
                    Debug.LogError("[PetSlotDrag] inventoryManager 연결 안 됨");
                }
            },
            error =>
            {
                Debug.LogError("[PetSlotDrag] 펫 데이터 요청 실패: " + error);

                if (placedPetTransform != null)
                {
                    Destroy(placedPetTransform.gameObject);
                }
            }
        );

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