using UnityEngine;
using System.Collections;

public class Princess : MonoBehaviour, ITimeAffectable
{
    public float moveSpeed = 3f; // 공주 이동 속도
    public Transform groundCheck; // 발 아래 확인 위치
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f); // 박스 크기
    public LayerMask groundLayer; // 땅 레이어
    public float fallThreshold = 0.2f; // 구멍에서 떨어지기 전의 허용 거리

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded = false;
    private bool isGameOver = false;
    private bool isFalling = false; // 낙하 여부 확인
    private bool isTimeStopped = false; //시간정지

    private bool isShieldActive = false;
    private int extraLives = 0; // 여벌 목숨
    
//흑백효과
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
        if (!isShieldActive && other.CompareTag("Enemy"))
        {
            if (extraLives > 0)
            {
                extraLives--; // 여벌 목숨이 있으면 죽지 않음
                Debug.Log("Princess survived using extra life!");
                return;
            }
            Debug.Log("Princess was defeated!");
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

//스킬
    public void EnableShield(float duration, bool maxLevel)
    {
        if (isShieldActive) return;

        isShieldActive = true;
        spriteRenderer.color = Color.cyan; // 보호막 활성화 표시
        StartCoroutine(DisableShieldAfterTime(duration));

        if (maxLevel) extraLives++; // MAX 레벨일 경우 여벌 목숨 추가
    }

    private IEnumerator DisableShieldAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        isShieldActive = false;
        spriteRenderer.color = originalColor;
    }


//이하 시간정지
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