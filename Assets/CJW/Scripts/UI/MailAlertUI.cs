using UnityEngine;

public class MailAlertUI : MonoBehaviour
{
    [SerializeField] private GameObject unreadMailIcon;

    private void OnEnable()
    {
        RefreshFromDataManager();
    }

    public void RefreshFromDataManager()
    {
        if (DataManager.Data == null)
        {
            if (unreadMailIcon != null)
                unreadMailIcon.SetActive(false);

            return;
        }

        bool hasUnread = DataManager.Data.HasUnreadMail;

        if (unreadMailIcon != null)
            unreadMailIcon.SetActive(hasUnread);
    }
}