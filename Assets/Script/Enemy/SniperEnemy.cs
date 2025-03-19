using UnityEngine;

public class SniperEnemy : BaseEnemy
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float bulletSpeed = 10f;
    public float attackRange = 10f;

    private float nextFireTime;

    void Update()
    {
        if (isTimeStopped) return;

        float distanceToTarget = Vector2.Distance(transform.position, princess.position);

        if (distanceToTarget <= attackRange && Time.time >= nextFireTime)
        {
            FireAtTarget();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void FireAtTarget()
    {
        if (firePoint == null || bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = (princess.position - firePoint.position).normalized * bulletSpeed;
        }
    }
}
