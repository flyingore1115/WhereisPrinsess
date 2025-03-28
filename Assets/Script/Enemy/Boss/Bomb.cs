using UnityEngine;

public class Bomb : MonoBehaviour, ITimeAffectable
{
    public float fallSpeed = 3f;
    public float explodeRadius = 2f;
    private float lifetime = 5f;

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
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 만약 Bomb이 transform.Translate(...)로 떨어진다면,
        // isTimeStopped 상태일 때는 동작하지 않도록:
        if (!isTimeStopped)
        {
            // 또는 rigidbody2D.velocity를 쓰셔도 됩니다.
            transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
        }
    }

    // 폭발 처리 (OnTriggerEnter2D 예시, OnCollisionEnter2D 등 상황에 맞게)
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 폭발 범위 피해
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        foreach (var c in hits)
        {
            if (c.CompareTag("Player"))
            {
                // c.GetComponent<PlayerOver>()?.TakeDamage(1);
            }
        }

        // 폭발 이펙트
        if (explosionParticlePrefab != null)
        {
            GameObject effect = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            // 필요하면 파티클 렌더링 순서 수정
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
}
