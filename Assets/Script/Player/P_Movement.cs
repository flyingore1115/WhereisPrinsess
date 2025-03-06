using UnityEngine;
using System.Collections;

public class P_Movement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform firePoint;
    private bool isGrounded;

    public bool canDash;
    private float dodgeChance = 0f;

    public void Init(Rigidbody2D rb, Animator animator, SpriteRenderer spriteRenderer, Transform firePoint)
    {
        this.rb = rb;
        this.animator = animator;
        this.spriteRenderer = spriteRenderer;
        this.firePoint = firePoint;
    }

    public void HandleMovement(bool isAttacking)
    {
        if (isAttacking) return;

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

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

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX("playerJumpSound");
            }
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
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
}
