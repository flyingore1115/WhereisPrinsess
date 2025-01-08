using UnityEngine;

public class Princess : MonoBehaviour
{
    public float moveSpeed = 3f; // 공주 이동 속도
    public Transform groundCheck; // 발 아래 확인 위치
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f); // 박스 크기
    public LayerMask groundLayer; // 땅 레이어
    public float fallThreshold = 0.2f; // 구멍에서 떨어지기 전의 허용 거리

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded = false;
    private bool isGameOver = false;
    private bool isFalling = false; // 낙하 여부 확인

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // 공주 발 아래 땅 감지
        isGrounded = CheckGrounded();

        // 땅에 있을 때만 이동
        if (!isGameOver && isGrounded)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        }
        else if (!isGrounded && !isGameOver)
        {
            // 공중에서는 수평 속도를 멈추고 자연스럽게 떨어짐
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

        // GameOverManager 호출
        GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
        if (gameOverManager != null)
        {
            // TriggerGameOverAfterAnimation 호출로 변경
            StartCoroutine(gameOverManager.TriggerGameOverAfterAnimation(animator, this));
            Debug.Log("내가호출했엌ㅋㅋㅋㅋㅋㅋ");
        }
    }

    private bool CheckGrounded()
    {
        // 박스 캐스트를 사용해 공주 발 아래 땅 확인
        RaycastHit2D hit = Physics2D.BoxCast(
            groundCheck.position,
            groundCheckSize,
            0f,
            Vector2.down,
            fallThreshold,
            groundLayer
        );

        if (hit.collider != null)
        {
            Debug.DrawRay(groundCheck.position, Vector2.down * fallThreshold, Color.green); // 땅 감지 시각화
            return true;
        }

        Debug.DrawRay(groundCheck.position, Vector2.down * fallThreshold, Color.red); // 땅 미감지 시각화
        return false;
    }
}
