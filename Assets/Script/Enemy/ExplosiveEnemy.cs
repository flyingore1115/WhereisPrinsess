using UnityEngine;
using System.Collections;

public class ExplosiveEnemy : BaseEnemy
{
    public float detectionRadius = 5f;
    public float explosionRadius = 1f;
    public float moveSpeed = 3f;
    private bool isActivated = false;
    private bool isExploding = false;

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        if (isTimeStopped || isExploding) return;

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
            if (hit.CompareTag("Princess"))
            {
                isActivated = true;
                break;
            }
        }
    }

    void MoveTowardsTarget()
    {
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

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    public override void StopTime()
    {
        base.StopTime();
        if (animator != null)
        {
            animator.SetBool("isWalking", false); // 정지 상태에서 걷기 멈춤
        }
    }

    public override void ResumeTime()
    {
        base.ResumeTime();
        if (animator != null && isActivated)
        {
            animator.SetBool("isWalking", true); // 시간 정지 해제 후 걷기 재개
        }
    }
}
