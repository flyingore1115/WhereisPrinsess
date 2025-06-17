using UnityEngine;
using System.Collections;

/// <summary>
/// ▣ “테디” 보스 AI  
/// 1사이클: [폭발 경고]→[폭탄 낙하]→[탄막] → 순간이동 + 유도탄  
/// ────────────────────────────────────────────────
/// 개선사항
/// • 일반공격 3회 맞으면 즉시 텔레포트 + 랜덤 공격  
/// • Phase 2 진입 직후 텔레포트 + 특수 탄막  
/// • 텔레포트 중/후 잠시 무적  
/// </summary>
public class Teddy : BaseEnemy
{
    /*──────────────────────────────────────────────
     * ▣ 인스펙터 노출 변수 (기존 + 생략 없이)
     *─────────────────────────────────────────────*/

    [Header("Boss Stats")]
    public int bossMaxHP = 20;                // 보스 최대 체력
    public float moveSpeed = 2f;              // 좌우 기본 이동 속도

    [Header("Cycle Settings")]
    public float cycleIdleTime = 2f;          // 사이클 간 휴식
    public float alertAnimTime = 1f;          // ‘Warning’ 애니메이션 길이

    [Header("Bomb Pattern Settings")]
    public float bombSpawnY = 8f;             // 폭탄 낙하 시작 Y
    public Vector2 bombXRange = new Vector2(-6f, 6f);
    public int bombCountNormal = 3;
    public int bombCountSpecial = 5;

    [Header("Barrage Pattern Settings")]
    public int   bulletCountNormal = 8;
    public int   bulletCountSpecial = 12;
    public float bulletSpeed = 3f;
    public Vector2 bulletCenter;              // 탄막 발사 중심

    [Header("Explosion Pattern Settings")]
    public GameObject explosionWarningPrefab;
    public float      explosionDelay = 1.5f;
    public Vector2[]  explosionPositions;

    [Header("References")]
    public BossHealthUI bossHealthUI;
    public GameObject   bombPrefab;
    public GameObject   bulletPrefab;

    [Header("Teleport Settings")]
    public Vector2[] movePoints;              // 순간이동 후보 좌표
    public float teleportDelay = 0.5f;        // 텔레포트 전 대기
    public float vanishTime   = 0.1f;         // 사라져 있는 시간
    public GameObject teleportEffectPrefab;

    [Header("Homing Missile Settings")]
    public GameObject homingMissilePrefab;
    public Vector2[]  missileSpawnPoints;
    public int        homingInterval = 1;     // 1프레임 간격

    /*──────────────────────────────────────────────
     * ▣ 새로 추가된 내부 변수
     *─────────────────────────────────────────────*/

    private int  normalHitCount      = 0;   // 일반공격 누적 횟수
    private bool isInvincibleForHits = false; // 텔레포트 중 무적

    /* 기존 내부 상태 변수 */
    private bool     isPhase2  = false;     // 30% 이하 진입 플래그
    private float    moveDir   = 1f;        // 좌/우 이동 방향
    private Transform playerT;              // 플레이어 Transform 캐시

    /*──────────────────────────────────────────────
     * ▣ Unity 생명주기
     *─────────────────────────────────────────────*/

    /// <summary>Awake: BaseEnemy 초기화 + HP 세팅</summary>
    protected override void Awake()
    {
        base.Awake();
        maxHealth      = bossMaxHP;
        currentHealth  = bossMaxHP;
        UpdateHealthDisplay();

        if (Player.Instance != null)
            playerT = Player.Instance.transform;
    }

    /// <summary>Start: UI 초기화 후 메인 패턴 루프 실행</summary>
    void Start()
    {
        Princess.Instance?.PlayScaredIdle();
        bossHealthUI?.InitBossUI("테디", bossMaxHP);

        StartCoroutine(MainPatternLoop());
    }

    /// <summary>Update: 이동·페이즈 체크</summary>
    void Update()
    {
        if (isTimeStopped) return;

        FacePlayer();
        BasicMovement();

        // Phase 2 진입(HP ≤ 30 %) 
        if (!isPhase2 && currentHealth <= bossMaxHP * 0.3f)
        {
            isPhase2 = true;
            Debug.Log("[Teddy] Phase2 activated!");
            StartCoroutine(Phase2TeleportAttack());
        }
    }

    /*──────────────────────────────────────────────
     * ▣ 기본 이동·보조 로직
     *─────────────────────────────────────────────*/

