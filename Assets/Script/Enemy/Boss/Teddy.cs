using UnityEngine;
using System.Collections;

public class Teddy : BaseEnemy
{
    [Header("Boss Stats")]
    public int bossMaxHP = 20;
    public float moveSpeed = 2f;

    [Header("Cycle Settings")]
    public float cycleIdleTime = 2f;  // 패턴 사이 쿨타임

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
        if (bossHealthUI != null)
            bossHealthUI.InitBossUI("테디", bossMaxHP);

        StartCoroutine(MainPatternLoop());
    }

    void Update()
    {
        // ■ 시간 정지라면 이동 로직 패스
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
        if (transform.position.x > 7f) moveDir = -1f;
        else if (transform.position.x < -7f) moveDir = 1f;
        transform.Translate(Vector2.right * moveDir * moveSpeed * Time.deltaTime);
    }

    // ─────────────────────────────────────
    // 메인 패턴 루프: 죽기 전까지 무한 반복
    // ─────────────────────────────────────
    private IEnumerator MainPatternLoop()
    {
        while (!isDead)
        {
            // 1) 한 사이클 실행
            yield return StartCoroutine(DoOneCycle(isPhase2));

            // 2) 사이클 간 대기
            yield return StartCoroutine(WaitWhileTimeStopped()); // 시간 정지면 대기
            yield return new WaitForSeconds(cycleIdleTime);
        }
    }

    // ─────────────────────────────────────
    // 한 사이클 = 폭발 -> 폭탄 -> 탄막 -> (필요하면 또 폭발)
    // ─────────────────────────────────────
    private IEnumerator DoOneCycle(bool special)
    {
        // 1) 폭발
        yield return StartCoroutine(ExplodeOnce(special));

        // 2) 폭탄 N개
        int bombs = special ? bombCountSpecial : bombCountNormal;
        yield return StartCoroutine(DropMultipleBombs(bombs));

        // 3) 탄막
        int bulletCount = special ? bulletCountSpecial : bulletCountNormal;
        yield return StartCoroutine(FireRadialBullets(bulletCount));

        // (원하면 더 패턴 추가)
        yield return null;
    }

    // ─────────────────────────────────────
    // 폭탄 여러개 투하
    // ─────────────────────────────────────
    private IEnumerator DropMultipleBombs(int count)
    {
        if (bombPrefab == null) yield break;

        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(WaitWhileTimeStopped());

            float randomX = Random.Range(bombXRange.x, bombXRange.y);
            Vector2 spawnPos = new Vector2(randomX, bombSpawnY);
            Instantiate(bombPrefab, spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(0.3f);
        }
    }

    // ─────────────────────────────────────
    // 탄막 발사
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
    // 폭발 예시 (1회)
    // ─────────────────────────────────────
    private IEnumerator ExplodeOnce(bool special)
    {
        if (explosionPositions == null || explosionPositions.Length == 0) yield break;
        
        yield return StartCoroutine(WaitWhileTimeStopped());

        int index = Random.Range(0, explosionPositions.Length);
        Vector2 pos = explosionPositions[index];

        // 예고 프리팹
        if (explosionWarningPrefab != null)
        {
            var warning = Instantiate(explosionWarningPrefab, pos, Quaternion.identity);
            warning.transform.localScale = new Vector3(explosionRadius * 2, explosionRadius * 2, 1f);
            Destroy(warning, explosionDelay);
        }

        // 딜레이 후 실제 폭발
        yield return new WaitForSeconds(explosionDelay);

        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, pos, Quaternion.identity);

        // 피해 처리
        var hits = Physics2D.OverlapCircleAll(pos, explosionRadius);
        foreach (var c in hits)
        {
            if (c.CompareTag("Player"))
            {
                // c.GetComponent<PlayerOver>()?.TakeDamage(1);
            }
        }
        yield return null;
    }

    // ─────────────────────────────────────
    // 시간 정지 중에는 코루틴을 진행하지 않고 대기
    // ─────────────────────────────────────
    private IEnumerator WaitWhileTimeStopped()
    {
        // TimeStopController.Instance.IsTimeStopped 가 true인 동안 대기
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

        Debug.Log($"[Teddy] took damage {damage}, HP={currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log("[Teddy] Boss defeated!");
        // 추가 종료 로직
    }
}
