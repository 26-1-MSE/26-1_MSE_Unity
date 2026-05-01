using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject petInventoryContent;
    [SerializeField] private GameObject itemInventoryContent;

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        ShowPetInventory();
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }

    public void ShowPetInventory()
    {
        petInventoryContent.SetActive(true);
        itemInventoryContent.SetActive(false);
    }

    public void ShowItemInventory()
    {
        petInventoryContent.SetActive(false);
        itemInventoryContent.SetActive(true);
    }
}