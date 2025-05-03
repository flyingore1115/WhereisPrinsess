using UnityEngine;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("페이드 캔버스")]
    public CanvasGroup fadePrefab;
    private CanvasGroup fadeCanvas;
    public float fadeDuration = 0.5f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 프리팹으로부터 인스턴스 생성
            var go = Instantiate(fadePrefab.gameObject);
            fadeCanvas = go.GetComponent<CanvasGroup>();
            go.transform.SetParent(transform, false);
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void StartFadeOut(System.Action onComplete = null)
    {
        StartCoroutine(Fade(0f, 1f, onComplete));
    }

    public void StartFadeIn(System.Action onComplete = null)
    {
        StartCoroutine(Fade(1f, 0f, onComplete));
    }

    public void StartFadeInOut(System.Action midAction = null)
    {
        StartCoroutine(FadeSequence(midAction));
    }

    private IEnumerator Fade(float from, float to, System.Action onComplete)
    {
        if (fadeCanvas == null) yield break;

        fadeCanvas.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeCanvas.alpha = alpha;
            yield return null;
        }

        fadeCanvas.alpha = to;
        if (to == 0f) fadeCanvas.gameObject.SetActive(false);

        onComplete?.Invoke();
    }

    private IEnumerator FadeSequence(System.Action midAction)
    {
        yield return Fade(0f, 1f, null);

        midAction?.Invoke();

        yield return Fade(1f, 0f, null);
    }
}
