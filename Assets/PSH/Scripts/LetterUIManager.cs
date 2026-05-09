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
        // 임시 더미 데이터. 나중에 서버 데이터로 교체
        mails.Add(new MailData
        {
            id = 1,
            senderName = "Nick",
            title = "You know you love me.",
            body = "Never let 'em see that they get to you. " +
                   "I thought you were the one who'd believe in me. " +
                   "Because you're my pack.",
            date = "2024.05.21 12:39",
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
            date = "2024.05.20 14:06",
            isRead = true
        });

        mails.Add(new MailData
        {
            id = 3,
            senderName = "Finnick",
            title = "Toot toot",
            body = "You kiss me tomorrow, I'll bite your face off! Ciao.",
            date = "2024.05.19 09:12",
            isRead = false
        });

        mails.Add(new MailData
        {
            id = 4,
            senderName = "Gary",
            title = "Permission to hug?",
            body = "I have to prove it. Please. This is our only chance to set things right. " +
                   "And when I do, my family will finally be able to come home.",
            date = "2024.05.18 16:58",
            isRead = true
        });

        mails.Add(new MailData
        {
            id = 5,
            senderName = "Gazelle ",
            title = "Good evening, Zootopia!",
            body = "I'm Gazelle, and you are one hot dancer.",
            date = "2024.05.17 20:45",
            isRead = true
        });

        mails.Add(new MailData
        {
            id = 6,
            senderName = "Flash",
            title = "Priscilla......",
            body = "What do you call... a three humped camel?",
            date = "2024.05.16 17:22",
            isRead = false
        });

        mails.Add(new MailData
        {
            id = 7,
            senderName = "Nibbles",
            title = "Heya, bub.",
            body = "Your partner needs you and Nibbles Maplestick is going to get you to her.",
            date = "2024.05.15 09:34",
            isRead = false
        });

        mails.Add(new MailData
        {
            id = 8,
            senderName = "Mr. Big",
            title = "Ice 'em.",
            body = "I trusted you, Nicky. We broke bread together. " +
                   "Grandmama made you a cannoli. " +
                   "And how did you repay my generosity? With a rug. " +
                   "Made from the butt of a skunk. A skunk butt rug.",
            date = "2024.05.15 14:27",
            isRead = true
        });

        mails.Add(new MailData
        {
            id = 9,
            senderName = "Clawhauser",
            title = "Are you familiar with Gazelle?",
            body = "Greatest singer of our lifetime. Angel with horns.",
            date = "2024.05.14 10:15",
            isRead = false
        });

        mails.Add(new MailData
        {
            id = 10,
            senderName = "Chief Bogo",
            title = "You're fired.",
            body = "It's not about how badly you WANT something. It's about what you are CAPABLE of!",
            date = "2024.05.13 13:41",
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
        bool wasRead = mail.isRead;

        mail.isRead = true;

        detailTitleText.text = mail.title;
        toText.text = "To. Player";
        bodyText.text = mail.body;
        fromText.text = "From. " + mail.senderName;
        dateText.text = mail.date;


        //detail 패널 read/new 색 변경
        if (wasRead)
        {
            readStateText.text = "Read";
            readStateBg.color = new Color(1, 0.2f, 0); // 주황
        }
        else
        {
            readStateText.text = "NEW";
            readStateBg.color = new Color(1, 1, 1); // 초록
        }

        // 열람 후 읽음 처리
        mail.isRead = true;

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

