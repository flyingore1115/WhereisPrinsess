using UnityEngine;
using System.Collections;

public class Teddy : BaseEnemy
{
    [Header("Boss Stats")]
    public int bossMaxHP = 20;             // 보스 체력
    public float moveSpeed = 2f;           // 보스 이동 속도

    [Header("Bomb Pattern Settings")]
    public float bombInterval = 4f;          // 폭탄 투하 주기
    public float bombSpawnY = 8f;            // 폭탄 소환 Y좌표
    public Vector2 bombXRange = new Vector2(-6f, 6f); // 폭탄 X좌표 범위
    public int bombCount = 3;                // 한번에 떨어질 폭탄 개수

    [Header("Barrage Pattern Settings")]
    public float bulletInterval = 6f;        // 원형 탄막 주기
    public int bulletCount = 8;              // 원형 탄막 발사 개수
    public float bulletSpeed = 3f;           // 탄 속도
    public Vector2 bulletCenter = new Vector2(0f, 0f); // 탄막 발사 위치 (월드 좌표)

    [Header("Explosion Pattern Settings")]
    public GameObject explosionWarningPrefab; // 빨간 범위 예고 프리팹
    public GameObject explosionEffectPrefab;  // 폭발 이펙트 프리팹
    public float explosionRadius = 2f;        // 폭발 범위 보여주기
    public float explosionDelay = 1.5f;       // 예고 후 실제 폭발까지 딜레이

    public Vector2[] explosionPositions = new Vector2[3]; //폭발 지점

    [Header("References")]
    public BossHealthUI bossHealthUI;
    public GameObject bombPrefab;
    public GameObject bulletPrefab;

    private bool phase2Started = false;
    private float moveDir = 1f;

    protected override void Awake()
    {
        base.Awake();
        maxHealth = bossMaxHP;
        currentHealth = bossMaxHP;
        UpdateHealthDisplay();
    }

    void Start()
    {
        if (bossHealthUI != null)
        {
            bossHealthUI.InitBossUI("테디", bossMaxHP);
        }
        StartCoroutine(MainPatternLoop());
    }

    void Update()
    {
        BasicMovement();

        // 체력 30% 이하 -> Phase2
        if (!phase2Started && currentHealth <= bossMaxHP * 0.3f)
        {
            phase2Started = true;
            Debug.Log("Teddy Boss Phase2 started!");
            StartCoroutine(SpecialPattern());
        }
    }

    private void BasicMovement()
    {
        if (transform.position.x > 7f) moveDir = -1f;
        else if (transform.position.x < -7f) moveDir = 1f;
        transform.Translate(Vector2.right * moveDir * moveSpeed * Time.deltaTime);
    }

    private IEnumerator MainPatternLoop()
    {
        while (!isDead)
        {
            // 폭탄 투하 (랜덤X, 여러개)
            DropMultipleBombs();
            yield return new WaitForSeconds(bombInterval);

            // 원형 탄막 발사
            FireRadialBullets();
            yield return new WaitForSeconds(bulletInterval);

            // (원한다면 폭발 예고-폭발도 여기에 추가 가능)
        }
    }

    /// <summary>
    /// [1] 폭탄 여러개를 임의 X범위에서 떨어뜨리는 함수
    /// </summary>
    private void DropMultipleBombs()
    {
        if (bombPrefab == null) return;

        for (int i = 0; i < bombCount; i++)
        {
            float randomX = Random.Range(bombXRange.x, bombXRange.y);
            Vector2 spawnPos = new Vector2(randomX, bombSpawnY);
            Instantiate(bombPrefab, spawnPos, Quaternion.identity);
        }
    }

    /// <summary>
    /// [2] 원형 탄막 발사 (임의 위치 bulletCenter에서)
    /// </summary>
    private void FireRadialBullets()
    {
        if (bulletPrefab == null) return;

        float angleStep = 360f / bulletCount;
        float angle = 0f;
        Vector2 spawnCenter = bulletCenter; // 월드 좌표
        for (int i = 0; i < bulletCount; i++)
        {
            float rad = Mathf.Deg2Rad * angle;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // 탄막 생성
            GameObject bullet = Instantiate(bulletPrefab, spawnCenter, Quaternion.identity);
            TeddyBarrage tb = bullet.GetComponent<TeddyBarrage>();
            if (tb != null)
            {
                tb.SetDirection(dir);
                tb.speed = bulletSpeed; // 추가: Inspector보다 이쪽이 우선이라면
            }
            angle += angleStep;
        }
    }

    /// <summary>
    /// [3] 폭발(범위 예고 -> 폭발 이펙트) 예시
    ///  x,y 인스펙터 or 계산으로 받아서 호출하면 됨
    /// </summary>
    /// 
    private IEnumerator ShuffleAndExplode()
    {
        if (explosionPositions.Length < 2)
        {
            Debug.LogWarning("[Teddy] 폭발 좌표가 2개 이상 필요합니다.");
            yield break;
        }

        // 인덱스를 섞어 랜덤 순서 결정
        int[] indices = { 0, 1, 2 };
        for (int i = 0; i < indices.Length; i++)
        {
            int j = Random.Range(i, indices.Length);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        // 앞 2개를 순차 폭발
        yield return StartCoroutine(ExplodeAtPosition(explosionPositions[indices[0]]));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(ExplodeAtPosition(explosionPositions[indices[1]]));
    }

    private IEnumerator ExplodeAtPosition(Vector2 pos)
    {
        Debug.LogWarning("폭발 실행됨=========================");
        // 빨간 범위 예고
        if (explosionWarningPrefab != null)
        {
            GameObject warning = Instantiate(explosionWarningPrefab, pos, Quaternion.identity);
            // 예: 원 크기 explosionRadius 반영(Scale 등)
            warning.transform.localScale = new Vector3(explosionRadius*2, explosionRadius*2, 1f);
            // warning 파괴 시점을 explosionDelay 이후로
            Destroy(warning, explosionDelay);
        }

        // 기다린 뒤 폭발 이펙트
        yield return new WaitForSeconds(explosionDelay);

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, pos, Quaternion.identity);
        }

        // 실제 피해 처리(OverlapCircleAll)
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, explosionRadius);
        foreach (var c in hits)
        {
            if (c.CompareTag("Player"))
            {
                // c.GetComponent<PlayerOver>()?.TakeDamage(1);
            }
        }
        Debug.Log($"[Teddy] Explosion at {pos} done!");
    }

    private IEnumerator SpecialPattern()
    {
        while (!isDead && currentHealth <= bossMaxHP * 0.3f)
        {
            // 폭탄 2연타
            DropMultipleBombs();
            yield return new WaitForSeconds(1.5f);
            DropMultipleBombs();
            yield return new WaitForSeconds(bombInterval);

            // 탄막 2연타
            FireRadialBullets();
            yield return new WaitForSeconds(0.5f);
            FireRadialBullets();
            yield return new WaitForSeconds(bulletInterval);

            // 폭발 패턴
            yield return StartCoroutine(ShuffleAndExplode());
            yield return new WaitForSeconds(2f);
        }
    }


    // 데미지 처리
    public override void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        if (bossHealthUI != null)
            bossHealthUI.UpdateHP(currentHealth);

        Debug.Log($"{gameObject.name} took damage {damage}, HP = {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log("Teddy Boss defeated!");
        // 보스 사망 후 추가 효과
    }
}