    /// <summary>플레이어 위치 기준 좌우 스프라이트 반전</summary>
    private void FacePlayer()
    {
        if (playerT == null || spriteRenderer == null) return;
        spriteRenderer.flipX = (playerT.position.x < transform.position.x);
    }

    /// <summary>스테이지 경계 내 단순 왕복 이동</summary>
    private void BasicMovement()
    {
        if (transform.position.x > 7f)  moveDir = -1f;
        if (transform.position.x < -7f) moveDir = 1f;

        transform.Translate(Vector2.right * moveDir * moveSpeed * Time.deltaTime);
    }

    /// <summary>시간정지 해제될 때까지 대기</summary>
    private IEnumerator WaitWhileTimeStopped()
    {
        while (TimeStopController.Instance != null &&
               TimeStopController.Instance.IsTimeStopped)
            yield return null;
    }

    /*──────────────────────────────────────────────
     * ▣ 메인 패턴 루프
     *─────────────────────────────────────────────*/

    /// <summary>
    /// 보스가 살아있는 동안 반복 실행되는 메인 루프  
    /// 사이클 → 순간이동 패턴 → 휴식
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
/// 사이클이 끝난 뒤 실행되는 순간이동 패턴.
/// ① teleportDelay 대기 → ② 사라짐 이펙트 + 비활성화  
/// ③ vanishTime 대기 → ④ 새로운 위치로 이동  
/// ⑤ 등장 이펙트 + 재활성화  
/// ⑥ 유도탄 발사  
/// </summary>
private IEnumerator TeleportAfterPattern()
{
    // 1) 순간이동 전 대기
    yield return new WaitForSeconds(teleportDelay);

    // 2) 사라짐 이펙트
    if (teleportEffectPrefab != null)
        Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
    spriteRenderer.enabled = false;

    // 3) vanishTime 대기
    yield return new WaitForSeconds(vanishTime);

    // 4) 이동 지점 랜덤 선택
    Vector2 target = transform.position;
    if (movePoints.Length > 0)
    {
        do
            target = movePoints[Random.Range(0, movePoints.Length)];
        while ((Vector2)transform.position == target);
    }
    transform.position = target;

    // 5) 등장 이펙트 + 재활성화
    if (teleportEffectPrefab != null)
        Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
    spriteRenderer.enabled = true;

    // 6) 이동 후 유도탄 발사
    yield return StartCoroutine(FireHomingMissilesFromPoints());
}


    /// <summary>1사이클(폭발→폭탄→탄막) 수행</summary>
    private IEnumerator DoOneCycle(bool special)
    {
        // (1) 폭발 경고 패턴
        yield return StartCoroutine(PlayAlertAnimation());
        yield return StartCoroutine(SpawnExplosionWarning());

        // (2) 폭탄 낙하
        yield return StartCoroutine(PlayAlertAnimation());
        int bombs = special ? bombCountSpecial : bombCountNormal;
        yield return StartCoroutine(DropMultipleBombs(bombs));

        // (3) 탄막
        yield return StartCoroutine(PlayAlertAnimation());
        int bullets = special ? bulletCountSpecial : bulletCountNormal;
        yield return StartCoroutine(FireRadialBullets(bullets));
    }

    /// <summary>Warning 애니메이션 트리거 + 대기</summary>
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
     * ▣ 개별 패턴(폭발, 폭탄, 탄막, 유도탄)
     *─────────────────────────────────────────────*/

    /// <summary>폭발 경고 프리팹 생성</summary>
    private IEnumerator SpawnExplosionWarning()
    {
        if (explosionWarningPrefab == null || explosionPositions.Length == 0)
            yield break;

        yield return StartCoroutine(WaitWhileTimeStopped());

        Vector2 pos = explosionPositions[Random.Range(0, explosionPositions.Length)];
        var warning = Instantiate(explosionWarningPrefab, pos, Quaternion.identity);

        var ew = warning.GetComponent<ExplosionWarning>();
        if (ew != null) ew.warningDuration = explosionDelay;
    }

    /// <summary>폭탄 여러 개 순차 낙하</summary>
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

