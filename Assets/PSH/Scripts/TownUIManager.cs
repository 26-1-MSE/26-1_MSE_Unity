using UnityEngine;
using System.Collections;

public class TownUIManager : MonoBehaviour
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
}