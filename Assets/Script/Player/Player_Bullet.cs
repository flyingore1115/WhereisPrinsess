using UnityEngine;

public class Bullet : MonoBehaviour, ITimeAffectable
{
    public float speed = 30f;
    private Rigidbody2D rb;
    private bool isTimeStopped = false;
    private Vector2 direction;
    public int damageAmount = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 등록: TimeStopController에 자신을 등록
        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
        {
            tsc.RegisterTimeAffectedObject(this);
            // ★ 즉시 시간정지 상태 체크: 만약 현재 시간정지 중이면 StopTime() 호출
            if (tsc.IsTimeStopped)
            {
                StopTime();
            }
        }
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        // 발사 방향에 따라 각도 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate()
    {
        if (isTimeStopped) return;
        rb.linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 기존 Destroy 대신, 적의 TakeDamage() 호출
            var enemy = collision.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
            }
            else
            {
                Destroy(collision.gameObject);
            }

            // 총알은 충돌 시 제거
            Destroy(gameObject);

            // 시간 게이지 충전
            TimeStopController timeStopController = FindFirstObjectByType<TimeStopController>();
            if (timeStopController != null)
            {
                timeStopController.AddTimeGauge(5f);
            }
        }
    }

    void OnDestroy()
    {
        // 파괴될 때 TimeStopController 목록에서 제거
        TimeStopController timeStopController = FindFirstObjectByType<TimeStopController>();
        if (timeStopController != null)
        {
            timeStopController.RemoveTimeAffectedObject(this);
        }
    }

    public void StopTime()
    {
        isTimeStopped = true;
        rb.linearVelocity = Vector2.zero; // 속도 0 설정
        rb.simulated = false;       // 물리 시뮬레이션 중단
    }

    public void ResumeTime()
    {
        isTimeStopped = false;
        rb.simulated = true;        // 물리 시뮬레이션 재개
    }

    public void RestoreColor() { } // 총알은 색 변화가 필요 없으므로 빈 구현
}
