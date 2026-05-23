using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject petInventoryContent;
    [SerializeField] private GameObject itemInventoryContent;
    [SerializeField] private GameObject foodPanel;

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

        GetComponent<ItemInventoryManager>().RefreshItemInventory();
    }

    public void ShowItemInventory()
    {
        petInventoryContent.SetActive(false);

        foodPanel.SetActive(true);
        itemInventoryContent.SetActive(true);

        GetComponent<ItemInventoryManager>().RefreshItemInventory();

        Debug.Log($"itemInventoryContent active: {itemInventoryContent.activeInHierarchy}");
    }
}