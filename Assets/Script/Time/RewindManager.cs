using UnityEngine;
using System.Collections;

public class RewindManager : MonoBehaviour
{
    private static RewindManager instance;
    private Vector2 savedPrincessPosition;
    private Vector2 savedPlayerPosition;
    private bool hasCheckpoint = false;

    public float rewindDuration = 0.5f; // 되감기 연출 지속 시간 (빠르게)
    
    // StatusTextManager를 통해 메시지 출력 (UI 텍스트)
    // 예: "되감기 포인트 지정됨", "되감기 중", "아무 키나 누르면 시작" 등

    public static RewindManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("RewindManager");
                instance = obj.AddComponent<RewindManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    // 타임 포인트 저장 시 호출
    public void SaveCheckpoint(Vector2 princessPos, Vector2 playerPos)
    {
        savedPrincessPosition = princessPos;
        savedPlayerPosition = playerPos;
        hasCheckpoint = true;
        Debug.Log($"[RewindManager] Checkpoint saved: Princess {princessPos}, Player {playerPos}");
        // UI 메시지 출력: "되감기 포인트 지정됨"
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("되감기 포인트 지정됨");
    }

    public bool HasCheckpoint()
    {
        return hasCheckpoint;
    }

    // 게임오버 시 호출하여 저장된 체크포인트로 즉시 되감기 실행
    public void RewindToCheckpoint()
    {
        if (!hasCheckpoint)
        {
            Debug.LogWarning("[RewindManager] No checkpoint available.");
            return;
        }
        StartCoroutine(RewindCoroutine());
    }

    private IEnumerator RewindCoroutine()
    {
        Debug.Log("[RewindManager] Rewinding...");
        // UI 메시지 출력: "되감기 중"
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("되감기 중...");

        // BGM은 SoundManager에서 따로 관리하므로, 여기서는 효과음 처리 생략

        float elapsed = 0f;
        // 공주와 플레이어 참조
        Princess princess = FindObjectOfType<Princess>();
        Player player = FindObjectOfType<Player>();

        if (princess == null || player == null)
        {
            Debug.LogWarning("[RewindManager] Missing Princess or Player.");
            yield break;
        }

        Vector2 startPrincessPos = princess.transform.position;
        Vector2 startPlayerPos = player.transform.position;

        // 즉시(지연 없이) 되감기 시작: 0.5초 동안 선형 보간
        while (elapsed < rewindDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rewindDuration;
            princess.transform.position = Vector2.Lerp(startPrincessPos, savedPrincessPosition, t);
            player.transform.position = Vector2.Lerp(startPlayerPos, savedPlayerPosition, t);
            yield return null;
        }
        // 강제 보정
        princess.transform.position = savedPrincessPosition;
        player.transform.position = savedPlayerPosition;

        Debug.Log("[RewindManager] Rewind complete.");
        // 되감기 완료 후, 게임오버 상태 유지하며 UI 텍스트로 "아무 키나 누르면 시작" 출력
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("아무 키나 누르면 시작");

        // 게임을 일시정지 상태로 전환
        Time.timeScale = 0f;
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        // 재시작 시점
        Time.timeScale = 1f;
        // UI 메시지 제거
        if (StatusTextManager.Instance != null)
            StatusTextManager.Instance.ShowMessage("");
    }
}
