using UnityEngine;
using UnityEngine.InputSystem;

public class SceneMove : MonoBehaviour
{
    public enum TargetScene
    {
        Lobby,
        PetTown,
        PetRoom,
        Island
    }

    public enum InputType
    {
        TriggerKey,   // S키 + 트리거
        Button        // UI 버튼
    }

    [Header("이동 설정")]
    [SerializeField] private TargetScene targetScene;
    [SerializeField] private InputType inputType;

    private bool playerInRange = false;

    private void Update()
    {
        if (inputType != InputType.TriggerKey) return;
        if (!playerInRange) return;

        if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
        {
            Move();
        }
    }

    // 버튼에서 호출할 함수
    public void OnClickMove()
    {
        if (inputType == InputType.Button)
        {
            Move();
        }
    }

    private void Move()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance가 없습니다.");
            return;
        }

        switch (targetScene)
        {
            case TargetScene.Lobby:
                GameManager.Instance.GoToLobby();
                break;
            case TargetScene.PetTown:
                GameManager.Instance.GoToPetTown();
                break;
            case TargetScene.PetRoom:
                GameManager.Instance.GoToPetRoom();
                break;
            case TargetScene.Island:
                GameManager.Instance.GoToIsland();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (inputType != InputType.TriggerKey) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (inputType != InputType.TriggerKey) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}