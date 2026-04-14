using UnityEngine;
using System.Collections;

public class IslandUIManager : MonoBehaviour
{
    
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject settingUI;

    private Animator animator;


    private void Awake()
    {
        animator = settingUI.GetComponent<Animator>();

    }

    public void OpenPanel()
    {
        background.SetActive(true);
        settingUI.SetActive(true);
    }

    public void CloseAll()
    {
        background.SetActive(false);      
        StartCoroutine(CloseAfterDelay());
    }

    IEnumerator CloseAfterDelay()
    {
        //panel.GetComponent<Animator>().SetTrigger("close");
        animator.SetTrigger("close");
        yield return new WaitForSeconds(0.3f); 

        settingUI.SetActive(false);
        //background.SetActive(false);


        animator.ResetTrigger("close");
    }

}
