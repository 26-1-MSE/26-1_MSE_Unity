using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Displays a single mail item in the mail list and opens its detail view on click.
/// </summary>
public class NoteItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text senderText;
    [SerializeField] private TMP_Text previewText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private Button button;
    [SerializeField] private Image readStateBg;
    [SerializeField] private TMP_Text readStateText;

    private MailData mail;
    private LetterUIManager manager;

    // Binds mail data to the UI and sets up the click listener
    public void Setup(MailData mailData, LetterUIManager uiManager)
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

    // Updates the read/unread badge text and color
    public void SetReadState(bool isRead)
    {
        if (isRead)
        {
            readStateText.text = "Read";
            readStateBg.color = new Color(1, 0.2f, 0); 
        }
        else
        {
            readStateText.text = "NEW";
            readStateBg.color = new Color(1, 1, 1); 
        }
    }
}