using UnityEngine;

public class TimePoint : MonoBehaviour
{
    private bool isUsed = false;

    // TimePoint.cs  ▸ OnTriggerEnter2D() 내부만 교체
private void OnTriggerEnter2D(Collider2D collision)
{
    // Princess 태그가 아니라면 무시
    if (!collision.CompareTag("Princess") || isUsed) return;
    isUsed = true;

    // ① Princess 또는 Lady 컴포넌트 찾기
    Princess princess = collision.GetComponent<Princess>();
    Lady     lady     = collision.GetComponent<Lady>();

    // 둘 다 없으면 처리 불가
    Transform princessTf = (princess != null) ? princess.transform
                          : (lady     != null) ? lady.transform
                          : null;
    if (princessTf == null) return;

    // Player & PlayerOver
    PlayerOver pOver = FindFirstObjectByType<PlayerOver>();
    Player     player = FindFirstObjectByType<Player>();
    if (player == null || pOver == null) return;

    // ② 플레이어가 쓰러져 있으면 ‘부활’ 시퀀스
    if (pOver.IsDisabled)
    {
        Debug.Log("[TimePoint] 플레이어 Disable ▸ 부활용 체크포인트 저장");
        TimePointManager.Instance.SaveCheckpoint(
            princessTf.position, player.transform.position);

        if (RewindManager.Instance != null)
{
    RewindManager.Instance.StartRewind();   // 스냅샷 역재생+체크포인트 복원
}
        StartCoroutine(CoStopPrincessAndResume((MonoBehaviour)(object)(princess ?? (object)lady)));
    }
    else
    {
        // ③ 일반 체크포인트 저장
        Debug.Log("[TimePoint] 일반 체크포인트 저장");
        TimePointManager.Instance.SaveCheckpoint(
            princessTf.position, player.transform.position);
    }
}

    /// <summary>
    /// 공주를 잠시 멈추고, 사용자 입력이 들어오면 재개시키는 흐름
    /// </summary>
    private System.Collections.IEnumerator CoStopPrincessAndResume(MonoBehaviour target)
{
    Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
    if (rb != null) rb.linearVelocity = Vector2.zero;

    // 공주 or 아가씨 모두 isControlled=true 속성을 공유하게 만들어야 함
    if (target is Princess p)
        p.isControlled = true;
    else if (target is Lady l)
        l.isControlled = true;

    Debug.Log("[TimePoint] 대상 일시 정지. 아무 키 입력 대기 중...");

    while (!Input.anyKeyDown)
        yield return null;

    if (target is Princess pp)
        pp.isControlled = false;
    else if (target is Lady ll)
        ll.isControlled = false;

    Debug.Log("[TimePoint] 이동 재개됨.");
}

}
