using UnityEngine;

public class PrincessMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float princessRange;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Princess moves to the right continuously
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    }
    void OnDrawGizmos()
    {
        // 공격 범위를 표시하기 위해 빨간색 원으로 그리기
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, princessRange);
    }
}
