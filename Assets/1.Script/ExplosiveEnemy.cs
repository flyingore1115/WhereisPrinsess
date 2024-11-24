using UnityEngine;

public class ExplosiveEnemy : MonoBehaviour
{
    public float detectionRadius = 5f; // ���� �ݰ�
    public float explosionRadius = 1f; // ���� �ݰ�
    public float moveSpeed = 3f; // ���� �ӵ�
    public Sprite idleSprite; // ������ �ִ� �̹���
    public Sprite activeSprite; // �Ͼ�� �̹���
    public Color explosionColor = Color.red; // ���� �� ����

    private Transform target; // ���� ��� (���ָ� ���)
    private bool isActivated = false;
    private bool isExploding = false; // ���� ������ ����
    private SpriteRenderer spriteRenderer;
    private Animator animator; // ���� �ִϸ��̼� ����

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        spriteRenderer.sprite = idleSprite; // ���� �� ������ �ִ� �̹����� ����
    }

    void Update()
    {
        if (isExploding) return; // ���� �߿��� ������Ʈ ����

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
            if (hit.CompareTag("Princess")) // ���ָ� ����
            {
                target = hit.transform;
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

        // ���� �ݰ濡 ������ ���� �غ�
        if (distanceToTarget <= explosionRadius)
        {
            StartCoroutine(PrepareToExplode()); // ���� �غ� �ڷ�ƾ ����
        }
        else
        {
            // ���� ��� ������ �̵�
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }

    // ���� �غ� �ڷ�ƾ
    private System.Collections.IEnumerator PrepareToExplode()
    {
        if (isExploding) yield break; // �ߺ� ���� ����

        isExploding = true;

        // ���� �ִϸ��̼� Ʈ����
        if (animator != null)
        {
            animator.SetTrigger("Explode"); // ���� �ִϸ��̼� Ʈ���� ����
        }

        Debug.Log("ExplosiveEnemy is preparing to explode!");

        // 1�� ���
        yield return new WaitForSeconds(1f);

        Explode(); // ����
    }

    // ���� �Լ�
    void Explode()
    {
        Debug.Log("ExplosiveEnemy ����!");

        // ���ֿ� �浹 �� ���� ����
        if (target.CompareTag("Princess"))
        {
            Princess princessScript = target.GetComponent<Princess>();
            if (princessScript != null)
            {
                princessScript.GameOver(); // ���� ���� ���� ȣ��
            }
        }

        Destroy(gameObject); // ������Ʈ ����
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
