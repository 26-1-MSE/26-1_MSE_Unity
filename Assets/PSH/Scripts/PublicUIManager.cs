using System.Collections;
using UnityEditor;
using UnityEngine;

public class PublicUIManager : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private float closeDelay = 0.3f;

    public void OpenPanel(GameObject panel)
    {
        background.SetActive(true);
        panel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        StartCoroutine(CloseAfterDelay(panel));
    }

    private IEnumerator CloseAfterDelay(GameObject panel)
    {
        Animator animator = panel.GetComponent<Animator>();

        animator.SetTrigger("close");

        yield return new WaitForSeconds(closeDelay);

        panel.SetActive(false);
        background.SetActive(false);

        animator.ResetTrigger("close");
    }


    public void ExitGame()
    {
        #if UNITY_EDITOR
        // 유니티 에디터에서 플레이 모드 종료
        EditorApplication.isPlaying = false;
        #else
        // 실제 빌드된 앱 종료
        Application.Quit();
        #endif
    }
}