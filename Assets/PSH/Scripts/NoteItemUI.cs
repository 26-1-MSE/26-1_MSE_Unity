using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text senderText;
    [SerializeField] private TMP_Text previewText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text newText;
    [SerializeField] private Button button;

    private MailData mail;
    private NotesUIManager manager;

    public void Setup(MailData mailData, NotesUIManager uiManager)
    {
        mail = mailData;
        manager = uiManager;

        senderText.text = mail.senderName;
        previewText.text = mail.title;
        dateText.text = mail.date;
        newText.gameObject.SetActive(true);
        newText.text = mail.isRead ? "Read" : "New";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => manager.OpenDetail(mail));
    }
}