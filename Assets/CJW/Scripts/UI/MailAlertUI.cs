using UnityEngine;


// Controls the unread mail notification icon.
public class MailAlertUI : MonoBehaviour
{
    [SerializeField] private GameObject unreadMailIcon;

    private void OnEnable()
    {
        RefreshFromDataManager();
    }

    // Updates the unread mail icon using the current mail state stored in DataManager.
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