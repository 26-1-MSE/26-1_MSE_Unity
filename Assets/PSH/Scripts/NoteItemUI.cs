using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text senderText;
    [SerializeField] private TMP_Text previewText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private Button button;
    [SerializeField] private Image readStateBg;
    [SerializeField] private TMP_Text readStateText;

    private MailData mail;
    private NotesUIManager manager;

    public void Setup(MailData mailData, NotesUIManager uiManager)
    {
        mail = mailData;
        manager = uiManager;

        senderText.text = mail.senderName;
        previewText.text = mail.title;
        dateText.text = mail.date;

        SetReadState(mail.isRead); 

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => manager.OpenDetail(mail));
    }

    public void SetReadState(bool isRead)
    {
        if (isRead)
        {
            readStateText.text = "Read";
            readStateBg.color = new Color(1, 0.2f, 0); // 주황
        }
        else
        {
            readStateText.text = "NEW";
            readStateBg.color = new Color(1, 1, 1); // 초록
        }
    }
}