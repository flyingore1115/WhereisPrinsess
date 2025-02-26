using UnityEngine;
using System.Collections;

public class ExplosiveEnemy : MonoBehaviour, ITimeAffectable
{
    public float detectionRadius = 5f; // 감지 반경
    public float explosionRadius = 1f; // 폭발 반경
    public float moveSpeed = 3f; // 이동 속도
    private Transform target; // 추적 대상
    private Transform princess; // 공주
    private Transform player; // 플레이어
    private bool isActivated = false;
    private bool isExploding = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isAggroOnPlayer = false; // 플레이어에게 어그로 여부
    public float aggroDuration = 5f; // 플레이어 어그로 지속 시간

    private bool isTimeStopped = false; // 시간정지 상태
    private Color originalColor;

    public Material grayscaleMaterial;
    private Material originalMaterial;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
        animator = GetComponent<Animator>();
        originalColor = spriteRenderer.color;
        princess = GameObject.FindGameObjectWithTag("Princess").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        target = princess; // 기본 타겟은 공주
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
            // 매 프레임 목표 방향을 계산하여 적이 올바르게 바라보도록 함.
            // NOTE: 아래 flipX 조건은 스프라이트의 기본 방향에 따라 조정해야 함.
            // 만약 스프라이트가 기본적으로 왼쪽을 바라보고 있다면, 아래처럼 설정.
            bool defaultFacingRight = false; // set true if enemy sprite is drawn facing right by default.
            if (defaultFacingRight)
            {
                spriteRenderer.flipX = (direction.x < 0);
            }
            else
            {
                spriteRenderer.flipX = (direction.x > 0);
            }

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

        // 초기 방향 설정 (한 번만 설정함; 이후에는 MoveTowardsTarget()에서 계속 업데이트)
        Vector2 direction = player.position - transform.position;
        bool defaultFacingRight = false; // adjust based on sprite default direction.
        if (defaultFacingRight)
        {
            spriteRenderer.flipX = (direction.x < 0);
        }
        else
        {
            spriteRenderer.flipX = (direction.x > 0);
        }

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
            moveSpeed /= 2; // 이동 속도를 절반으로 감소
            Invoke("RestoreSpeed", duration);
        }
    }

    private void RestoreSpeed()
    {
        moveSpeed *= 2; // 원래 속도로 복구
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
