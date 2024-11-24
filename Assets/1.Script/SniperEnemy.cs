using UnityEngine;

public class SniperEnemy : MonoBehaviour
{
    public GameObject bulletPrefab; // 불렛 프리팹
    public Transform firePoint; // 불렛 생성 위치
    public float fireRate = 2f; // 초당 발사 횟수
    public float bulletSpeed = 10f; // 불렛 속도
    private float nextFireTime;
    private Transform princess;

    void Start()
    {
        princess = GameObject.FindGameObjectWithTag("Princess").transform;

        // firePoint 위치와 방향 확인
        if (firePoint == null)
        {
            Debug.LogError("FirePoint가 설정되지 않았습니다!");
        }
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            FireAtPrincess();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void FireAtPrincess()
    {
        if (princess == null)
        {
            Debug.LogWarning("Princess 태그를 가진 오브젝트를 찾을 수 없습니다.");
            return;
        }

        Vector2 direction = (princess.position - firePoint.position).normalized;

        // 불렛 생성 및 방향 설정
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }
        else
        {
            Debug.LogError("불렛 프리팹에 Rigidbody2D가 없습니다!");
        }

        // 불렛 회전 설정 (optional: 불렛의 Sprite가 방향을 맞추도록)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
