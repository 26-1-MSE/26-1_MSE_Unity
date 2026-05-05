using UnityEngine;
using TMPro;

public class ShopNameDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text shopNameText;

    private void OnEnable()
    {
        DataManager.OnProfileChanged += UpdateShopName;
    }

    private void OnDisable()
    {
        DataManager.OnProfileChanged -= UpdateShopName;
    }

    private void UpdateShopName()
    {
        shopNameText.text = DataManager.Data.PetShopName;
    }
}