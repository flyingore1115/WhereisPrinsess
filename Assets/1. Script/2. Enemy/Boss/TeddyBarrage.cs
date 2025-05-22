using UnityEngine;
using System.Collections;

public class TeddyBarrage : MonoBehaviour, ITimeAffectable
{
    [Header("Barrage Settings")]
    public float lifetime = 5f;          // 탄막 수명
    public float speed = 10f;            // 탄막 이동 속도

    private Rigidbody2D rb;
    private bool isTimeStopped = false;
    private Vector2 direction;

    [Header("Optional Fire Effect (One-shot)")]
    public GameObject fireEffectPrefab;  // 탄막 발사 시 나타나는 이펙트 (옵션, Inspector에 할당)

    [Header("Continuous Effect")]

    public GameObject continuousEffectPrefab; // 탄막이 존재하는 동안 지속적으로 재생될 이펙트
    
    [Header("Explosion Effect")]
    public GameObject explosionEffectPrefab; // 탄막 파괴 시 폭발 이펙트 (Inspector에 할당)

    private float currentLifetime;
    

    private GameObject continuousEffectInstance; // 생성된 지속 이펙트 인스턴스

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 시간 정지 시스템에 등록
        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
        {
            tsc.RegisterTimeAffectedObject(this);
            if (tsc.IsTimeStopped)
            {
                StopTime();
            }
        }

        currentLifetime = lifetime;
        
        // 발사 효과 실행 (한번만 실행)
        if (fireEffectPrefab != null)
        {
            GameObject effect = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1.5f);
        }
        
        // 지속 효과 생성: 탄막 오브젝트의 자식으로 붙여서, 탄막이 존재하는 동안 계속 재생
        if (continuousEffectPrefab != null)
        {
            continuousEffectInstance = Instantiate(continuousEffectPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    void Update()
{
    if (isTimeStopped) return;

    currentLifetime -= Time.deltaTime;
    if (currentLifetime <= 0f)
    {
        SpawnExplosionEffect();
        Destroy(gameObject);
    }
}


    /// <summary>
    /// 탄막 발사 방향 설정
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate()
    {
        if (isTimeStopped || rb == null) return;
        rb.linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

        // 플레이어에 닿으면 데미지 처리 후 폭발 이펙트 재생
        if (collision.CompareTag("Player"))
        {
            PlayerOver player = collision.GetComponent<PlayerOver>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
            SpawnExplosionEffect();
            Destroy(gameObject);
        }
        // 보스(Enemy 태그)와 충돌하면 무시
        else if (collision.CompareTag("Enemy"))
        {
            //아무일도없었다
        }
        else
        {
            SpawnExplosionEffect();
            Destroy(gameObject);
        }
    }

    private void SpawnExplosionEffect()
    {
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            // 폭발 이펙트 프리팹은 자체 Destroy 로직이 있어야 함
        }
    }

    // ITimeAffectable 구현
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

    public void RestoreColor()
    {
        // 별도 색 복원 로직 없음
    }

    private void OnDestroy()
    {
        // 지속 효과 제거 (필요하면)
        if (continuousEffectInstance != null)
        {
            Destroy(continuousEffectInstance);
        }
        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null)
        {
            tsc.RemoveTimeAffectedObject(this);
        }
    }
}
