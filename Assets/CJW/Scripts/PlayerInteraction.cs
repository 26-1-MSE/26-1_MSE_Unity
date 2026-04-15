using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentInteractable;
    [SerializeField] private TextMeshProUGUI interactText; 
    [SerializeField] private Camera mainCamera;

    private void Update()
    {
        if (interactText != null && interactText.gameObject.activeSelf)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            interactText.transform.position = screenPos;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact(this);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            if (interactText != null && interactable.GetInteractMessage() != null)
                interactText.gameObject.SetActive(true);
            Debug.Log("��ȣ�ۿ� ������ ������Ʈ ������ ����");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && currentInteractable == interactable)
        {
            currentInteractable = null;
            if (interactText != null)
                interactText.gameObject.SetActive(false);
            Debug.Log("��ȣ�ۿ� ������ ������Ʈ �������� ����");
        }
    }
    public void EndChop()
    {
        GetComponent<Animator>().SetBool("isChopping", false);
    }  
    public void EndScoop()
    {
        GetComponent<Animator>().SetBool("isScooping", false);
    }
}