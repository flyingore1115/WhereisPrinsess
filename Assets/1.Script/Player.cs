using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public float attackRange = 3f;
    public float attackMoveSpeed = 15f; // 빠른 이동 속도

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        playerMove();
    }

    private void playerMove()
    {
        if (!isAttacking)
        {
            // Horizontal movement
            float moveInput = Input.GetAxis("Horizontal");
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

            // Check if grounded
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            // Jump
            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        // Check for mouse click and enemy within range
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                float distanceToEnemy = Vector2.Distance(transform.position, hit.collider.transform.position);
                if (distanceToEnemy <= attackRange)
                {
                    // Move player to enemy position and destroy enemy
                    StartCoroutine(MoveToEnemyAndAttack(hit.collider.gameObject));
                }
            }
        }
    }

    private System.Collections.IEnumerator MoveToEnemyAndAttack(GameObject enemy)
    {
        isAttacking = true;
        rb.gravityScale = 0; // 중력 비활성화

        while (Vector2.Distance(transform.position, enemy.transform.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, enemy.transform.position, attackMoveSpeed * Time.deltaTime);
            yield return null;
        }

        // Destroy the enemy
        Destroy(enemy);

        rb.gravityScale = 1; // 중력 다시 활성화
        isAttacking = false;
    }

    void OnDrawGizmos()
    {
        // 공격 범위를 표시하기 위해 빨간색 원으로 그리기
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
