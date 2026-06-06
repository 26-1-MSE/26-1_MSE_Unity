using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject petInventoryContent;
    [SerializeField] private GameObject itemInventoryContent;
    [SerializeField] private GameObject foodPanel;
    public void OpenInventory()
    {
        Debug.Log("OpenInventory 호출됨");
        if (GetComponent<PublicUIManager>().IsAnyPanelOpen()) return;
        GetComponent<PublicUIManager>().SetCurrentPanel(inventoryPanel);
        inventoryPanel.SetActive(true);
        ShowPetInventory();
    }

    public void CloseInventory()
    {
        GetComponent<PublicUIManager>().ClearCurrentPanel();
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
        if (foodPanel != null)
            foodPanel.SetActive(true);
        itemInventoryContent.SetActive(true);
        GetComponent<ItemInventoryManager>().RefreshItemInventory();
    }
}