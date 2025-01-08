using UnityEngine;

public class SniperEnemy : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float bulletSpeed = 10f;
    public float attackRange = 10f;

    private Transform target;
    private Transform princess;
    private Transform player;

    private float nextFireTime;

    private bool isAggroOnPlayer = false; // 플레이어에게 어그로 여부
    public float aggroDuration = 5f; // 플레이어 어그로 지속 시간

    void Start()
    {
        princess = GameObject.FindGameObjectWithTag("Princess").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        target = princess; // 기본 타겟은 공주
    }

    void Update()
    {
        if (target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange && Time.time >= nextFireTime)
        {
            FireAtTarget();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void FireAtTarget()
    {
        if (firePoint == null || bulletPrefab == null) return;

        Vector2 direction = (target.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }

        SoundManager.Instance.PlaySFX("fireSound");
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), GetComponent<Collider2D>());

    }

    public void AggroPlayer()
    {
        if (isAggroOnPlayer) return;

        isAggroOnPlayer = true;
        target = player; // 타겟을 플레이어로 변경
        Invoke(nameof(ResetAggro), aggroDuration); // 지속 시간 후 복구
    }

    private void ResetAggro()
    {
        isAggroOnPlayer = false;
        target = princess; // 타겟을 다시 공주로 변경
    }
}
