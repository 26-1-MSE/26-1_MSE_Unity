using UnityEngine;
using UnityEngine.UI;

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


    private void Start()
    {
        RefreshPetInventory();
    }

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

                // PetSlotDrag에 0 전달
                PetSlotDrag dragEmpty = petSlotImages[i].GetComponentInParent<PetSlotDrag>();
                if (dragEmpty != null)
                    dragEmpty.SetPetTypeId(0);

                continue;
            }

            petSlotImages[i].sprite = GetPetSprite(pets[i].petTypeId);
            petSlotImages[i].enabled = true;

            // PetSlotDrag에 typeId 전달
            PetSlotDrag drag = petSlotImages[i].GetComponentInParent<PetSlotDrag>();
            if (drag != null)
                drag.SetPetTypeId(pets[i].petTypeId);
        }
    }

    //typeId에 맞는 동물 스프라이트
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


    // 펫 드래그해서 내놓을 때 호출
    public void OnPetPlaced()
    {
        inventoryPanel_Pet.SetActive(false);
        inventoryPanel_Food.SetActive(true);
    }
}