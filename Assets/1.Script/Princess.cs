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
        // ���� ������ ǥ���ϱ� ���� ������ ������ �׸���
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, princessRange);
    }
}
