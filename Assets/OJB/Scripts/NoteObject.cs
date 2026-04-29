using UnityEngine;
using TMPro;

public class NoteObject : MonoBehaviour
{
    public KeyCode requiredKey;  // 이 음표에 필요한 키
    [SerializeField] private TextMeshProUGUI keyText;  // 음표 안에 표시될 키 텍스트
    private float moveSpeed = 5f;  // 이동 속도

    public void Init(KeyCode key)
    {
        requiredKey = key;
        if (keyText != null)
            keyText.text = key.ToString();
    }

    private void Update()
    {
        // 오른쪽 → 왼쪽으로 이동
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }
}