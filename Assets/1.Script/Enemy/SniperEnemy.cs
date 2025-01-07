using UnityEngine;

public class SniperEnemy : MonoBehaviour
{
    public GameObject bulletPrefab; // 발사할 총알
    public Transform firePoint; // 총알 발사 위치
    public float fireRate = 2f; // 초당 발사 횟수
    public float bulletSpeed = 10f; // 총알 속도
    public float attackRange = 10f; // 사격 가능한 최대 거리
    public AudioClip fireSound; // 총 발사 사운드 클립

    private AudioSource audioSource; // 오디오 소스 컴포넌트
    private float nextFireTime;
    private Transform princess;

    void Start()
    {
        // 공주 오브젝트를 찾아 Transform 저장
        princess = GameObject.FindGameObjectWithTag("Princess").transform;

        // firePoint 위치가 설정되지 않았을 경우 에러 로그 출력
        if (firePoint == null)
        {
            Debug.LogError("FirePoint가 설정되지 않았습니다!");
        }

        // AudioSource 초기화
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (princess == null)
        {
            Debug.LogWarning("Princess 태그의 게임 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // 공주와의 거리 계산
        float distanceToPrincess = Vector2.Distance(transform.position, princess.position);

        // 공주가 범위 안에 있을 경우에만 사격
        if (distanceToPrincess <= attackRange && Time.time >= nextFireTime)
        {
            FireAtPrincess();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void FireAtPrincess()
    {
        if (princess == null)
        {
            Debug.LogWarning("Princess 태그의 게임 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // 공주를 향한 방향 계산
        Vector2 direction = (princess.position - firePoint.position).normalized;

        // 총알 생성 및 속도 설정
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }
        else
        {
            Debug.LogError("총알 프리팹에 Rigidbody2D가 없습니다!");
        }

        // 총알의 회전 설정 (optional: Sprite를 정렬)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // 총 발사 사운드 재생
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound); // 한 번만 재생
        }
    }
}
