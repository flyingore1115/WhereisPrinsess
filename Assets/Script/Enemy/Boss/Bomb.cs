using UnityEngine;


public class Bomb : MonoBehaviour
{
    public float fallSpeed = 3f;
    public float explodeRadius = 2f;

    private float lifetime = 5f;
    private Rigidbody2D rb;
    private bool isTimeStopped = false;

    public GameObject explosionParticlePrefab;  //폭발 이펙트

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 폭발 처리
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        foreach (var c in hits)
        {
            if (c.CompareTag("Player"))
            {
                PlayerOver player = c.GetComponent<PlayerOver>();
                player.TakeDamage(1);
            }
        }
        if (explosionParticlePrefab != null)
        {
            GameObject effect = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            
            // 파티클 정렬 설정 (플레이어보다 앞으로 나오게)
            ParticleSystemRenderer psr = effect.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sortingOrder = 100;
            }
        }
        Destroy(gameObject);
    }

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
