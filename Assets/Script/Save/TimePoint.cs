using UnityEngine;

public class TimePoint : MonoBehaviour
{
    private bool isUsed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 공주 태그에만 반응
        if (!collision.CompareTag("Princess")) return;
        if (isUsed) return;
        isUsed = true;

        Princess princess = collision.GetComponent<Princess>();
        if (princess == null) return;

        // 플레이어 / PlayerOver
        PlayerOver playerOver = FindFirstObjectByType<PlayerOver>();
        Player player = FindFirstObjectByType<Player>();
        if (player == null || playerOver == null) return;

        // ★ 플레이어가 '행동불능' 상태인 경우 => 부활 절차
        if (playerOver.IsDisabled)
        {
            Debug.Log("[TimePoint] 플레이어가 Disable 상태이므로 '부활' 시퀀스 실행");

            // 1) 체크포인트 저장
            TimePointManager.Instance.SaveCheckpoint(
                princess.transform.position,
                player.transform.position
            );

            // 2) 플레이어를 체크포인트 위치(=공주 위치)로 이동 + 부활(체력 복원)
            //    - ImmediateRevive() 쓰거나, 직접 OnRewindComplete() 호출 등 원하는 방식
            TimePointManager.Instance.ImmediateRevive();

            // 3) 공주를 멈추고, 일정 흐름 후 재개
            StartCoroutine(CoStopPrincessAndResume(princess));
        }
        else
        {
            // ★ 평소대로 체크포인트 저장만
            Debug.Log("[TimePoint] 일반 체크포인트 저장");
            TimePointManager.Instance.SaveCheckpoint(
                princess.transform.position,
                player.transform.position
            );
        }
    }

    /// <summary>
    /// 공주를 잠시 멈추고, 사용자 입력이 들어오면 재개시키는 흐름
    /// </summary>
    private System.Collections.IEnumerator CoStopPrincessAndResume(Princess princess)
    {
        // (선택) 공주 이동 정지
        Rigidbody2D rb = princess.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        // Princess.cs에 public bool stopMovement 추가 가능
        princess.isControlled = true; 
        // → FixedUpdate()에서 isControlled면 이동 안 함

        Debug.Log("[TimePoint] 공주 일시 정지. 아무 키(또는 마우스)를 눌러 재개");

        // 사용자 입력 대기
        while (!Input.anyKeyDown) 
        {
            yield return null;
        }

        // 공주 재개
        princess.isControlled = false;
        Debug.Log("[TimePoint] 공주 이동 재개 완료");
    }
}
