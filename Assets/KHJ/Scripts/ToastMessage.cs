using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays a brief toast notification with configurable fade-in, hold, and fade-out
/// durations. Interrupts any in-progress toast when a new message arrives.
/// </summary>
public class ToastMessage : MonoBehaviour
{
    [SerializeField] private CanvasGroup toastCanvasGroup;
    [SerializeField] private TMP_Text toastText;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 1f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private Coroutine currentToastCoroutine;

    public void ShowToast(string message)
    {
        toastText.text = message;

        // Stop any running toast so the new message takes over immediately
        if (currentToastCoroutine != null)
        {
            StopCoroutine(currentToastCoroutine);
        }
        toastCanvasGroup.gameObject.SetActive(true);
        currentToastCoroutine = StartCoroutine(ToastAnimation());
    }

    private IEnumerator ToastAnimation()
    {
        float elapsed = 0f;

        // Fade in
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            toastCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        toastCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(displayDuration);

        elapsed = 0f;

        // Fade out
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            toastCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        toastCanvasGroup.alpha = 0f;
        toastCanvasGroup.gameObject.SetActive(false);
    }
}
