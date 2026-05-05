using TMPro;
using UnityEngine;

public class ProfileUI : MonoBehaviour
{
    [Header("Profile Text")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text petShopNameText;

    private void OnEnable()
    {
        DataManager.OnProfileChanged += RefreshProfile;
        RefreshProfile();
    }

    private void OnDisable()
    {
        DataManager.OnProfileChanged -= RefreshProfile;
    }

    private void RefreshProfile()
    {
        if (DataManager.Data == null)
            return;

        if (nicknameText != null)
            nicknameText.text = DataManager.Data.Nickname;

        if (petShopNameText != null)
            petShopNameText.text = DataManager.Data.PetShopName;
    }
}