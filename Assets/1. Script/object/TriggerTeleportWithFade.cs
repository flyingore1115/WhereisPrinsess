// TriggerTeleportWithFade.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(InteractionUIController))]
public class TriggerTeleportWithFade : MonoBehaviour
{
    private InteractionUIController ui;

    [Header("이동 좌표 (새 위치)")]
    public Vector2 targetPosition;

    [Tooltip("인터랙션 시 표시할 행동 텍스트 (예: '이동')")]
    public string actionDescription = "이동";

    private bool isPlayerInside = false;
    private bool isTeleporting = false;

    void Awake()
    {
        ui = GetComponent<InteractionUIController>();
        if (ui == null)
            Debug.LogError($"[TriggerTeleportWithFade] InteractionUIController 컴포넌트를 찾을 수 없습니다: {name}");
    }

    void Start()
    {
        // 처음에는 UI 숨김
        ui.Hide();
    }

    void Update()
    {
        if (isPlayerInside && !isTeleporting && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(TeleportWithFade());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = true;
        ui.Show("E", actionDescription);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = false;
        ui.Hide();
    }

    private IEnumerator TeleportWithFade()
    {
        isTeleporting = true;

        if (Player.Instance != null)
            Player.Instance.ignoreInput = true;

        FadeManager.Instance.StartFadeInOut(() =>
        {
            if (Player.Instance != null)
            {
                Player.Instance.transform.position = targetPosition;
                Player.Instance.movement.ResetInput();
            }
        });

        // 페이드 인·아웃이 두 배 딜레이 만큼 걸리므로
        yield return new WaitForSeconds(FadeManager.Instance.fadeDuration * 2f);

        if (Player.Instance != null)
            Player.Instance.ignoreInput = false;

        isTeleporting = false;
    }
}
