using UnityEngine;

public class MailAlertUI : MonoBehaviour
{
    [Header("Mail Alert")]
    [SerializeField] private GameObject unreadMailIcon;

    private void OnEnable()
    {
        DataManager.OnUnreadMailStateChanged += RefreshMailAlert;

        if (DataManager.Data != null)
        {
            RefreshMailAlert(DataManager.Data.HasUnreadMail);
        }
    }

    private void OnDisable()
    {
        DataManager.OnUnreadMailStateChanged -= RefreshMailAlert;
    }

    private void RefreshMailAlert(bool hasUnreadMail)
    {
        if (unreadMailIcon != null)
        {
            unreadMailIcon.SetActive(hasUnreadMail);
        }
    }
}