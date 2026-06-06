using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PublicUIManager : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private float closeDelay = 0.3f;

    private GameObject currentPanel;

    private void Start()
    {
        currentPanel = null;
    }

    public void OpenPanel(GameObject panel)
    {
        if (currentPanel != null) return; // 이미 열려있으면 무시하고 다른 패널 안열리게 하는것
        currentPanel = panel;
        background.SetActive(true);
        panel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        currentPanel = null;
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

        SceneManager.LoadScene("S0_Lobby");
    }

    public bool IsAnyPanelOpen()
    {
        Debug.Log("currentPanel: " + currentPanel);
        return currentPanel != null;
    }

    public void SetCurrentPanel(GameObject panel)
    {
        currentPanel = panel;
    }

    public void ClearCurrentPanel()
    {
        currentPanel = null;
    }
}