using TMPro;
using UnityEngine;

/// <summary>
/// Displays player profile information.
/// Updates nickname and pet shop name when profile data changes.
/// </summary>
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

    // Updates profile UI using the latest data stored in DataManager.
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