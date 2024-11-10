using UnityEngine;

public class ExplosiveEnemy : MonoBehaviour
{
    public float detectionRadius = 5f; // ���� �ݰ�
    public float explosionRadius = 1f; // ���� �ݰ�
    public float moveSpeed = 3f; // ���� �ӵ�
    public Sprite idleSprite; // ������ �ִ� �̹���
    public Sprite activeSprite; // �Ͼ�� �̹���
    public Color explosionColor = Color.red; // ���� �� ����

    private Transform target; // ���� ��� (�÷��̾� �Ǵ� ����)
    private bool isActivated = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = idleSprite; // ���� �� ������ �ִ� �̹����� ����
    }

    void Update()
    {
        if (!isActivated)
        {
            CheckForTargets(); // ���� �ݰ� �ȿ� �ִ��� Ȯ��
        }

        if (isActivated && target != null)
        {
            MoveTowardsTarget(); // Ȱ��ȭ�� ��� ���� ��� ������ �̵�
        }
    }

    // ���� �ݰ� �ȿ� �ִ� ����� Ȯ���Ͽ� ���� ������� ����
    void CheckForTargets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Princess"))
            {
                target = hit.transform; // ù ��°�� ������ �÷��̾� �Ǵ� ���ָ� ���� ������� ����
                ActivateEnemy(); // �� Ȱ��ȭ
                break;
            }
        }
    }

    // ���� Ȱ��ȭ�ϰ� �̹����� ����
    void ActivateEnemy()
    {
        isActivated = true;
        spriteRenderer.sprite = activeSprite;
    }

    // ���� ��󿡰� �ٰ�����
    void MoveTowardsTarget()
    {
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // ���� �ݰ濡 ������ ����
        if (distanceToTarget <= explosionRadius)
        {
            Explode();
        }
        else
        {
            // ���� ��� ������ �̵�
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }

    // ���� �Լ�
    void Explode()
    {
        spriteRenderer.color = explosionColor; // ���� ����

        if (target.CompareTag("Princess"))
        {
            Princess princessScript = target.GetComponent<Princess>();
            if (princessScript != null)
            {
                princessScript.GameOver(); // ���� ���� ���� �Լ� ȣ��
            }
        }

        Debug.Log("ExplosiveEnemy ����!"); // �ֿܼ� ���� �޽��� ���
        Destroy(gameObject, 0.5f); // ���� 0.5�� �Ŀ� �����Ͽ� ���� ������ ���̵��� ��
    }

    // ���� �ݰ��� �ð������� Ȯ�� (������ ����)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
