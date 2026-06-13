/// <summary>
/// Serializable data structure representing a single mail/letter item.
/// </summary>


[System.Serializable]
public class MailData
{
    public int id;
    public string senderName;
    public string title;
    public string body;
    public string date;
    public bool isRead;
    public string nickname;
}