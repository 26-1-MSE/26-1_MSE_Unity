using UnityEngine;
using TMPro;


//Represents a single note object that scrolls across the score in the rhythm mini-game.
public class NoteObject : MonoBehaviour
{
    public KeyCode requiredKey;
    [SerializeField] private TextMeshProUGUI keyText;
    private float moveSpeed = 4f;

    [SerializeField] private float missLineX = -450f; // X position where the note is considered missed
    [SerializeField] private float judgeLineX = -400f;// X position of the judge line
    [SerializeField] private float judgeRange = 80f; // acceptable input range around the judge line

    private bool isMissed = false;
    private bool isHandled = false;

    // returns true if the note is within the acceptable input range
    public bool IsJudgeable =>
        transform.localPosition.x <= judgeLineX + judgeRange &&
        transform.localPosition.x >= judgeLineX - judgeRange;

    //Initializes the note with the required key and updates the key label
    public void Init(KeyCode key)
    {
        requiredKey = key;
        if (keyText != null)
            keyText.text = key.ToString();
    }

    private void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        //report a miss if the note passes the miss line without being handled
        if (!isMissed && !isHandled && transform.localPosition.x < missLineX)
        {
            isMissed = true;
            isHandled = true;
            OcarinaGameManager.Instance.OnNoteMissed(this);
        }
    }

    //marks this note as handled to prevent duplicate miss processing
    public void MarkHandled()
    {
        isHandled = true;
    }
}