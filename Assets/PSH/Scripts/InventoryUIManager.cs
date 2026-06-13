using System.Collections;
using UnityEngine;


/// <summary>
/// Manages inventory UI panels.
/// Handles opening/closing the inventory window and switching between pet and item inventories.
/// </summary>
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

    // Opens the inventory panel and requests
    public void OpenInventory()
    {
        ShowPetInventory(); 
        publicUI.OpenPanel(inventoryPanel);
        NetworkManager.Instance.RequestInventoryData(
            response =>
            {
                Debug.Log($"[InventoryUIManager] pets:{response.data.pets.Length}, items:{response.data.items.Length}");
                ShowPetInventory();
            },
            error =>
            {
                Debug.LogError("[InventoryUIManager] /inventory response failed : " + error);
                ShowPetInventory();
            }
        );
    }

    // Closes the inventory panel.
    public void CloseInventory()
    {
        publicUI.ClosePanel();
        StartCoroutine(ResetAfterClose());
    }

    // Resets the inventory view to the pet tab after the close animation finishes.
    private IEnumerator ResetAfterClose()
    {
        yield return new WaitForSeconds(publicUI.CloseDelay);
        ShowPetInventory();
    }

    // Displays the pet inventory tab and refreshes pet inventory UI.
    public void ShowPetInventory()
    {
        petInventoryContent.SetActive(true);
        itemInventoryContent.SetActive(false);
        GetComponent<DisplayPetUI>()?.RefreshPetInventory();
    }

    // Displays the item inventory tab and refreshes item inventory UI.
    public void ShowItemInventory()
    {
        petInventoryContent.SetActive(false);
        if (foodPanel != null)
            foodPanel.SetActive(true);
        itemInventoryContent.SetActive(true);
        GetComponent<ItemInventoryManager>()?.RefreshItemInventory();
    }
}