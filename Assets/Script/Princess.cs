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
    private int extraLives = 0; // 여벌 목숨
    // shieldActive 변수는 사용하지 않음

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
    }

    void FixedUpdate()
    {
        if (isTimeStopped) return;
        isGrounded = CheckGrounded();

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

        if (other.CompareTag("Enemy"))
        {
            if (extraLives > 0)
            {
                extraLives--; // 여벌 목숨 사용
                Debug.Log("Princess survived using extra life!");
                return;
            }
            Debug.Log("Princess was defeated!");
            GameOver();
        }
    }

    public void GameOver()
    {
        // 죽는 순간 게임 흐름이 계속되지 않도록 즉시 정지
        Time.timeScale = 0f;

        // 되감기 포인트가 있는 경우 되감기 실행, 없으면 기존 게임오버
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
        // “죽는 순간” 즉시 게임 정지했으므로, 시간이 흐르지 않음 => WaitForSecondsRealtime로 대기
        yield return new WaitForSecondsRealtime(0.2f); // 약간의 딜레이 후

        // 되감기 시작 전, 혹시 움직이는 것을 막기 위해 공주 / 플레이어 속도를 0으로
        rb.velocity = Vector2.zero;

        // “되감기” 실제 실행
        Time.timeScale = 1f; // 되감기 코루틴은 real-time으로 동작(= unscaled?),
                            // 여기서는 Time.timeScale=1로 잠깐 돌린 후
        TimePointManager.Instance.RewindToCheckpoint();
    }

    // 체크포인트가 없을 경우 기존 게임오버 로직 실행
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

    // 보호막 활성화: 일정 시간 동안 보호막 효과 적용 (색상 변경 + 여벌 목숨 부여 가능)
    public void EnableShield(float duration, bool maxLevel)
    {
        if (isShieldActive) return;
        isShieldActive = true;
        spriteRenderer.color = Color.cyan;

        if (maxLevel)
        {
            extraLives++;
            Debug.Log("MAX Level Shield: Extra life granted!");
        }
        StartCoroutine(DisableShieldAfterTime(duration));
    }

    private IEnumerator DisableShieldAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        isShieldActive = false;
        spriteRenderer.color = originalColor;
    }

    // 데미지 처리 함수: 보호막 활성화 중이면 데미지 무시
    public void TakeDamage(int damage)
    {
        // shieldActive는 사용하지 않고 isShieldActive로 판단
        if (isShieldActive)
        {
            Debug.Log("Shield absorbed damage!");
            return;
        }
        GameOver();
    }

    // 시간 정지 관련
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
}
