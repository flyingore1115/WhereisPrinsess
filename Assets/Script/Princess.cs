using UnityEngine;
using System.Collections;
using MyGame;  // MyGame 네임스페이스에 정의된 데이터 클래스(TimePointData 등) 사용

public class Princess : MonoBehaviour, ITimeAffectable
{
    public float moveSpeed = 3f;              // 공주 이동 속도
    public Transform groundCheck;             // 발 아래 확인 위치
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f); // 박스 크기
    public LayerMask groundLayer;             // 땅 레이어
    public float fallThreshold = 0.2f;        // 구멍에서 떨어지기 전 허용 거리

    public Vector3 defaultStartPosition { get; private set; } //초기위치저장

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded = false;
    private bool isGameOver = false;
    private bool isTimeStopped = false;       // 시간 정지 상태

    private bool isShieldActive = false;
    // extraLives는 static 변수로 관리하여 씬 재시작 후에도 유지
    public static int persistentExtraLives = 0;
    public int extraLives = 0;                // 여벌 목숨

    // 무적 상태
    private bool isInvincible = false;
    public float invincibilityDuration = 2f;  // 무적 타임 지속 시간

    [HideInInspector]
    public bool isControlled = false;         // 공주 조종 여부

    // 흑백 효과 관련
    private Color originalColor;
    public Material grayscaleMaterial;
    private Material originalMaterial;

    void Awake()
    {
        Debug.Log($"[Princess] Awake() => transform.position = {transform.position}");
        defaultStartPosition = transform.position; //현재 위치를 기본 시작 위치로
    }

    void Start()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        originalColor = spriteRenderer.color;

        // 기존 persistentExtraLives 값 적용
        extraLives = persistentExtraLives;

        

        Debug.Log($"[Princess] Start() initial transform.position={transform.position}");
    }

    void FixedUpdate()
    {
        if (isTimeStopped) return;
        isGrounded = CheckGrounded();

        // 조종 모드일 경우, 외부(PrincessControlHandler)에서 이동 처리하므로 기본 이동하지 않음.
        if (isControlled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 기본적으로 공주는 우측으로 이동 (땅에 닿아 있을 때)
        if (!isGameOver && isGrounded)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
        }
        else if (!isGrounded && !isGameOver)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 보호막 활성 상태면 데미지 무시
        if (isShieldActive)
        {
            Debug.Log("Shield absorbed damage! Princess is safe.");
            return;
        }

        // 시간 정지 중이면 충돌 무시
        TimeStopController tsc = FindFirstObjectByType<TimeStopController>();
        if (tsc != null && tsc.IsTimeStopped)
        {
            return;
        }

        // "Enemy" 태그와 충돌 시 처리
        if (other.CompareTag("Enemy"))
        {
            ExplosiveEnemy explosive = other.GetComponent<ExplosiveEnemy>();
            if (explosive != null && !explosive.IsActivated)
            {
                Debug.Log("적 개체 활동 안해서 무시");
                return;
            }

            // 이미 무적 상태면 데미지 무시
            if (isInvincible)
            {
                Debug.Log("무적상태에서 맞음!");
                return;
            }


            // 플레이어 살아있든 말든, 공주 맞으면 GameOver
            Debug.Log("일반 게임오버");
            GameOver();

        }
    }

    public void GameOver()
    {
        GameOverManager gameOverManager = FindFirstObjectByType<GameOverManager>();
        if (gameOverManager != null)
        {
            gameOverManager.TriggerGameOver();
        }
        else
        {
            Debug.LogError("GameOverManager를 찾을 수 없습니다!");
        }
    }


    private IEnumerator CoRewindThenCheckpoint()
    {
        Time.timeScale = 1f;

        // RewindManager가 존재하는지 확인
        if (RewindManager.Instance == null)
        {
            Debug.LogError("[Princess] RewindManager.Instance is NULL! 되감기를 실행할 수 없습니다.");
            yield break;
        }

        // RewindManager 내부 객체도 확인
        if (RewindManager.Instance.player == null || RewindManager.Instance.princess == null)
        {
            Debug.LogError("[Princess] RewindManager 내부의 player 또는 princess가 NULL 상태입니다.");
            yield break;
        }

        // 1) 되감기 실행
        RewindManager.Instance.StartRewind();

        // 2) 되감기가 끝날 때까지 대기
        while (RewindManager.Instance.IsRewinding)
        {
            yield return null;
        }

        // 3) 체크포인트를 통해 적/위치 재설정
        TimePointManager.Instance.RewindToCheckpoint();
    }


    private IEnumerator DelayedRewind()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        rb.linearVelocity = Vector2.zero;
        Time.timeScale = 1f;
        TimePointManager.Instance.RewindToCheckpoint();
    }

    private void TriggerGameOverSequence()
    {
        isGameOver = true;
        if (animator != null)
        {
            animator.SetTrigger("isDie");
        }
        Debug.Log("Game Over");
        GameOverManager gameOverManager = FindFirstObjectByType<GameOverManager>();
        if (gameOverManager != null)
        {
            //StartCoroutine(gameOverManager.TriggerGameOverAfterAnimation(animator, this));
        }
    }

    private bool CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(groundCheck.position, groundCheckSize, 0f, Vector2.down, fallThreshold, groundLayer);
        if (hit.collider != null)
        {
            Debug.DrawRay(groundCheck.position, Vector2.down * fallThreshold, Color.green);
            return true;
        }
        Debug.DrawRay(groundCheck.position, Vector2.down * fallThreshold, Color.red);
        return false;
    }

    public void ResetToDefaultPosition()
    {
        transform.position = defaultStartPosition;
    }

    // 보호막 활성화: 일정 시간 동안 보호막 효과 적용 (색상 변경)
    public void EnableShield(float duration, bool maxLevel)
    {
        Debug.Log($"[Princess] EnableShield called. isShieldActive={isShieldActive}, maxLevel={maxLevel}");
        if (isShieldActive) return;

        isShieldActive = true;
        spriteRenderer.color = Color.cyan;

        // 여벌 목숨 추가는 스킬 업그레이드 시 처리하므로 여기서는 추가하지 않음.
        StartCoroutine(DisableShieldAfterTime(duration));
    }

    private IEnumerator DisableShieldAfterTime(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        isShieldActive = false;
        spriteRenderer.color = originalColor;
        Debug.Log("[Princess] Shield disabled.");
    }

    public void TakeDamage(int damage)
    {
        if (isShieldActive)
        {
            Debug.Log("Shield absorbed damage!");
            return;
        }
        GameOver();
    }

    // 무적 타임 코루틴: 일정 시간 동안 무적 상태로 전환
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        Debug.Log("Princess is now invincible.");
        yield return new WaitForSecondsRealtime(invincibilityDuration);
        isInvincible = false;
        Debug.Log("Princess invincibility ended.");
    }
    public void StopTime()
    {
        if (this == null || spriteRenderer == null) return;
        isTimeStopped = true;
        PostProcessingManager.Instance.SetDefaultEffects();
        if (animator != null)
        {
            animator.speed = 0;
        }
        // ★ Rigidbody 물리 중단
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

        }
    }

    public void ResumeTime()
    {
        if (this == null || spriteRenderer == null) return;
        isTimeStopped = false;
        if (animator != null)
        {
            animator.speed = 1;
        }
        PostProcessingManager.Instance.SetDefaultEffects();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    // 추가: 여벌 목숨을 외부에서 바로 추가할 수 있는 메서드
    public void AddExtraLife()
    {
        extraLives++;
        persistentExtraLives = extraLives; // static 업데이트
        Debug.Log("MAX Level Shield: Extra life granted!");
    }
}
