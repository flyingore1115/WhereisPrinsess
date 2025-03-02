using UnityEngine;
using System.Collections;

public class ExplosiveEnemy : MonoBehaviour, ITimeAffectable
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

    private bool isTimeStopped = false;
    private Color originalColor;

    public Material grayscaleMaterial; // 기본 그레이스케일 Material (프리팹 또는 에셋)
    private Material originalMaterial;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 원본 Material은 인스턴스화하지 않은 상태로 저장합니다.
        originalMaterial = spriteRenderer.sharedMaterial;
        animator = GetComponent<Animator>();
        originalColor = spriteRenderer.color;
        princess = GameObject.FindGameObjectWithTag("Princess").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        target = princess;
    }

    void Update()
    {
        if (isTimeStopped) return;
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
            Vector2 direction = target.position - transform.position;
            bool defaultFacingRight = false;
            if (defaultFacingRight)
                spriteRenderer.flipX = (direction.x < 0);
            else
                spriteRenderer.flipX = (direction.x > 0);

            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }

    private IEnumerator Explode()
    {
        isExploding = true;
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

        Vector2 direction = player.position - transform.position;
        bool defaultFacingRight = false;
        if (defaultFacingRight)
            spriteRenderer.flipX = (direction.x < 0);
        else
            spriteRenderer.flipX = (direction.x > 0);

        SoundManager.Instance.PlaySFX("utteranceSound");
        SoundManager.Instance.PlaySFX("activationSound");
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

    public void SlowDown(float duration)
    {
        if (!isTimeStopped)
        {
            moveSpeed /= 2;
            Invoke("RestoreSpeed", duration);
        }
    }

    private void RestoreSpeed()
    {
        moveSpeed *= 2;
    }

    public void TakeDamage()
    {
        Debug.Log("[Enemy] Took Damage!");
        Destroy(gameObject);
    }
    public void StopTime()
    {
        if (this == null || spriteRenderer == null) return;
        
        isTimeStopped = true;
        if (grayscaleMaterial != null)
        {
            spriteRenderer.material = grayscaleMaterial;
        }
        if (animator != null)
        {
            animator.speed = 0;
        }
    }


    public void ResumeTime()
    {
        if (this == null || spriteRenderer == null) return;
        
        isTimeStopped = false;
        if (animator != null)
        {
            animator.speed = 1;
        }
        RestoreColor();
    }


    public void RestoreColor()
    {
        if (this == null || spriteRenderer == null) return;
        spriteRenderer.material = originalMaterial;
    }
}
