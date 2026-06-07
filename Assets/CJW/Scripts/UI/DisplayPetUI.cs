using UnityEngine;
using UnityEngine.UI;

public class DisplayPetUI : MonoBehaviour
{
    [Header("Pet Slot Images")]
    [SerializeField] private Image[] petImages = new Image[4];

    [Header("Pet Sprites")]
    [SerializeField] private Sprite rabbitSprite;
    [SerializeField] private Sprite foxSprite;
    [SerializeField] private Sprite deerSprite;
    [SerializeField] private Sprite boarSprite;

    public void RefreshPetInventory()
    {
        Debug.Log("[DisplayPetInventoryUI] RefreshPetInventory 호출됨");

        if (DataManager.Data == null)
        {
            Debug.LogWarning("[DisplayPetInventoryUI] DataManager.Data 없음");
            return;
        }

        DataManager.OwnedPetSlot[] pets = DataManager.Data.OwnedPetSlots;

        if (pets == null)
        {
            Debug.LogWarning("[DisplayPetInventoryUI] OwnedPetSlots 없음");
            ClearAllSlots();
            return;
        }

        for (int i = 0; i < petImages.Length; i++)
        {
            if (i >= pets.Length || pets[i].petId == 0 || pets[i].petTypeId == 0)
            {
                ClearSlot(i);
                continue;
            }

            Sprite petSprite = GetPetSprite(pets[i].petTypeId);
            bool hasPet = petSprite != null;

            petImages[i].gameObject.SetActive(hasPet);
            petImages[i].sprite = petSprite;
            petImages[i].enabled = hasPet;

            Debug.Log($"[DisplayPetInventoryUI] UI 세팅 / slot:{i}, petId:{pets[i].petId}, typeId:{pets[i].petTypeId}, sprite:{petSprite}");
        }
    }

    private void ClearAllSlots()
    {
        for (int i = 0; i < petImages.Length; i++)
        {
            ClearSlot(i);
        }
    }

    private void ClearSlot(int index)
    {
        if (petImages[index] != null)
        {
            petImages[index].sprite = null;
            petImages[index].enabled = false;
            petImages[index].gameObject.SetActive(false);
        }
    }

    private Sprite GetPetSprite(int petTypeId)
    {
        switch (petTypeId)
        {
            case 1: return rabbitSprite;
            case 2: return foxSprite;
            case 3: return deerSprite;
            case 4: return boarSprite;
            default: return null;
        }
    }
}