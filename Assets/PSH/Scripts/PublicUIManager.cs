using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Manages opening/closing of UI panels, background overlay, and close delay.
/// </summary>

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


    // Plays close animation, then deactivates panel and background
    private IEnumerator CloseAfterDelay(GameObject panel)
    {
        Animator anim = panel.GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("close");
        yield return new WaitForSeconds(closeDelay);
        panel.SetActive(false);
        background.SetActive(false);
    }


    // Returns to the lobby scene
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