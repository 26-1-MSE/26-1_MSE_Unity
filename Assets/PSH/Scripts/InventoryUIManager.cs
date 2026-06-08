using System.Collections;
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
                Debug.LogError("[InventoryUIManager] /inventory 응답 실패 : " + error);
                ShowPetInventory();
            }
        );
    }

    public void CloseInventory()
    {
        publicUI.ClosePanel();
        StartCoroutine(ResetAfterClose());
    }

    private IEnumerator ResetAfterClose()
    {
        yield return new WaitForSeconds(publicUI.CloseDelay);
        ShowPetInventory();
    }

    public void ShowPetInventory()
    {
        petInventoryContent.SetActive(true);
        itemInventoryContent.SetActive(false);
        GetComponent<DisplayPetUI>()?.RefreshPetInventory();
    }

    public void ShowItemInventory()
    {
        petInventoryContent.SetActive(false);
        if (foodPanel != null)
            foodPanel.SetActive(true);
        itemInventoryContent.SetActive(true);
        GetComponent<ItemInventoryManager>()?.RefreshItemInventory();
    }
}