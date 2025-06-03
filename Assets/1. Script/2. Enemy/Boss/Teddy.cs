using UnityEngine;
using System.Collections;

/// <summary>
/// ────────────────────────────────────────────────────────────────────────────────
/// “테디” 보스 AI
///
///  ● 1 사이클 = [경고+폭발] → [폭탄낙하] → [탄막]
///  ● 사이클이 끝나면 0.5초 대기 후 순간이동 + 공주 추적 유도탄 발사
///  ● HP 30 % 이하부터 Phase 2 (폭탄/탄막 개수 증가)
///
///  ⚠️ 2025-06-03: 폭발 경고·카운트다운·실제 폭발 로직을
///                 별도 프리팹(ExplosionWarning)으로 완전히 분리했다.
///                 Teddy.cs 안에서는 ‘경고 프리팹 인스턴스화’만 담당한다.
/// ────────────────────────────────────────────────────────────────────────────────
/// </summary>
public class Teddy : BaseEnemy
{
    /*──────────────────────────────────────────────
     * 1. 인스펙터 노출 변수
     *────────────────────────────────────────────*/
    [Header("Boss Stats")]
    public int bossMaxHP = 20;
    public float moveSpeed = 2f;

    [Header("Cycle Settings")]
    public float cycleIdleTime = 2f;     // 사이클 간 휴식
    public float alertAnimTime = 1f;     // ‘Warning’ 애니메이션 길이

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
    public GameObject explosionWarningPrefab;   // ExplosionWarning 프리팹
    public float explosionRadius = 2f;          // 데미지 반경
    public float explosionDelay = 1.5f;         // 경고 지속·폭발까지 남은 시간
    public Vector2[] explosionPositions;        // 폭발 위치 후보

    [Header("References")]
    public BossHealthUI bossHealthUI;
    public GameObject bombPrefab;
    public GameObject bulletPrefab;

    [Header("Teleport Settings")]
    public Vector2[] movePoints;
    public float teleportDelay = 0.5f;
    public float vanishTime = 0.1f;
    public GameObject teleportEffectPrefab;

    [Header("Homing Missile Settings")]
    public GameObject homingMissilePrefab;
    public int homingMissileCount = 3;
    public float homingInterval = 0.2f;

    /*──────────────────────────────────────────────
     * 2. 내부 상태 변수
     *────────────────────────────────────────────*/
    private bool isPhase2 = false;
    private float moveDir = 1f;
    private Transform playerT;

    /*──────────────────────────────────────────────
     * 3. Unity 생명주기 함수
     *────────────────────────────────────────────*/

    /// <summary>
    /// Awake: BaseEnemy 초기화 + HP 세팅 + 플레이어 트랜스폼 캐싱
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        maxHealth = bossMaxHP;
        currentHealth = bossMaxHP;
        UpdateHealthDisplay();

