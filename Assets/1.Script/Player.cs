using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public float attackRange = 3f;
    public float attackMoveSpeed = 15f;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private bool isGrounded;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!isAttacking)
        {
            float moveInput = Input.GetAxis("Horizontal");
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                float distanceToEnemy = Vector2.Distance(transform.position, hit.collider.transform.position);
                if (distanceToEnemy <= attackRange)
                {
                    StartCoroutine(MoveToEnemyAndAttack(hit.collider.gameObject));
                }
            }
        }
    }

    private System.Collections.IEnumerator MoveToEnemyAndAttack(GameObject enemy)
    {
        isAttacking = true;
        
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        playerCollider.enabled = false;

        // 적 위치로 이동
        while (Vector2.Distance(transform.position, enemy.transform.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, enemy.transform.position, attackMoveSpeed * Time.deltaTime);
            yield return null;
        }

        // 적 제거
        Destroy(enemy);

        // 공격 후 플레이어 위치를 적의 위치에서 멀어지도록 약간 조정
        Vector2 escapeDirection = (transform.position - enemy.transform.position).normalized;
        transform.position += (Vector3)escapeDirection * 0.5f;

        // 충돌, 중력 및 속도 복원
        playerCollider.enabled = true;
        rb.gravityScale = 1;
        isAttacking = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
