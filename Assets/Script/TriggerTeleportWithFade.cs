using UnityEngine;
using System.Collections;

public class TriggerTeleportWithFade : MonoBehaviour
{
    [Header("이동 좌표 (새 위치)")]
    public Vector2 targetPosition;

    [Header("페이드 효과 컨트롤")]
    public CanvasGroup fadeCanvas; // UI Canvas에 있는 페이드용 이미지 (알파 조절)
    public float fadeDuration = 0.5f;

    private bool isPlayerInside = false;
    private bool isTeleporting = false;

    private void Start()
    {
        if (fadeCanvas != null)
            fadeCanvas.gameObject.SetActive(false); // 시작 시 비활성화
    }

    private void Update()
    {
        if (isPlayerInside && !isTeleporting && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(TeleportWithFade());
        }
    }

    private IEnumerator TeleportWithFade()
    {
        isTeleporting = true;

        // 입력 차단
        if (Player.Instance != null)
            Player.Instance.ignoreInput = true;

        if (fadeCanvas != null)
        fadeCanvas.gameObject.SetActive(true); // 시작 시 켜기

        // 페이드 아웃
        yield return StartCoroutine(Fade(0f, 1f));

        // 텔레포트 수행
        if (Player.Instance != null)
        {
            Player.Instance.transform.position = targetPosition;
        }

        // 페이드 인
        yield return StartCoroutine(Fade(1f, 0f));

        // 입력 복원
        if (Player.Instance != null)
            Player.Instance.ignoreInput = false;

        isTeleporting = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeCanvas == null)
        {
            Debug.LogWarning("[TriggerTeleportWithFade] 페이드 캔버스가 없습니다.");
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeCanvas.alpha = alpha;
            yield return null;
        }

        fadeCanvas.alpha = to;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }
}
