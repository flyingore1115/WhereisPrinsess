using UnityEngine;

public class Princess : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 플레이어 오브젝트를 찾아 충돌 비활성화
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider2D>();
            Collider2D princessCollider = GetComponent<Collider2D>();

            if (playerCollider != null && princessCollider != null)
            {
                Physics2D.IgnoreCollision(princessCollider, playerCollider);
            }
        }
    }

    void Update()
    {
        // Princess moves to the right continuously
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    }
}
