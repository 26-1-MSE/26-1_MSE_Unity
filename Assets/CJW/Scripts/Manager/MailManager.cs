using UnityEngine;

public class MailManager : MonoBehaviour
{
    [SerializeField] private ToastMessage toastMessage;

    private void Start()
    {
        NetworkManager.Instance.RequestMailList(
            response =>
            {
                Debug.Log("메일 개수: " + response.data.mails.Length);

                foreach (var mail in response.data.mails)
                {
                    Debug.Log($"메일 / id:{mail.mailId}, title:{mail.title}, sender:{mail.sender}, read:{mail.isRead}");
                }

                if (response.data.mails.Length > 0)
                {
                    int firstMailId = response.data.mails[0].mailId;
                    RequestDetailTest(firstMailId);
                }
            },
            error =>
            {
                Debug.LogError("[MailManager] 목록 조회 실패: " + error);
                toastMessage?.ShowToast(error);
            }
        );
    }

    private void RequestDetailTest(int mailId)
    {
        NetworkManager.Instance.RequestMailDetail(
            mailId,
            response =>
            {
                Debug.Log("===== 메일 상세 =====");
                Debug.Log("id: " + response.data.mailId);
                Debug.Log("title: " + response.data.title);
                Debug.Log("nickname: " + response.data.nickname);
                Debug.Log("sender: " + response.data.sender);
                Debug.Log("content: " + response.data.content);
                Debug.Log("isRead: " + response.data.isRead);
                Debug.Log("createdAt: " + response.data.createdAt);
            },
            error =>
            {
                Debug.LogError("[MailManager] 상세 조회 실패: " + error);
                toastMessage?.ShowToast(error);
            }
        );
    }
}