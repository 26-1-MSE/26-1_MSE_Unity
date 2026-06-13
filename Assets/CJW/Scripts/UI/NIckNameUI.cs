using TMPro;
using UnityEngine;

/// <summary>
/// Displays the player's nickname.
/// Updates automatically when profile data changes.
/// </summary>
public class NicknameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nicknameText;

    private void Awake()
    {
        if (nicknameText == null)
            nicknameText = GetComponent<TMP_Text>();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerName = "Default";
            meshRenderer.sortingOrder = 2;
        }
    }

    private void OnEnable()
    {
        DataManager.OnProfileChanged += RefreshNickname;
        RefreshNickname();
    }

    private void OnDisable()
    {
        DataManager.OnProfileChanged -= RefreshNickname;
    }


    // Updates nickname text using the latest profile data from DataManager.
    private void RefreshNickname()
    {
        if (DataManager.Data == null || nicknameText == null)
            return;

        nicknameText.text = DataManager.Data.Nickname;
    }
}