using UnityEngine;
using System.Collections;


[RequireComponent(typeof(InteractionIcon))]

public class TriggerTeleportWithFade : MonoBehaviour
{
    InteractionIcon icon;


    [Header("이동 좌표 (새 위치)")]
    public Vector2 targetPosition;

    private bool isPlayerInside = false;
    private bool isTeleporting = false;

    void Awake()
    {
        icon = GetComponent<InteractionIcon>();
        if (icon == null)
        Debug.LogError("[TriggerTeleportWithFade] InteractionIcon 컴포넌트가 없습니다!");
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

    if (Player.Instance != null) //입력 차단
        Player.Instance.ignoreInput = true;

    FadeManager.Instance.StartFadeInOut(() =>
    {
        if (Player.Instance != null)
            Player.Instance.transform.position = targetPosition;
            Player.Instance.movement.ResetInput();
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
            icon.Show();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            icon.Hide();
        }
    }
}
