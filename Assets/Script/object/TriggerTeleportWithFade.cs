using UnityEngine;
using System.Collections;

public class TriggerTeleportWithFade : MonoBehaviour
{
    [Header("이동 좌표 (새 위치)")]
    public Vector2 targetPosition;

    private bool isPlayerInside = false;
    private bool isTeleporting = false;

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

    if (Player.Instance != null) //입력 차단
        Player.Instance.ignoreInput = true;

    FadeManager.Instance.StartFadeInOut(() =>
    {
        if (Player.Instance != null)
            Player.Instance.transform.position = targetPosition;
    });

    // 딜레이 끝나기까지 대기
    yield return new WaitForSeconds(FadeManager.Instance.fadeDuration * 2f);

    if (Player.Instance != null)
        Player.Instance.ignoreInput = false;

    isTeleporting = false;
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
