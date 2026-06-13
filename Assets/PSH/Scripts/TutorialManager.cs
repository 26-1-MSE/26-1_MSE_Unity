using UnityEngine;


/// <summary>
/// Manages the multi-page tutorial UI, 
/// including page navigation and button visibility.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject[] pages;
    [SerializeField] private GameObject prevButton;
    [SerializeField] private GameObject nextButton;

    private int currentPage = 0;

    private PublicUIManager publicUI;

    private void Start()
    {
        publicUI = GetComponent<PublicUIManager>();
    }

    public void OpenTutorial()
    {
        publicUI.OpenPanel(tutorialPanel);
        ShowPage(0);
    }

    public void CloseTutorial()
    {
        publicUI.ClosePanel();
    }

    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
            ShowPage(currentPage + 1);
    }

    public void PrevPage()
    {
        if (currentPage > 0)
            ShowPage(currentPage - 1);
    }

    // Activates the target page and updates prev/next button visibility
    private void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);
        currentPage = index;

        prevButton.SetActive(currentPage > 0);
        nextButton.SetActive(currentPage < pages.Length - 1);
    }
}