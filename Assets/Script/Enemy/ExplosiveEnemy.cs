using System.Collections;
using UnityEngine;

public class ExplosiveEnemy : MonoBehaviour, ITimeAffectable // 🔥 ITimeAffectable 추가
{
    public float detectionRadius = 5f;
    public float explosionRadius = 1f;
    public float moveSpeed = 3f;
    private Transform target;
    private Transform princess;
    private Transform player;
    private bool isActivated = false;
    private bool isExploding = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isAggroOnPlayer = false;
    public float aggroDuration = 5f;

    private Rigidbody2D rb;
    private bool isTimeStopped = false; // 🔥 시간 정지 변수 추가
    private Color originalColor;
    private Material originalMaterial;
    private Material grayScaleMaterial;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        originalColor = spriteRenderer.color;
        originalMaterial = spriteRenderer.material;
        grayScaleMaterial = Resources.Load<Material>("GrayScaleMaterial");

        princess = GameObject.FindGameObjectWithTag("Princess").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        target = princess;
    }

    void Update()
    {
        if (isTimeStopped) return; // 🔥 시간 정지 상태면 움직이지 않음

        if (isExploding) return;

        if (!isActivated && !isAggroOnPlayer)
        {
            DetectTarget();
        }

        if (isActivated && target != null)
        {
            MoveTowardsTarget();
        }
    }

    void DetectTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Princess"))
            {
                target = princess;
                Activate();
                break;
            }
        }
    }

    void Activate()
    {
        isActivated = true;
        animator.SetTrigger("Walk");
    }

    void MoveTowardsTarget()
    {
        if (Vector2.Distance(transform.position, target.position) <= explosionRadius)
        {
            StartCoroutine(Explode());
        }
        else
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            rb.velocity = direction * moveSpeed;
        }
    }

    private IEnumerator Explode()
    {
        isExploding = true;
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Explode");

        yield return new WaitForSeconds(1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerOver playerScript = hit.GetComponent<PlayerOver>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage();
                }
            }
            else if (hit.CompareTag("Princess"))
            {
                Princess princessScript = hit.GetComponent<Princess>();
                if (princessScript != null)
                {
                    princessScript.GameOver();
                }
            }
        }

        Destroy(gameObject);
    }

    public void AggroPlayer()
    {
        if (isAggroOnPlayer) return;

        isAggroOnPlayer = true;
        isActivated = true;
        target = player;
        animator.SetTrigger("Walk");

        Invoke(nameof(ResetAggro), aggroDuration);
    }

    private void ResetAggro()
    {
        isAggroOnPlayer = false;
        target = princess;

        if (Vector2.Distance(transform.position, princess.position) <= detectionRadius)
        {
            Activate();
        }
    }

    void OnDestroy()
    {
        TimeStopController timeStopController = FindObjectOfType<TimeStopController>();
        if (timeStopController != null)
        {
            timeStopController.RemoveTimeAffectedObject(this);
        }
    }

    public void StopTime()
    {
        isTimeStopped = true;
        rb.velocity = Vector2.zero;
        // rb.simulated = false; // 이 줄을 제거하여, collider가 활성 상태로 남도록 함.
        animator.speed = 0;
        spriteRenderer.material = grayScaleMaterial;
    }


    public void ResumeTime()
    {
        isTimeStopped = false;
        rb.simulated = true;
        animator.speed = 1;
        spriteRenderer.material = originalMaterial;
    }

    public void RestoreColor()
    {
        spriteRenderer.material = originalMaterial;
    }
}
