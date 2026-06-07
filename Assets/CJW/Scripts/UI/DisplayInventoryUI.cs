using UnityEngine;
using UnityEngine.UI;

public class DisplayInventoryUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject petInventoryContent;
    [SerializeField] private GameObject itemInventoryContent;

    [Header("Pet Slots")]
    [SerializeField] private Image[] petImages = new Image[4];

    [Header("Pet Sprites")]
    [SerializeField] private Sprite rabbitSprite;
    [SerializeField] private Sprite foxSprite;
    [SerializeField] private Sprite deerSprite;
    [SerializeField] private Sprite boarSprite;

    [Header("Item Manager")]
    [SerializeField] private ItemInventoryManager itemInventoryManager;

    private void Start()
    {
        ShowPetInventory();
    }

    public void ShowPetInventory()
    {
        petInventoryContent.SetActive(true);
        itemInventoryContent.SetActive(false);

        RefreshPetInventory();
    }

    public void ShowItemInventory()
    {
        petInventoryContent.SetActive(false);
        itemInventoryContent.SetActive(true);

        itemInventoryManager?.RefreshItemInventory();
    }

    private void RefreshPetInventory()
    {
        if (DataManager.Data == null)
        {
            Debug.LogWarning("[DisplayInventoryUI] DataManager.Data ¾øÀ½");
            return;
        }

        var pets = DataManager.Data.OwnedPetSlots;

        for (int i = 0; i < petImages.Length; i++)
        {
            if (i >= pets.Length || pets[i].petId == 0)
            {
                petImages[i].sprite = null;
                petImages[i].enabled = false;
                continue;
            }

            Sprite petSprite = GetPetSprite(pets[i].petTypeId);

            petImages[i].sprite = petSprite;
            petImages[i].enabled = (petSprite != null);

            Debug.Log(
                $"[DisplayInventoryUI] slot:{i} petId:{pets[i].petId} type:{pets[i].petTypeId}"
            );
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