using UnityEngine;
using System.Collections;

/// <summary>
/// 보스가 발사하는 **탄막(Bullet) 프리팹** 스크립트  
/// • 지정 방향‧속도로 이동  
/// • 벽에 1회 반사(튕김) → 2번째 충돌 시 파괴  
/// • 시간 정지 대응
/// </summary>
public class TeddyBarrage : MonoBehaviour, ITimeAffectable
{
    /*──────────────────────────────────────────────
     * ▣ 인스펙터 노출 변수
     *─────────────────────────────────────────────*/

    [Header("Barrage Settings")]
    [Tooltip("탄막 존재 시간(초)")]
    public float lifetime = 5f;

    [Tooltip("탄막 이동 속도")]
    public float speed = 10f;

    [Header("Optional Effects")]
    public GameObject fireEffectPrefab;        // 발사 순간 효과
    public GameObject continuousEffectPrefab;  // 비행 중 지속 효과
    public GameObject explosionEffectPrefab;   // 파괴 시 폭발 효과

    /*──────────────────────────────────────────────
     * ▣ 내부 상태 변수
     *─────────────────────────────────────────────*/

    private Rigidbody2D rb;          // 이동용 Rigidbody
    private bool        isTimeStopped = false;
    private Vector2     direction;   // 이동 방향(정규화)
    private int         bounceCount  = 0;  // 벽 반사 횟수
    private GameObject  continuousEffectInstance;

    /*──────────────────────────────────────────────
     * ▣ Unity 생명주기
     *─────────────────────────────────────────────*/

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 시간 정지 시스템 등록
        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
        {
            tsc.RegisterTimeAffectedObject(this);
            if (tsc.IsTimeStopped) StopTime();
        }

        // 발사 이펙트(1회)
        if (fireEffectPrefab != null)
        {
            var fx = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1.5f);
        }

        // 지속 이펙트(자식으로 부착)
        if (continuousEffectPrefab != null)
        {
            continuousEffectInstance = Instantiate(
                continuousEffectPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    void Update()
    {
        if (isTimeStopped) return;

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            SpawnExplosionEffect();
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (isTimeStopped || rb == null) return;
        rb.linearVelocity = direction * speed;
    }

    /*──────────────────────────────────────────────
     * ▣ 설정 & 충돌 처리
     *─────────────────────────────────────────────*/

    /// <summary>발사 방향 설정(외부 호출)</summary>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>Trigger 충돌(플레이어 등)</summary>
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            col.GetComponent<PlayerOver>()?.TakeDamage(1);
            SpawnExplosionEffect();
            Destroy(gameObject);
        }
        else if (col.CompareTag("Enemy"))
        {
            // 보스 자신과 충돌은 무시
        }
    }

    /// <summary>실제 물리 충돌(벽) 처리 → 반사 1회</summary>
    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Wall")) return;

        if (bounceCount == 0)
        {
            // 첫 충돌: 반사
            Vector2 normal = col.contacts[0].normal;
            direction = Vector2.Reflect(direction, normal).normalized;
            rb.linearVelocity = direction * speed;
            bounceCount++;
        }
        else
        {
            // 두 번째 충돌: 파괴
            SpawnExplosionEffect();
            Destroy(gameObject);
        }
    }

    /*──────────────────────────────────────────────
     * ▣ 보조 메서드
     *─────────────────────────────────────────────*/

    /// <summary>폭발 이펙트 생성</summary>
    private void SpawnExplosionEffect()
    {
        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
    }

    private void OnDestroy()
    {
        if (continuousEffectInstance != null)
            Destroy(continuousEffectInstance);

        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null) tsc.RemoveTimeAffectedObject(this);
    }

    /*──────────────────────────────────────────────
     * ▣ ITimeAffectable 구현
     *─────────────────────────────────────────────*/

    public void StopTime()
    {
        isTimeStopped = true;
        if (rb != null)
        {
            rb.linearVelocity  = Vector2.zero;
            rb.simulated = false;
        }
    }

    public void ResumeTime()
    {
        isTimeStopped = false;
        if (rb != null) rb.simulated = true;
    }
}
