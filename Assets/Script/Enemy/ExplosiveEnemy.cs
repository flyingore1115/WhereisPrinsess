using UnityEngine;

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

    private bool isTimeStopped = false; //시간정지
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
            DetectTarget(); // 공주 감지
        }

        if (isActivated && target != null)
        {
            MoveTowardsTarget(); // 타겟 추적
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
        //SoundManager.Instance.PlaySFX("utteranceSound");
        //SoundManager.Instance.PlaySFX("activationSound");
    }

    void MoveTowardsTarget()
    {
        if (Vector2.Distance(transform.position, target.position) <= explosionRadius)
        {
            StartCoroutine(Explode());
        }
        else
        {
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }

    private System.Collections.IEnumerator Explode()
    {
        isExploding = true;
        animator.SetTrigger("Explode");
        //SoundManager.Instance.PlaySFX("explosionSound");

        yield return new WaitForSeconds(1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerOver playerScript = hit.GetComponent<PlayerOver>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(); // 데미지 호출
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
        isActivated = true; // 플레이어에게 어그로가 끌릴 때 즉시 활성화
        target = player; // 타겟을 플레이어로 변경

        animator.SetTrigger("Walk"); // 걷기 애니메이션 트리거

        SoundManager.Instance.PlaySFX("utteranceSound");
        SoundManager.Instance.PlaySFX("activationSound");

        Invoke(nameof(ResetAggro), aggroDuration); // 일정 시간 후 복구
    }

    private void ResetAggro()
    {
        isAggroOnPlayer = false;
        target = princess; // 타겟을 다시 공주로 변경

        // 활성화 상태를 유지
        if (Vector2.Distance(transform.position, princess.position) <= detectionRadius)
        {
            Activate();
        }
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
