using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PublicUIManager : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private float closeDelay = 0.3f;

    public float CloseDelay => closeDelay;

    private GameObject currentPanel;

    public bool IsAnyPanelOpen() => currentPanel != null;

    public void OpenPanel(GameObject panel)
    {
        if (currentPanel != null) return;
        currentPanel = panel;
        background.SetActive(true);
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (currentPanel == null) return;
        StartCoroutine(CloseAfterDelay(currentPanel));
        currentPanel = null;
    }

    private IEnumerator CloseAfterDelay(GameObject panel)
    {
        Animator anim = panel.GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("close");
        yield return new WaitForSeconds(closeDelay);
        panel.SetActive(false);
        background.SetActive(false);
    }


    // lobby 씬으로 돌아감
    public void ExitGame()
    {
        SceneManager.LoadScene("S0_Lobby");
    }


    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        
        #else
            Application.Quit();
        
        #endif
    }
}