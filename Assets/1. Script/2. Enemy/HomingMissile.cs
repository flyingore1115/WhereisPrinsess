using UnityEngine;
using MyGame;    // IDamageable 인터페이스가 MyGame 네임스페이스에 정의되어 있다면 필요

public class HomingMissile : MonoBehaviour, ITimeAffectable
{
    private Transform target;

    [Header("Missile Settings")]
    public float speed = 6f;            // 미사일 전진 속도
    public float rotateSpeed = 200f;    // 회전 속도
    public float lifeTime = 6f;         // 생존 시간

    private float elapsedLife = 0f;
    private bool isTimeStopped = false;

    private Rigidbody2D rb;             // 움직임 제어를 위해 Rigidbody2D 사용 (선택적)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 시간 정지 시스템에 등록
        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
        {
            tsc.RegisterTimeAffectedObject(this);
            if (tsc.IsTimeStopped)
                StopTime();
        }
    }

    /// <summary>
    /// 외부에서 타겟을 초기화할 때 호출합니다.
    /// </summary>
    public void Init(Transform target)
    {
        this.target = target;
        elapsedLife = 0f;
    }

    void Update()
    {
        // 시간정지 중이면 이동과 수명 카운팅 모두 중단
        if (isTimeStopped) 
            return;

        // 1) 수명 경과 처리
        elapsedLife += Time.deltaTime;
        if (elapsedLife >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // 2) 타겟 유무 확인
        if (target == null) 
            return;

        // 3) 타겟 방향 계산
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;

        // 4) 회전량 계산 (Z축 기준) 및 회전 적용
        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        transform.Rotate(0f, 0f, -rotateAmount * rotateSpeed * Time.deltaTime);

        // 5) 전진
        transform.Translate(transform.up * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어나 공주 충돌 시
        if (other.CompareTag("Player") || other.CompareTag("Princess"))
        {
            var dmgable = other.GetComponent<IDamageable>();
            dmgable?.Hit(1);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // 시간 정지 시스템에서 제거
        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
            tsc.RemoveTimeAffectedObject(this);
    }

    // ───────────────────────────────────
    // ITimeAffectable 구현
    // ───────────────────────────────────

    public void StopTime()
    {
        isTimeStopped = true;
        if (rb != null)
        {
            // Rigidbody2D가 있다면 물리 시뮬레이션 중단
            rb.simulated = false;
        }
    }

    public void ResumeTime()
    {
        isTimeStopped = false;
        if (rb != null)
        {
            // Rigidbody2D 시뮬레이션 재개
            rb.simulated = true;
        }
    }
}
