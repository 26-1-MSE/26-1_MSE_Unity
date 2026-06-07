using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject petInventoryContent;
    [SerializeField] private GameObject itemInventoryContent;
    [SerializeField] private GameObject foodPanel;

    private PublicUIManager publicUI;

    private void Start()
    {
        publicUI = GetComponent<PublicUIManager>();
    }

    public void OpenInventory()
    {
        publicUI.OpenPanel(inventoryPanel);
        ShowPetInventory();
    }

    public void CloseInventory()
    {
        publicUI.ClosePanel();
    }



    public void ShowPetInventory()
    {
        petInventoryContent.SetActive(true);
        itemInventoryContent.SetActive(false);
    }

    public void ShowItemInventory()
    {
        petInventoryContent.SetActive(false);
        if (foodPanel != null) foodPanel.SetActive(true);
        itemInventoryContent.SetActive(true);
        GetComponent<ItemInventoryManager>().RefreshItemInventory();
    }
}