using UnityEngine;

public class SniperEnemy : BaseEnemy
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f; // 초당 발사 횟수
    public float bulletSpeed = 10f;
    public float attackRange = 10f;
    public float firePointOffset = 1.0f; // 적 중심으로부터 firePoint의 거리

    private float nextFireTime;

    void Update()
    {
        if (isTimeStopped) return;
        
        // 어그로 상태일 경우 플레이어를, 아닐 경우 공주를 타겟으로 사용
        Transform target = (isAggroOnPlayer && player != null) ? player : princess;
        if (target == null) return;

        // firePoint 위치 및 회전 업데이트 (타겟에 따라)
        UpdateFirePointPosition(target);

        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget <= attackRange && Time.time >= nextFireTime)
        {
            FireAtTarget(target);
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    // 타겟을 인자로 받아 firePoint를 업데이트
    private void UpdateFirePointPosition(Transform target)
    {
        if (firePoint == null || target == null) return;

        // 적의 위치에서 타겟 방향으로 offset 거리만큼 이동
        Vector2 direction = (target.position - transform.position).normalized;
        firePoint.position = transform.position + (Vector3)(direction * firePointOffset);

        // 회전 업데이트 : firePoint가 타겟을 바라보도록
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);

        // 필요시, 적의 flip 설정 (예: 타겟이 왼쪽에 있으면 flipX=true)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (target.position.x < transform.position.x);
        }
    }

    // 타겟을 인자로 받아 총알 발사
    private void FireAtTarget(Transform target)
    {
        if (firePoint == null || bulletPrefab == null || target == null) return;

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 direction = (target.position - firePoint.position).normalized;
            rb.linearVelocity = direction * bulletSpeed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
