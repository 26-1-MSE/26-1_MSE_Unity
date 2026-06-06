using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 

public class LetterUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject listPanel;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private GameObject newBubbleIcon;

    [Header("List")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject noteItemPrefab;
    [SerializeField] private TMP_Text noteCountText;

    [Header("Detail")]
    [SerializeField] private TMP_Text detailTitleText;
    [SerializeField] private TMP_Text toText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text fromText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text readStateText;
    [SerializeField] private Image readStateBg;

    private List<MailData> mails = new List<MailData>();

    private void Start()
    {
        CloseAll();
    }

    public void OpenNotes()
    {
        if (GetComponent<PublicUIManager>().IsAnyPanelOpen()) return;
        GetComponent<PublicUIManager>().SetCurrentPanel(listPanel);
        background.SetActive(true);
        listPanel.SetActive(true);
        detailPanel.SetActive(false);
        LoadMailListFromServer();
    }

    private void LoadMailListFromServer()
    {
        NetworkManager.Instance.RequestMailList(
            response =>
            {
                mails.Clear();

                foreach (var mail in response.data.mails)
                {
                    mails.Add(new MailData
                    {
                        id = mail.mailId,
                        title = mail.title,
                        senderName = mail.sender,
                        date = mail.createdAt,
                        isRead = mail.isRead,
                        body = ""
                    });
                }

                RefreshList();
                RefreshNewBubble();
            },
            error =>
            {
                Debug.LogError("[LetterUIManager] 메일 목록 조회 실패: " + error);
            }
        );
    }

    public void CloseAll()
    {
        GetComponent<PublicUIManager>().ClearCurrentPanel();
        background.SetActive(false);
        listPanel.SetActive(false);
        detailPanel.SetActive(false);
    }

    public void OpenDetail(MailData mail)
    {
        NetworkManager.Instance.RequestMailDetail(
            mail.id,
            response =>
            {
                MailDetailData detail = response.data;

                mail.isRead = detail.isRead;
                mail.body = detail.content;
                mail.title = detail.title;
                mail.senderName = detail.sender;
                mail.date = detail.createdAt;
                mail.nickname = detail.nickname;

                detailTitleText.text = detail.title;
                toText.text = "To. " + detail.nickname;
                bodyText.text = detail.content;
                fromText.text = "From. " + detail.sender;
                dateText.text = detail.createdAt;

                readStateText.text = detail.isRead ? "Read" : "NEW";
                readStateBg.color = detail.isRead
                    ? new Color(1, 0.2f, 0)
                    : new Color(1, 1, 1);

                detailPanel.SetActive(true);

                RefreshList();
                RefreshNewBubble();
            },
            error =>
            {
                Debug.LogError("[LetterUIManager] 메일 상세 조회 실패: " + error);
            }
        );
    }

    public void CloseDetail()
    {
        detailPanel.SetActive(false);
    }

    private void RefreshList()
    {
        if (contentParent == null)
        {
            Debug.LogError("Content Parent가 비어있음");
            return;
        }

        if (noteItemPrefab == null)
        {
            Debug.LogError("Note Item Prefab이 비어있음");
            return;
        }

        if (noteCountText == null)
        {
            Debug.LogError("Note Count Text가 비어있음");
            return;
        }

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (MailData mail in mails)
        {
            GameObject item = Instantiate(noteItemPrefab, contentParent);

            NoteItemUI itemUI = item.GetComponent<NoteItemUI>();
            if (itemUI == null)
            {
                Debug.LogError("NoteItem 프리팹에 NoteItemUI 스크립트가 없음");
                return;
            }

            itemUI.Setup(mail, this);
        }

        //new 개수 세서 표시
        int newCount = 0;
        foreach (MailData mail in mails)
            if (!mail.isRead) newCount++;

        noteCountText.text = newCount > 0
            ? $"{newCount} NEW / {mails.Count}"
            : $"{mails.Count} / 10";
    }
    private void RefreshNewBubble()
    {
        bool hasNew = false;

        foreach (MailData mail in mails)
        {
            if (!mail.isRead)
            {
                hasNew = true;
                break;
            }
        }

        if (newBubbleIcon != null)
            newBubbleIcon.SetActive(hasNew);
    }
}

