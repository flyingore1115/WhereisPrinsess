using UnityEngine;

public class Bomb : MonoBehaviour, ITimeAffectable
{
    [Header("Bomb Settings")]
    public float fallSpeed = 3f;
    public float explodeRadius = 2f;

    private Rigidbody2D rb;
    private bool isTimeStopped = false;

    public GameObject explosionParticlePrefab;  // 폭발 이펙트

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 시간 정지 컨트롤러에 등록
        var tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
        {
            tsc.RegisterTimeAffectedObject(this);
        }
    }

    void Start()
    {
        // ● 자동 Destroy 기능 제거:
        // Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 시간정지 중이 아니면 낙하 속도 적용
        if (!isTimeStopped)
        {
            transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 폭발 범위 내 데미지 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        foreach (var c in hits)
        {
            if (c.CompareTag("Player"))
            {
                c.GetComponent<PlayerOver>()?.TakeDamage(1);
            }
            else if (c.CompareTag("Princess"))
            {
                c.GetComponent<Princess>()?.TakeDamage(1); // 공주에게도 데미지 적용
            }
        }

        // 폭발 이펙트 생성
        if (explosionParticlePrefab != null)
        {
            GameObject effect = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            // 필요하다면 파티클 크기나 순서를 조정
        }

        Destroy(gameObject);
    }

    // ───────────────────────────────────
    // ■ ITimeAffectable 구현
    // ───────────────────────────────────
    public void StopTime()
    {
        isTimeStopped = true;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
    }

    public void ResumeTime()
    {
        isTimeStopped = false;
        if (rb != null)
        {
            rb.simulated = true;
        }
    }

    /// <summary>
    /// 씬 뷰에서 폭발 반경 시각화 (오브젝트 선택 시에만 원 그리기)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }
}
