using UnityEngine;
using System.Collections;

public class P_Movement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // 대쉬 관련 변수
    public float dashDistance = 3f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1.5f;
    public float invincibilityTime = 0.3f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform firePoint;
    private bool isGrounded;
    public bool IsGrounded => isGrounded;

    public bool canDash { get; private set; } = true;
    private bool isDashing = false;
    private bool isInvincible = false;
    private float dodgeChance = 0f;

    // 참조용: Player 컴포넌트
    private Player player;

    // Init 함수 (public)
    public void Init(Rigidbody2D rb, Animator animator, SpriteRenderer spriteRenderer, Transform firePoint)
    {
        this.rb = rb;
        this.animator = animator;
        this.spriteRenderer = spriteRenderer;
        this.firePoint = firePoint;
        // Player 컴포넌트는 같은 GameObject에 있으므로 가져옵니다.
        player = Player.Instance;
    }

    void Update()
    {
        // 입력 무시 플래그가 true이면, 이동 입력을 아예 무시하고 Idle 상태 유지
        if (player != null && player.ignoreInput)
        {
            animator.SetBool("isRun", false);
            return;
        }

        // 대쉬 중이면 일반 이동 처리하지 않음
        if (!isDashing)
        {
            HandleMovement(attackIsActive: false);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash &&
            (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        // → 입력 여부와 상관없이 항상 지면 감지
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void HandleMovement(bool attackIsActive)
    {
        if (attackIsActive) return;

        // ─ 1) 점프 먼저 처리 ─
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);      // 잔여 Y 제거
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            SoundManager.Instance?.PlaySFX("playerJumpSound");
        }

        float moveInput = Input.GetAxisRaw("Horizontal");
    // holding 상태면 속도 50% 적용
        float speedMultiplier = Player.Instance.holdingPrincess ? 0.5f : 1f;
        Vector2 movementVector = new Vector2(moveInput * moveSpeed * speedMultiplier, rb.linearVelocity.y);
        rb.linearVelocity = movementVector;

        if (moveInput != 0)
        {
            animator.SetBool("isRun", true);
            spriteRenderer.flipX = moveInput < 0;
            firePoint.localPosition = new Vector3(
                Mathf.Abs(firePoint.localPosition.x) * (spriteRenderer.flipX ? -1 : 1),
                firePoint.localPosition.y,
                firePoint.localPosition.z
            );
        }
        else
        {
            animator.SetBool("isRun", false);
        }
    }

    IEnumerator Dash()
    {
        Debug.Log("대쉬함");
        canDash = false;
        isDashing = true;
        isInvincible = true;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 dashDirection = new Vector2(moveX, moveY).normalized;
        if (dashDirection == Vector2.zero)
        {
            isDashing = false;
            isInvincible = false;
            canDash = true;
            yield break;
        }

        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + (dashDirection * dashDistance);

        int obstacleLayer = LayerMask.GetMask("Obstacle");
        RaycastHit2D hit = Physics2D.Raycast(startPosition, dashDirection, dashDistance, obstacleLayer);
        if (hit.collider != null)
        {
            targetPosition = hit.point;
        }

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsed / dashDuration);
            yield return null;
        }
        transform.position = targetPosition;

        isDashing = false;
        yield return new WaitForSecondsRealtime(invincibilityTime);
        isInvincible = false;
        yield return new WaitForSecondsRealtime(dashCooldown - invincibilityTime);
        canDash = true;
    }

    public void EnableDash()
    {
        Debug.Log("[Player] Dash Unlocked!");
        canDash = true;
    }

    public void SetDodgeChance(float chance)
    {
        dodgeChance = chance;
    }

    public bool TryDodge()
    {
        if (Random.value < dodgeChance)
        {
            Debug.Log("[Skill] Dodge successful!");
            return true;
        }
        return false;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

// 기존 ResetInput() 교체
public void ResetInput()
{
    // 잔여 속도 제거
    rb.linearVelocity = Vector2.zero;
    // 대쉬 중이었으면 해제
    isDashing = false;
}

}
