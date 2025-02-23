using UnityEngine;

public class Princess : MonoBehaviour, ITimeAffectable
{
    public float moveSpeed = 3f; // 공주 이동 속도
    public Transform groundCheck; // 발 아래 확인 위치
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f); // 박스 크기
    public LayerMask groundLayer; // 땅 레이어
    public float fallThreshold = 0.2f; // 구멍에서 떨어지기 전의 허용 거리

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGameOver = false;
    private bool isFalling = false; // 낙하 여부 확인

    // 시간 정지 상태 변수
    private bool isTimeStopped = false;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        originalColor = spriteRenderer.color;
    }

    void FixedUpdate()
    {
        // 시간 정지 상태면 이동 업데이트를 건너뛰고 속도를 0으로 설정
        if (isTimeStopped)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        bool isGrounded = CheckGrounded();

        // 땅 위에 있고 게임 오버 상태가 아니면 오른쪽으로 이동
        if (!isGameOver && isGrounded)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        }
        // 공중에 있을 때는 수평 이동 없이 자연 낙하
        else if (!isGrounded && !isGameOver)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isGameOver && other.CompareTag("fall")) // 구멍과 충돌
        {
            Debug.Log("Princess fell into a hole!");
            isFalling = true; // 낙하 상태로 설정
            GameOver();
        }
        else if (!isGameOver && other.CompareTag("Enemy")) // 적과 충돌
        {
            Debug.Log("Princess was defeated by an enemy!");
            isFalling = false; // 일반 게임오버
            GameOver();
        }
    }

    public void GameOver()
    {
        isGameOver = true;

        if (animator != null)
        {
            if (isFalling)
            {
                animator.SetTrigger("isFall"); // 낙하 애니메이션 트리거
            }
            else
            {
                animator.SetTrigger("isDie"); // 쓰러짐 애니메이션 트리거
            }
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
        RaycastHit2D hit = Physics2D.BoxCast(
            groundCheck.position,
            groundCheckSize,
            0f,
            Vector2.down,
            fallThreshold,
            groundLayer
        );

        return hit.collider != null;
    }

    // ITimeAffectable 구현: 시간 정지 시 이동 및 애니메이션을 멈춤
    public void StopTime()
    {
        isTimeStopped = true;
        rb.velocity = Vector2.zero;
        if (animator != null)
        {
            animator.speed = 0;
        }
    }

    public void ResumeTime()
    {
        isTimeStopped = false;
        if (animator != null)
        {
            animator.speed = 1;
        }
    }

    public void RestoreColor()
    {
        spriteRenderer.color = originalColor;
    }
}