        if (Player.Instance != null)
            playerT = Player.Instance.transform;
    }

    /// <summary>
    /// Start: UI 초기화 후 보스 인트로 종료 시 메인 루프 시작
    /// </summary>
    void Start()
    {
        Princess.Instance?.PlayScaredIdle();
        bossHealthUI?.InitBossUI("테디", bossMaxHP);

        StartCoroutine(MainPatternLoop());
    }

    /// <summary>
    /// Update: 시간정지가 아닐 때 이동·페이싱·스프라이트 방향 처리
    /// </summary>
    void Update()
    {
        if (isTimeStopped) return;

        FacePlayer();
        BasicMovement();

        // Phase 2 전환
        if (!isPhase2 && currentHealth <= bossMaxHP * 0.3f)
        {
            isPhase2 = true;
            Debug.Log("[Teddy] Phase2 activated!");
        }
    }

    /*──────────────────────────────────────────────
     * 4. 이동·보조 메서드
     *────────────────────────────────────────────*/

    /// <summary>플레이어 x 위치에 따라 스프라이트를 좌우 뒤집음</summary>
    private void FacePlayer()
    {
        if (playerT == null || spriteRenderer == null) return;
        spriteRenderer.flipX = (playerT.position.x < transform.position.x);
    }

    /// <summary>스테이지 경계 내에서 단순 좌우 왕복 이동</summary>
    private void BasicMovement()
    {
        if (transform.position.x > 7f) moveDir = -1f;
        else if (transform.position.x < -7f) moveDir = 1f;
        transform.Translate(Vector2.right * moveDir * moveSpeed * Time.deltaTime);
    }

    /// <summary>Coroutine: 시간정지 해제될 때까지 대기</summary>
    private IEnumerator WaitWhileTimeStopped()
    {
        while (TimeStopController.Instance != null && TimeStopController.Instance.IsTimeStopped)
            yield return null;
    }

    /*──────────────────────────────────────────────
     * 5. 보스 패턴 메인 루프
     *────────────────────────────────────────────*/

    /// <summary>
    /// MainPatternLoop: 보스 생존 중 반복 실행.
    /// 한 사이클 수행 → 순간이동 → 휴식
    /// </summary>
    private IEnumerator MainPatternLoop()
    {
        while (!isDead)
        {
            yield return StartCoroutine(DoOneCycle(isPhase2));
            yield return StartCoroutine(WaitWhileTimeStopped());
            yield return StartCoroutine(TeleportAfterPattern());
            yield return new WaitForSeconds(cycleIdleTime);
        }
    }

    /// <summary>
    /// DoOneCycle: 경고+폭발 → 폭탄 → 탄막 1세트 수행
    /// </summary>
    private IEnumerator DoOneCycle(bool special)
    {
        yield return StartCoroutine(PlayAlertAnimation());
        yield return StartCoroutine(SpawnExplosionWarning());

        yield return StartCoroutine(PlayAlertAnimation());
        int bombs = special ? bombCountSpecial : bombCountNormal;
        yield return StartCoroutine(DropMultipleBombs(bombs));

        yield return StartCoroutine(PlayAlertAnimation());
        int bullets = special ? bulletCountSpecial : bulletCountNormal;
        yield return StartCoroutine(FireRadialBullets(bullets));
    }

    /// <summary>
    /// PlayAlertAnimation: 애니메이션 “Warning” 재생 + alertAnimTime 만큼 대기
    /// </summary>
    private IEnumerator PlayAlertAnimation()
    {
        yield return StartCoroutine(WaitWhileTimeStopped());
        animator?.SetTrigger("Warning");

        float t = 0f;
        while (t < alertAnimTime)
        {
            if (isTimeStopped) yield return StartCoroutine(WaitWhileTimeStopped());
            t += Time.deltaTime;
            yield return null;
        }
    }

    /*──────────────────────────────────────────────
     * 6. 개별 패턴 구현
     *────────────────────────────────────────────*/

    /// <summary>
    /// SpawnExplosionWarning:
    /// ① 랜덤 위치 선정 → ② ExplosionWarning 프리팹 인스턴스화
    ///    프리팹 내부에서 ‘경고–카운트다운–폭발’ 까지 모두 처리함
    /// </summary>
    private IEnumerator SpawnExplosionWarning()
    {
        if (explosionWarningPrefab == null || explosionPositions.Length == 0)
        {
            yield break;
        }
            

        yield return StartCoroutine(WaitWhileTimeStopped());

        // (1) 폭발 위치 랜덤 선택
        Vector2 pos = explosionPositions[Random.Range(0, explosionPositions.Length)];

        // (2) 프리팹 인스턴스화
        var warning = Instantiate(explosionWarningPrefab, pos, Quaternion.identity);

        // (3) 프리팹 파라미터 덮어쓰기 (반경·지속시간 등을 필요 시 전달)
        var ew = warning.GetComponent<ExplosionWarning>();
        if (ew != null)
        {
            ew.warningDuration = explosionDelay;
            // BaseSprite의 bounds를 쓰지 않고 직접 반경을 지정하고 싶다면
            // ew.SetRadius(explosionRadius);
        }

        // 폭발 자체는 프리팹이 알아서 처리하므로 Teddy는 대기만
        // (지연이 필요 없다면 바로 return)
        yield return null;
    }

    /// <summary>
    /// DropMultipleBombs: 윗공간에서 ‘count’개 폭탄 순차 낙하시킴
    /// </summary>
    private IEnumerator DropMultipleBombs(int count)
    {
        if (bombPrefab == null) yield break;

        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(WaitWhileTimeStopped());
            float x = Random.Range(bombXRange.x, bombXRange.y);
            Instantiate(bombPrefab, new Vector2(x, bombSpawnY), Quaternion.identity);
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// FireRadialBullets: 중심(bulletCenter)에서 ‘count’ 방향으로 균등 발사
    /// </summary>
    private IEnumerator FireRadialBullets(int count)
    {
        if (bulletPrefab == null) yield break;
        yield return StartCoroutine(WaitWhileTimeStopped());

        float step = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = step * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var b = Instantiate(bulletPrefab, bulletCenter, Quaternion.identity);
            var tb = b.GetComponent<TeddyBarrage>();
            if (tb != null)
            {
                tb.SetDirection(dir);
                tb.speed = bulletSpeed;
            }
        }
    }

    /*──────────────────────────────────────────────
     * 7. 순간이동 + 유도탄
     *────────────────────────────────────────────*/

    /// <summary>
    /// TeleportAfterPattern:
    /// ① 0.5초 대기 → ② 사라짐 → ③ 신규 위치로 텔레포트
    /// ④ 등장 이펙트 → ⑤ 공주 추적 유도탄 발사
    /// </summary>
    private IEnumerator TeleportAfterPattern()
    {
        yield return new WaitForSeconds(teleportDelay);

        // 사라지는 이펙트
        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(vanishTime);

        // 위치 재배치
        Vector2 target = transform.position;
        if (movePoints.Length > 0)
        {
            do
                target = movePoints[Random.Range(0, movePoints.Length)];
            while ((Vector2)transform.position == target);
        }
        transform.position = target;

        // 등장 이펙트
        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        spriteRenderer.enabled = true;

        // 유도탄 연속 발사
        yield return StartCoroutine(FireHomingMissilesAtPrincess());
    }

    /// <summary>
    /// FireHomingMissilesAtPrincess: 유도탄을 ‘homingMissileCount’회 발사
    /// </summary>
    private IEnumerator FireHomingMissilesAtPrincess()
    {
        if (homingMissilePrefab == null || Princess.Instance == null) yield break;

        for (int i = 0; i < homingMissileCount; i++)
        {
            var missile = Instantiate(homingMissilePrefab, transform.position, Quaternion.identity);
            missile.GetComponent<HomingMissile>()?.Init(Princess.Instance.transform);
            yield return new WaitForSeconds(homingInterval);
        }
    }

    /*──────────────────────────────────────────────
     * 8. 데미지·사망 처리
     *────────────────────────────────────────────*/

    /// <summary>
    /// TakeDamage: 그로기 상태에서만 호출됨 (BaseEnemy 쪽 로직)
    /// </summary>
    public override void TakeDamage(int damage)
    {
        if (isDead) return;
        base.TakeDamage(damage);
        bossHealthUI?.UpdateHP(currentHealth);
    }

    /// <summary>
    /// Die: 사망 시 다음 씬 로드
    /// </summary>
    protected override void Die()
    {
        base.Die();
        Debug.Log("[Teddy] Boss defeated!");
        MySceneManager.Instance.LoadNextScene();
    }
}
