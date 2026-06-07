using UnityEngine;

public class MailAlertUI : MonoBehaviour
{
    [SerializeField] private GameObject unreadMailIcon;

    private void OnEnable()
    {
        RefreshFromServer();
    }

    public void RefreshFromServer()
    {
        NetworkManager.Instance.RequestMailList(
            response =>
            {
                bool hasUnread = false;

                foreach (var mail in response.data.mails)
                {
                    if (!mail.isRead)
                    {
                        hasUnread = true;
                        break;
                    }
                }

                if (unreadMailIcon != null)
                    unreadMailIcon.SetActive(hasUnread);
            },
            error =>
            {
                Debug.LogError("[MailAlertUI] 메일 알림 조회 실패: " + error);
            }
        );
    }
}