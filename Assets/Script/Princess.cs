using UnityEngine;
using System.Collections;

public class Princess : MonoBehaviour, ITimeAffectable
{
    public float moveSpeed = 3f; // 공주 이동 속도
    public Transform groundCheck; // 발 아래 확인 위치
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f); // 박스 크기
    public LayerMask groundLayer; // 땅 레이어
    public float fallThreshold = 0.2f; // 구멍에서 떨어지기 전 허용 거리

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded = false;
    private bool isGameOver = false;
    private bool isTimeStopped = false; // 시간 정지 상태

    private bool isShieldActive = false;
    // extraLives를 static 변수로 관리하여 씬 재시작 후에도 유지
    public static int persistentExtraLives = 0;
    public int extraLives = 0; // 여벌 목숨

    // 무적 상태
    private bool isInvincible = false;
    public float invincibilityDuration = 2f; // 무적 타임 지속 시간

    [HideInInspector]
    public bool isControlled = false; //공주 조종당하는지

    // 흑백 효과 관련
    private Color originalColor;
    public Material grayscaleMaterial;
    private Material originalMaterial;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        originalColor = spriteRenderer.color;

        // 기존 persistentExtraLives 값 적용
        extraLives = persistentExtraLives;
    }

    void FixedUpdate()
    {
        if (isTimeStopped) return;
        isGrounded = CheckGrounded();

        // 공주 조종 모드일 때는, 외부(PrincessControlHandler)에서 이동 처리하므로 기본 이동 실행하지 않음.
        if (isControlled) return;

        if (!isGameOver && isGrounded)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        }
        else if (!isGrounded && !isGameOver)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isShieldActive)
        {
            Debug.Log("Shield absorbed damage! Princess is safe.");
            return;
        }

        TimeStopController tsc = FindObjectOfType<TimeStopController>();
        if (tsc != null && tsc.IsTimeStopped)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            ExplosiveEnemy explosive = other.GetComponent<ExplosiveEnemy>();
            if (explosive != null && !explosive.IsActivated)
            {
                Debug.Log("Explosive enemy not activated. Collision with Princess ignored.");
                return;
            }

            // 만약 이미 무적 상태라면 데미지 무시
            if (isInvincible)
            {
                Debug.Log("Princess is invincible, damage ignored.");
                return;
            }

            // 여벌 목숨이 있으면 소비하고 무적 타임 부여
            if (extraLives > 0)
            {
                extraLives--;
                persistentExtraLives = extraLives; // static 변수 갱신
                Debug.Log("Princess survived using extra life!");
                StartCoroutine(InvincibilityCoroutine());
                return;
            }
            Debug.Log("Princess was defeated!");
            GameOver();
        }
    }

    public void GameOver()
    {
        Time.timeScale = 0f;

        if (TimePointManager.Instance != null && TimePointManager.Instance.HasCheckpoint())
        {
            StartCoroutine(DelayedRewind());
        }
        else
        {
            TriggerGameOverSequence();
        }
    }

    private IEnumerator DelayedRewind()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        rb.velocity = Vector2.zero;
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
        GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
        if (gameOverManager != null)
        {
            StartCoroutine(gameOverManager.TriggerGameOverAfterAnimation(animator, this));
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

    // 보호막 활성화: 일정 시간 동안 보호막 효과 적용 (색상 변경)
    public void EnableShield(float duration, bool maxLevel)
    {
        Debug.Log($"[Princess] EnableShield called. isShieldActive={isShieldActive}, maxLevel={maxLevel}");
        // 여기서는 shield 사용과 여벌 목숨 추가를 분리합니다.
        if (isShieldActive)
        {
            return;
        }

        isShieldActive = true;
        spriteRenderer.color = Color.cyan;

        // 여벌 목숨 추가는 스킬 업그레이드 시 GameManager나 SkillManager에서 처리하므로 여기서는 추가하지 않음.

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
        if (grayscaleMaterial != null)
        {
            spriteRenderer.material = grayscaleMaterial;
        }
        if (animator != null)
        {
            animator.speed = 0;
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
        RestoreColor();
    }

    public void RestoreColor()
    {
        if (this == null || spriteRenderer == null) return;
        spriteRenderer.material = originalMaterial;
    }

    // 추가: 여벌 목숨을 외부에서 바로 추가할 수 있는 메서드
    public void AddExtraLife()
    {
        extraLives++;
        persistentExtraLives = extraLives; // static 업데이트
        Debug.Log("MAX Level Shield: Extra life granted!");
    }
}
