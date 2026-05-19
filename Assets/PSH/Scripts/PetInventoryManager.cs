using UnityEngine;

public class PetRoomInventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel_Pet;
    [SerializeField] private GameObject inventoryPanel_Food;

    // 펫 드래그해서 내놓을 때 호출
    public void OnPetPlaced()
    {
        inventoryPanel_Pet.SetActive(false);
        inventoryPanel_Food.SetActive(true);
    }
}