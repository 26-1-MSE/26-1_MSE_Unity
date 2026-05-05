using TMPro;
using UnityEngine;

public class NicknameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nicknameText;

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