    /// <summary>원형으로 균등 발사하는 탄막</summary>
    private IEnumerator FireRadialBullets(int count)
    {
        if (bulletPrefab == null) yield break;
        yield return StartCoroutine(WaitWhileTimeStopped());

        SoundManager.Instance?.PlaySFX("TeddyBarrageBlast");

        float step = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = step * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var b  = Instantiate(bulletPrefab, bulletCenter, Quaternion.identity);
            var tb = b.GetComponent<TeddyBarrage>();
            if (tb != null)
            {
                tb.SetDirection(dir);
                tb.speed = bulletSpeed;
            }
        }
    }

    /// <summary>미사일 연속 발사 (Phase별 좌표 다름)</summary>
    private IEnumerator FireHomingMissilesFromPoints()
    {
        if (homingMissilePrefab == null || missileSpawnPoints.Length < 3 ||
            Princess.Instance == null)
            yield break;

        // Phase1: 0,2 / Phase2: 0,1,2
        int[] idx = isPhase2 ? new int[] { 0, 1, 2 } : new int[] { 0, 2 };
        foreach (int i in idx)
        {
            var m = Instantiate(homingMissilePrefab,
                                (Vector3)missileSpawnPoints[i],
                                Quaternion.identity);
            m.GetComponent<HomingMissile>()?.Init(Princess.Instance.transform);
            yield return null; // 1프레임 간격
        }
    }

    /*──────────────────────────────────────────────
     * ▣ 추가된 특수 패턴
     *─────────────────────────────────────────────*/

    /// <summary>
    /// ▣ 일반공격을 3회 맞으면 발동  
    /// 1) 텔레포트 2) 탄막 또는 미사일 랜덤 선택  
    /// </summary>
    private IEnumerator TriggerTeleportAttack()
    {
        isInvincibleForHits = true; // 잠시 무적

        /* ① 사라짐 */
        yield return new WaitForSeconds(teleportDelay);
        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(vanishTime);

        /* ② 위치 이동 */
        Vector2 target = transform.position;
        if (movePoints.Length > 0)
        {
            do
                target = movePoints[Random.Range(0, movePoints.Length)];
            while ((Vector2)transform.position == target);
        }
        transform.position = target;

        /* ③ 등장 이펙트 */
        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        spriteRenderer.enabled = true;

        /* ④ 랜덤 공격 */
        if (Random.value < 0.5f)
            yield return StartCoroutine(FireRadialBullets(isPhase2 ? bulletCountSpecial
                                                                   : bulletCountNormal));
        else
            yield return StartCoroutine(FireHomingMissilesFromPoints());

        /* ⑤ 무적 해제 */
        yield return new WaitForSeconds(0.5f);
        isInvincibleForHits = false;
    }

    /// <summary>
    /// ▣ Phase 2 진입 시 즉시 발동  
    /// 텔레포트 + 특수 탄막
    /// </summary>
    private IEnumerator Phase2TeleportAttack()
    {
        /* 텔레포트 (동일 로직) */
        yield return new WaitForSeconds(teleportDelay);
        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(vanishTime);

        Vector2 target = transform.position;
        if (movePoints.Length > 0)
        {
            do
                target = movePoints[Random.Range(0, movePoints.Length)];
            while ((Vector2)transform.position == target);
        }
        transform.position = target;

        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        spriteRenderer.enabled = true;

        /* 특수 탄막 */
        yield return StartCoroutine(FireRadialBullets(bulletCountSpecial));
    }

    /*──────────────────────────────────────────────
     * ▣ 데미지·사망 처리 오버라이드
     *─────────────────────────────────────────────*/

    /// <summary>
    /// 데미지 처리 + 일반공격 카운트  
    /// ※ P_Attack이 주는 데미지를 ‘일반공격’으로 간주
    /// </summary>
    public override void TakeDamage(int damage)
    {
        if (isDead || isInvincibleForHits) return;

        base.TakeDamage(damage);
        bossHealthUI?.UpdateHP(currentHealth);

        /* 일반공격 3회 체크 */
        normalHitCount++;
        if (normalHitCount >= 3)
        {
            normalHitCount = 0;
            StartCoroutine(TriggerTeleportAttack());
        }
    }

    /// <summary>사망 시 호출: 다음 씬 로드</summary>
    protected override void Die()
    {
        base.Die();
        Debug.Log("[Teddy] Boss defeated!");
        MySceneManager.Instance.LoadNextScene();
    }

    /*──────────────────────────────────────────────
     * ▣ Rewind 대응 (기존)
     *─────────────────────────────────────────────*/

    public void ResetPatternOnRewind()
    {
        StopAllCoroutines();
        if (!isDead) StartCoroutine(MainPatternLoop());

        isPhase2 = (currentHealth <= bossMaxHP * 0.3f);
        normalHitCount = 0;
    }
}
