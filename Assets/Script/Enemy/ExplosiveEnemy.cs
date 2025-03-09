using UnityEngine;
using System.Collections;

public class ExplosiveEnemy : BaseEnemy
{
    public float detectionRadius = 5f;
    public float explosionRadius = 1f;
    public float moveSpeed = 3f;
    private bool isActivated = false;
    private bool isExploding = false;

    // 추가: public 프로퍼티로 활성화 상태 확인
    public bool IsActivated { get { return isActivated; } }

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        if (isTimeStopped || isExploding) return;

        // 활성화되지 않았으면 타겟 감지
        if (!isActivated && !isAggroOnPlayer)
        {
            DetectTarget();
        }

        if (isActivated && princess != null)
        {
            MoveTowardsTarget();
        }
    }

    void DetectTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (Collider2D hit in hits)
        {
            // 공주가 감지되면 활성화
            if (hit.CompareTag("Princess"))
            {
                isActivated = true;
                break;
            }
            // 플레이어 어그로가 걸린 경우, 플레이어도 감지되면 활성화
            else if (hit.CompareTag("Player") && isAggroOnPlayer)
            {
                isActivated = true;
                break;
            }
        }
    }

    void MoveTowardsTarget()
    {
        // 대상: 어그로 상태면 플레이어, 아니면 공주
        Transform target = isAggroOnPlayer ? player : princess;

        if (Vector2.Distance(transform.position, target.position) <= explosionRadius)
        {
            StartCoroutine(Explode());
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            if (animator != null)
            {
                animator.SetBool("isWalking", true);
            }
        }
    }

    private IEnumerator Explode()
    {
        isExploding = true;
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        yield return new WaitForSeconds(1f); // 폭발 애니메이션 재생 시간

        // 폭발 범위 내의 모든 오브젝트에 대해 데미지 처리 (예시: 플레이어에게 데미지 1)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerOver player = hit.GetComponent<PlayerOver>();
                if (player != null)
                {
                    player.TakeDamage(1);  // 플레이어에게 데미지 1 적용
                }
            }
        }

        Destroy(gameObject);
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
}
