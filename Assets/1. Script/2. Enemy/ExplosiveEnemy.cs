using UnityEngine;
using System.Collections;

public class ExplosiveEnemy : BaseEnemy
{
    public float detectionRadius = 5f;
    public float explosionRadius = 1f;
    public float moveSpeed = 3f;
    private bool isActivated = false;    // 한 번 감지되면 true로 변함
    private bool isExploding = false;      // 폭발 진행 중 여부

    // 활성화 상태 확인 프로퍼티
    public bool IsActivated { get { return isActivated; } }

    public GameObject explosionParticlePrefab;


    protected override void Awake()
    {
        base.Awake();
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    void Update()
    {
        // 시간 정지나 폭발 중이면 업데이트하지 않음
        if (isTimeStopped || isExploding) return;
        if (isStunned) return;

        // 감지 전이면 타겟 감지 진행
        if (!isActivated && !isAggroOnPlayer)
        {
            DetectTarget();
        }

        // 감지 상태이면 공주를 향해 움직임
        if (isActivated && princess != null)
        {
            MoveTowardsTarget();
        }
    }

    void DetectTarget()
    {
        // 지정된 반경 내의 Collider들을 검사
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Princess"))
            {
                isActivated = true;
                break;
            }
            else if (hit.CompareTag("Player") && isAggroOnPlayer)
            {
                isActivated = true;
                break;
            }
        }
    }

    void MoveTowardsTarget()
    {
        Transform target = isAggroOnPlayer ? player : princess;
        if (target == null) return;

        Vector2 direction = target.position - transform.position;

        float dx = target.position.x - transform.position.x;
        // if dx < 0 → 왼쪽, 오른쪽 바라보게 하려면 flipX=false
        spriteRenderer.flipX = (dx > 0);
        // or if (dx < 0) spriteRenderer.flipX=false; else true;


        // 2) 폭발 범위 체크
        if (Vector2.Distance(transform.position, target.position) <= explosionRadius)
        {
            StartCoroutine(Explode());
        }
        else
        {
            transform.position = Vector2.MoveTowards(
                transform.position, 
                target.position, 
                moveSpeed * Time.deltaTime
            );
            if (animator != null)
            {
                animator.SetBool("isWalking", true);
            }
        }
    }


    private IEnumerator Explode()
    {
        isExploding = true;

        // 폭발 이펙트
    if (explosionParticlePrefab != null)
    {
        GameObject effect = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
        
        // 파티클 정렬 설정 (플레이어보다 앞으로 나오게)
        ParticleSystemRenderer psr = effect.GetComponent<ParticleSystemRenderer>();
        if (psr != null)
        {
            psr.sortingOrder = 100;
        }
    }

    SoundManager.Instance?.PlaySFX("EnemyExplosionSound");


        yield return new WaitForSeconds(0.5f);

        // 폭발 범위 내의 모든 오브젝트에 데미지 처리 (예: 플레이어에게)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerOver playerScript = hit.GetComponent<PlayerOver>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(1);
                }
            }
        }

        // 폭발 후 적을 파괴하는 대신, 죽음 상태 표시 및 비활성화 처리
        isDead = true;
        Die();
    }

    public override void StopTime()
    {
        base.StopTime();
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }

    public override void ResumeTime()
    {
        base.ResumeTime();
        if (animator != null && isActivated)
        {
            animator.SetBool("isWalking", true);
        }
    }

    /// <summary>
    /// ResetOnRewind()는 되감기 또는 체크포인트 복원 시 호출되어,
    /// 적의 내부 상태(isActivated, isExploding 등)를 감지 전 상태로 초기화합니다.
    /// 이를 통해 되감기 후 공주를 다시 감지해야 적이 움직이게 됩니다.
    /// </summary>
    public void ResetOnRewind()
    {
        isActivated = false;
        isExploding = false;
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }
}
