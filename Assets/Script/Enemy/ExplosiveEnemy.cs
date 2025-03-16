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

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        // 시간 정지나 폭발 중이면 업데이트하지 않음
        if (isTimeStopped || isExploding) return;

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
        // 어그로 상태이면 플레이어, 아니면 공주를 대상으로 함
        Transform target = isAggroOnPlayer ? player : princess;

        if (Vector2.Distance(transform.position, target.position) <= explosionRadius)
        {
            // 폭발 조건에 도달하면 폭발 코루틴 시작
            StartCoroutine(Explode());
        }
        else
        {
            // 목표 지점을 향해 이동
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
        gameObject.SetActive(false);
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
