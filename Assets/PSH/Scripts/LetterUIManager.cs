using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the mail list and detail UI, 
/// including loading mails from the server and read-state tracking.
/// </summary>
public class LetterUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject letterPanel;
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
    private PublicUIManager publicUI;

    private void Start()
    {
        publicUI = GetComponent<PublicUIManager>();
    }

    public void OpenNotes()
    {
        publicUI.OpenPanel(letterPanel);
        listPanel.SetActive(true);
        detailPanel.SetActive(false);
        LoadMailListFromServer();
    }

    public void CloseNotes()
    {
        publicUI.ClosePanel();
    }


    // Fetches mail detail from server, updates local data, and shows detail panel
    public void OpenDetail(MailData mail)
    {
        NetworkManager.Instance.RequestMailDetail(
            mail.id,
            response =>
            {
                MailDetailData detail = response.data;

                mail.isRead = true;
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

                readStateText.text = "Read";
                readStateBg.color = new Color(1, 0.2f, 0);

                detailPanel.SetActive(true);
                RefreshList();
                RefreshNewBubble();
            },
            error =>
            {
                Debug.LogError("[LetterUIManager] Failed to load mail detail: " + error);
            }
        );
    }

    public void CloseDetail()
    {
        detailPanel.SetActive(false);
    }

    // Fetches the mail list from the server and rebuilds local data
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
                Debug.LogError("[LetterUIManager] Failed to load mail list: " + error);
            }
        );
    }

    // Rebuilds the mail list UI and updates the new-mail count
    private void RefreshList()
    {
        if (contentParent == null)
        {
            Debug.LogError("[LetterUIManager] Content Parent is not assigned");
            return;
        }

        if (noteItemPrefab == null)
        {
            Debug.LogError("[LetterUIManager] Note Item Prefab is not assigned");
            return;
        }

        if (noteCountText == null)
        {
            Debug.LogError("[LetterUIManager] Note Count Text is not assigned");
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
                Debug.LogError("[LetterUIManager] NoteItemUI component missing on prefab");
                return;
            }

            itemUI.Setup(mail, this);
        }

        
        int newCount = 0;
        foreach (MailData mail in mails)
            if (!mail.isRead) newCount++;

        noteCountText.text = newCount > 0
            ? $"{newCount} NEW / {mails.Count}"
            : $"{mails.Count} / {mails.Count}";
    }

    // Shows or hides the "new mail" notification bubble
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

