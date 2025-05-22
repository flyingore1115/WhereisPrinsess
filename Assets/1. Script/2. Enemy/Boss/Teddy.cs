using UnityEngine;
using System.Collections;

public class Teddy : BaseEnemy
{
    [Header("Boss Stats")]
    public int bossMaxHP = 20;
    public float moveSpeed = 2f;

    [Header("Cycle Settings")]
    public float cycleIdleTime = 2f; 
    public float alertAnimTime = 1f; 

    [Header("Bomb Pattern Settings")]
    public float bombSpawnY = 8f;     
    public Vector2 bombXRange = new Vector2(-6f, 6f);
    public int bombCountNormal = 3;   
    public int bombCountSpecial = 5;  

    [Header("Barrage Pattern Settings")]
    public int bulletCountNormal = 8; 
    public int bulletCountSpecial = 12;
    public float bulletSpeed = 3f;
    public Vector2 bulletCenter;      

    [Header("Explosion Pattern Settings")]
    public GameObject explosionWarningPrefab;
    public GameObject explosionEffectPrefab;
    public float explosionRadius = 2f;
    public float explosionDelay = 1.5f;
    public Vector2[] explosionPositions;

    [Header("References")]
    public BossHealthUI bossHealthUI;
    public GameObject bombPrefab;
    public GameObject bulletPrefab;

    private bool isPhase2 = false; 
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
        if (Princess.Instance != null)
        {
            Princess.Instance.PlayScaredIdle();
        }
        if (bossHealthUI != null)
            bossHealthUI.InitBossUI("테디", bossMaxHP);

