using UnityEngine;
using System.Collections;
using MyGame;  // ITimeAffectable 인터페이스가 정의된 네임스페이스

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class HallwayObstacle : MonoBehaviour, ITimeAffectable, IDamageable
{
    SpriteRenderer spriteRenderer;
    Collider2D col;
    bool isDeactivated = false;
    public void Hit(int damage) => Deactivate();
    

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void Deactivate()
    {
        if (isDeactivated) return;
        isDeactivated = true;

        // 보이던 그래픽·충돌 비활성화
        spriteRenderer.enabled = false;
        col.enabled = false;

        // 되감기 시작 → 끝날 때까지 대기
        StartCoroutine(ReactivateAfterRewind());
    }

    IEnumerator ReactivateAfterRewind()
    {
        // 되감기 시작 대기
        while (RewindManager.Instance == null || !RewindManager.Instance.IsRewinding)
            yield return null;
        // 되감기 진행 중 대기
        while (RewindManager.Instance.IsRewinding)
            yield return null;

        // 되감기 끝나면 자동 복원
        spriteRenderer.enabled = true;
        col.enabled = true;
        isDeactivated = false;
    }

    // 시간정지 중 회색조 효과를 적용해야 하면 구현
    public void StopTime()
    {
        // Obstacle에는 특별한 처리 없음
    }

    public void ResumeTime()
    {
        // Obstacle에는 특별한 처리 없음
    }
}
