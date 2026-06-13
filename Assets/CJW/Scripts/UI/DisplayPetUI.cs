using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Manages the pet inventory UI.
/// Displays owned pets and updates inventory slots with corresponding pet sprites.
/// </summary>
public class DisplayPetUI : MonoBehaviour
{
    [Header("Pet Slot Images")]
    [SerializeField] private Image[] petImages = new Image[4];

    [Header("Pet Sprites")]
    [SerializeField] private Sprite rabbitSprite;
    [SerializeField] private Sprite foxSprite;
    [SerializeField] private Sprite deerSprite;
    [SerializeField] private Sprite boarSprite;

    // Refreshes the pet inventory UI
    public void RefreshPetInventory()
    {
        Debug.Log("[DisplayPetInventoryUI] RefreshPetInventory is called");

        if (DataManager.Data == null)
        {
            Debug.LogWarning("[DisplayPetInventoryUI] There's no DataManager.Data ");
            return;
        }

        DataManager.OwnedPetSlot[] pets = DataManager.Data.OwnedPetSlots;

        if (pets == null)
        {
            Debug.LogWarning("[DisplayPetInventoryUI] There's no OwnedPetSlots ");
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

            Debug.Log($"[DisplayPetInventoryUI] UI setting / slot:{i}, petId:{pets[i].petId}, typeId:{pets[i].petTypeId}, sprite:{petSprite}");
        }
    }

    // Clears all inventory slots.
    private void ClearAllSlots()
    {
        for (int i = 0; i < petImages.Length; i++)
        {
            ClearSlot(i);
        }
    }

    // Clears a single inventory slot.
    private void ClearSlot(int index)
    {
        if (petImages[index] != null)
        {
            petImages[index].sprite = null;
            petImages[index].enabled = false;
            petImages[index].gameObject.SetActive(false);
        }
    }

    // Returns the sprite corresponding to the given pet type ID.
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