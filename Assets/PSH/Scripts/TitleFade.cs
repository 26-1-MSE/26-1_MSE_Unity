using UnityEngine;
using System.Collections;


/// <summary>
/// Fades a title UI in, holds it, then fades out and deactivates it.
/// </summary>

public class TitleFade : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float displayDuration = 3f;

    private void Start()
    {
        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        // fade in
        yield return StartCoroutine(Fade(0f, 1f));
        // hold it
        yield return new WaitForSeconds(displayDuration);
        // fade out
        yield return StartCoroutine(Fade(1f, 0f));

        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}