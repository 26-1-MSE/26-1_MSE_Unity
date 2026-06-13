using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Manages the pet slot UI in the pet room and switching between pet/food inventory panels.
/// </summary>
public class PetRoomInventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel_Pet;
    [SerializeField] private GameObject inventoryPanel_Food;

    [Header("Pet Slot Images")]
    [SerializeField] private Image[] petSlotImages = new Image[4];

    [Header("Pet Sprites")]
    [SerializeField] private Sprite rabbitSprite;
    [SerializeField] private Sprite foxSprite;
    [SerializeField] private Sprite deerSprite;
    [SerializeField] private Sprite boarSprite;

    [SerializeField] private InventoryUIManager inventoryUIManager;
    [SerializeField] private PetGrowthManager petGrowthManager;


    private void Start()
    {
        RefreshPetInventory();
    }

    // Updates pet slot icons and passes pet data to each PetSlotDrag
    private void RefreshPetInventory()
    {
        if (DataManager.Data == null)
            return;

        DataManager.OwnedPetSlot[] pets = DataManager.Data.OwnedPetSlots;

        for (int i = 0; i < petSlotImages.Length; i++)
        {
            if (i >= pets.Length || pets[i].petId == 0)
            {
                petSlotImages[i].sprite = null;
                petSlotImages[i].enabled = false;

                // Pass empty data to PetSlotDrag
                PetSlotDrag dragEmpty = petSlotImages[i].GetComponentInParent<PetSlotDrag>();
                if (dragEmpty != null)
                    dragEmpty.SetPetData(0, 0);

                continue;
            }

            petSlotImages[i].sprite = GetPetSprite(pets[i].petTypeId);
            petSlotImages[i].enabled = true;

            // Pass pet id and type to PetSlotDrag
            PetSlotDrag drag = petSlotImages[i].GetComponentInParent<PetSlotDrag>();
            if (drag != null)
                drag.SetPetData(pets[i].petId, pets[i].petTypeId);
        }
    }

    // Returns the sprite matching the given pet type id
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

    // Called when a pet is successfully placed in the room
    public void OnPetPlaced(PetRoomResponse response, Transform placedPetTransform)
    {
        if (petGrowthManager != null)
        {
            petGrowthManager.SetCurrentPet(response, placedPetTransform);
        }

        inventoryUIManager.ShowItemInventory();
    }
}