        var intro = FindFirstObjectByType<BossIntroUI>();
        if (intro != null)
        {
            intro.OnIntroEnd = () =>
            {
                StartCoroutine(MainPatternLoop());
            };
        }
        else
        {
            // 인트로가 없으면 바로 시작
            StartCoroutine(MainPatternLoop());
        };
    }

    void Update()
    {
        // 시간이 멈췄으면 이동 정지
        if (isTimeStopped) return;

        BasicMovement();

        // HP 30% 이하 -> phase2
        if (!isPhase2 && currentHealth <= bossMaxHP * 0.3f)
        {
            isPhase2 = true;
            Debug.Log("[Teddy] Phase2 activated!");
        }
    }

    private void BasicMovement()
    {
        // 간단 좌우 이동
        if (transform.position.x > 7f) moveDir = -1f;
        else if (transform.position.x < -7f) moveDir = 1f;
        transform.Translate(Vector2.right * moveDir * moveSpeed * Time.deltaTime);
    }

    // ─────────────────────────────────────
    // 메인 패턴 루프
    // ─────────────────────────────────────
    private IEnumerator MainPatternLoop()
    {
        while (!isDead)
        {
            // 한 사이클
            yield return StartCoroutine(DoOneCycle(isPhase2));

            // 사이클이 끝나면 대기
            yield return StartCoroutine(WaitWhileTimeStopped());
            yield return new WaitForSeconds(cycleIdleTime);
        }
    }

    // ─────────────────────────────────────
    // 한 사이클: (경고) -> 폭발 -> 폭탄 -> 탄막 ...
    // ─────────────────────────────────────
    private IEnumerator DoOneCycle(bool special)
    {
        //패턴 시작 전 “경고 모션” 애니메이션
        yield return StartCoroutine(PlayAlertAnimation());

        // ■ (2) 폭발
        yield return StartCoroutine(ExplodeOnce(special));

        //패턴 시작 전 “경고 모션” 애니메이션
        yield return StartCoroutine(PlayAlertAnimation());

        // ■ (3) 폭탄
        int bombs = special ? bombCountSpecial : bombCountNormal;
        yield return StartCoroutine(DropMultipleBombs(bombs));

        //패턴 시작 전 “경고 모션” 애니메이션
        yield return StartCoroutine(PlayAlertAnimation());

        // ■ (4) 탄막
        int bulletCount = special ? bulletCountSpecial : bulletCountNormal;
        yield return StartCoroutine(FireRadialBullets(bulletCount));
    }

    /// <summary>
    /// (1) 경고 애니메이션 재생
    /// </summary>
    private IEnumerator PlayAlertAnimation()
    {
        // 시간 멈춰 있으면 풀릴 때까지 대기
        yield return StartCoroutine(WaitWhileTimeStopped());

        // 애니메이터에 Trigger를 주거나, 상태 전환
        if (animator != null && !string.IsNullOrEmpty("Warning"))
        {
            Debug.Log("[Teddy] Alert animation start");
            animator.SetTrigger("Warning");
        }

        // 경고 모션 시간 대기
        float t = 0f;
        while (t < alertAnimTime)
        {
            // 시간정지면 그만 진행
            if (TimeStopController.Instance != null && TimeStopController.Instance.IsTimeStopped)
            {
                // 시간정지 해제 대기
                yield return StartCoroutine(WaitWhileTimeStopped());
            }

            t += Time.deltaTime;
            yield return null;
        }
    }

    // ─────────────────────────────────────
    // (2) 폭발
    // ─────────────────────────────────────
    private IEnumerator ExplodeOnce(bool special)
    {
        if (explosionPositions == null || explosionPositions.Length == 0)
            yield break;

        yield return StartCoroutine(WaitWhileTimeStopped());

        int index = Random.Range(0, explosionPositions.Length);
        Vector2 pos = explosionPositions[index];

        // 예고 표시
        if (explosionWarningPrefab != null)
        {
            var warning = Instantiate(explosionWarningPrefab, pos, Quaternion.identity);
            warning.transform.localScale = new Vector3(explosionRadius * 2, explosionRadius * 2, 1f);
            Destroy(warning, explosionDelay);
        }

        // 예고 후 폭발
        yield return new WaitForSeconds(explosionDelay);

        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, pos, Quaternion.identity);
            effect.transform.localScale = Vector3.one * (explosionRadius * 2f * 0.3f);
        }


        // 피해 처리
        var hits = Physics2D.OverlapCircleAll(pos, explosionRadius);
        foreach (var c in hits)
        {
            if (c.CompareTag("Player"))
            {
                 c.GetComponent<PlayerOver>()?.TakeDamage(1);
            }
        }
        yield return null;
    }

    // ─────────────────────────────────────
    // (3) 폭탄 여러개 투하
    // ─────────────────────────────────────
    private IEnumerator DropMultipleBombs(int count)
    {
        if (bombPrefab == null) yield break;

        for (int i = 0; i < count; i++)
        {
            // 시간 정지 중이면 대기
            yield return StartCoroutine(WaitWhileTimeStopped());

            float randomX = Random.Range(bombXRange.x, bombXRange.y);
            Vector2 spawnPos = new Vector2(randomX, bombSpawnY);
            Instantiate(bombPrefab, spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(1f);
        }
    }

    // ─────────────────────────────────────
    // (4) 탄막 발사
    // ─────────────────────────────────────
    private IEnumerator FireRadialBullets(int bulletCount)
    {
        if (bulletPrefab == null) yield break;

        yield return StartCoroutine(WaitWhileTimeStopped());

        float angleStep = 360f / bulletCount;
        float angle = 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float rad = Mathf.Deg2Rad * angle;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            var bullet = Instantiate(bulletPrefab, bulletCenter, Quaternion.identity);
            TeddyBarrage tb = bullet.GetComponent<TeddyBarrage>();
            if (tb != null)
            {
                tb.SetDirection(dir);
                tb.speed = bulletSpeed;
            }
            angle += angleStep;
        }
        yield return null;
    }

    // ─────────────────────────────────────
    // 시간 정지 중 대기
    // ─────────────────────────────────────
    private IEnumerator WaitWhileTimeStopped()
    {
        while (TimeStopController.Instance != null && TimeStopController.Instance.IsTimeStopped)
        {
            yield return null;
        }
    }

    // ─────────────────────────────────────
    // HP / 사망
    // ─────────────────────────────────────
    public override void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        if (bossHealthUI != null)
            bossHealthUI.UpdateHP(currentHealth);

        //Debug.Log($"[Teddy] took damage {damage}, HP={currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log("[Teddy] Boss defeated!");
        MySceneManager.Instance.LoadNextScene();
        // 추가 종료 로직
    }
}
