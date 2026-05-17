using UnityEngine;
using TMPro;

public class NoteObject : MonoBehaviour
{
    public KeyCode requiredKey;
    [SerializeField] private TextMeshProUGUI keyText;
    private float moveSpeed = 4f;
    [SerializeField] private float missLineX = -450f;
    [SerializeField] private float judgeLineX = -400f;
    [SerializeField] private float judgeRange = 80f;
    private bool isMissed = false;
    private bool isHandled = false;

    public bool IsJudgeable =>
        transform.localPosition.x <= judgeLineX + judgeRange &&
        transform.localPosition.x >= judgeLineX - judgeRange;

    public void Init(KeyCode key)
    {
        requiredKey = key;
        if (keyText != null)
            keyText.text = key.ToString();
    }

    private void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        if (!isMissed && !isHandled && transform.localPosition.x < missLineX)
        {
            isMissed = true;
            isHandled = true;
            OcarinaGameManager.Instance.OnNoteMissed(this);
        }
    }

    public void MarkHandled()
    {
        isHandled = true;
    }
}