using UnityEngine;

public class ExplosiveEnemy : MonoBehaviour
{
    public float detectionRadius = 5f; // 감지 반경
    public float explosionRadius = 1f; // 폭발 반경
    public float moveSpeed = 3f; // 추적 속도
    public Sprite idleSprite; // 가만히 있는 이미지
    public Sprite activeSprite; // 일어서는 이미지
    public Color explosionColor = Color.red; // 폭발 시 색상

    private Transform target; // 추적 대상 (공주만 대상)
    private bool isActivated = false;
    private bool isExploding = false; // 폭발 중인지 여부
    private SpriteRenderer spriteRenderer;
    private Animator animator; // 폭발 애니메이션 관리

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        spriteRenderer.sprite = idleSprite; // 시작 시 가만히 있는 이미지로 설정
    }

    void Update()
    {
        if (isExploding) return; // 폭발 중에는 업데이트 중지

        if (!isActivated)
        {
            CheckForTargets(); // 감지 반경 안에 있는지 확인
        }

        if (isActivated && target != null)
        {
            MoveTowardsTarget(); // 활성화된 경우 추적 대상 쪽으로 이동
        }
    }

    // 감지 반경 안에 있는 대상을 확인하여 추적 대상으로 설정
    void CheckForTargets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Princess")) // 공주만 추적
            {
                target = hit.transform;
                ActivateEnemy(); // 적 활성화
                break;
            }
        }
    }

    // 적을 활성화하고 이미지를 변경
    void ActivateEnemy()
    {
        isActivated = true;
        spriteRenderer.sprite = activeSprite;
    }

    // 추적 대상에게 다가가기
    void MoveTowardsTarget()
    {
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // 폭발 반경에 들어오면 폭발 준비
        if (distanceToTarget <= explosionRadius)
        {
            StartCoroutine(PrepareToExplode()); // 폭발 준비 코루틴 실행
        }
        else
        {
            // 추적 대상 쪽으로 이동
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }

    // 폭발 준비 코루틴
    private System.Collections.IEnumerator PrepareToExplode()
    {
        if (isExploding) yield break; // 중복 폭발 방지

        isExploding = true;

        // 폭발 애니메이션 트리거
        if (animator != null)
        {
            animator.SetTrigger("Explode"); // 폭발 애니메이션 트리거 설정
        }

        Debug.Log("ExplosiveEnemy is preparing to explode!");

        // 1초 대기
        yield return new WaitForSeconds(1f);

        Explode(); // 폭발
    }

    // 폭발 함수
    void Explode()
    {
        Debug.Log("ExplosiveEnemy 폭발!");

        // 공주와 충돌 시 게임 오버
        if (target.CompareTag("Princess"))
        {
            Princess princessScript = target.GetComponent<Princess>();
            if (princessScript != null)
            {
                princessScript.GameOver(); // 공주 게임 오버 호출
            }
        }

        Destroy(gameObject); // 오브젝트 삭제
    }

    // 감지 반경을 시각적으로 확인 (에디터 전용)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
