using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotesUIManager : MonoBehaviour
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
    [SerializeField] private TMP_Text toText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text fromText;
    [SerializeField] private TMP_Text readStateText;

    private List<MailData> mails = new List<MailData>();

    private void Start()
    {
        // 임시 더미 데이터. 나중에 서버 데이터로 교체
        mails.Add(new MailData
        {
            id = 1,
            senderName = "Nick",
            title = "You know you love me.",
            body = "Never let 'em see that they get to you. " +
                   "I thought you were the one who'd believe in me. " +
                   "Because you're my pack.",
            date = "2024.05.21",
            isRead = false
        });

        mails.Add(new MailData
        {
            id = 2,
            senderName = "Judy",
            title = "I really am just a dumb bunny.",
            body = "I am so sorry for the things I said. " +
                   "I know you'll never forgive me, and I don't blame you. " +
                   "But we're a team, right? I don't know when to quit.!",
            date = "2024.05.20",
            isRead = true
        });

        CloseAll();
        RefreshNewBubble();
    }

    public void OpenNotes()
    {
        background.SetActive(true);
        listPanel.SetActive(true);
        detailPanel.SetActive(false);

        RefreshList();
    }

    public void CloseAll()
    {
        background.SetActive(false);
        listPanel.SetActive(false);
        detailPanel.SetActive(false);
    }

    public void OpenDetail(MailData mail)
    {
        mail.isRead = true;

        toText.text = "To. Player";
        bodyText.text = mail.body;
        fromText.text = "From. " + mail.senderName;
        readStateText.text = mail.isRead ? "Read" : "NEW";

        detailPanel.SetActive(true);

        RefreshList();
        RefreshNewBubble();
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

        noteCountText.text = mails.Count + "/10";
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