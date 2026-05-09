using TMPro;
using UnityEngine;

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
            meshRenderer.sortingOrder = 200;
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

    private void RefreshNickname()
    {
        if (DataManager.Data == null || nicknameText == null)
            return;

        nicknameText.text = DataManager.Data.Nickname;
    }
}