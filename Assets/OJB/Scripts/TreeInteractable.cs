using UnityEngine;

public class TreeInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName = "Apple";  // 나무 종류별로 설정
    [SerializeField] private int maxItemCount = 3;       // 최대 아이템 개수
    [SerializeField] private Animator treeAnimator;      // 나무 애니메이터

    private int currentItemCount;                        // 현재 남은 아이템 개수

    private void Start()
    {
        currentItemCount = maxItemCount;
    }

    public void Interact(PlayerInteraction player)
    {
        if (currentItemCount <= 0) return;  // 아이템 없으면 무시

        // 아이템 수집
        currentItemCount--;
        Debug.Log($"{itemName} 획득! 남은 개수: {currentItemCount}");

        // 나무 애니메이션 재생
        if (treeAnimator != null)
            treeAnimator.SetTrigger("chop");

        // 아이템 없으면 상호작용 비활성화
        if (currentItemCount <= 0)
            Debug.Log("나무 아이템 소진");
    }

    public string GetInteractMessage()
    {
        if (currentItemCount <= 0) return null;  // 아이템 없으면 텍스트 숨김
        return "상호작용하기";
    }
}
