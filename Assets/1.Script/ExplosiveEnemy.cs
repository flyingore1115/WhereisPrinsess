using UnityEngine;

public class ExplosiveEnemy : MonoBehaviour
{
    public float detectionRadius = 5f; // 감지 반경
    public float explosionRadius = 1f; // 폭발 반경
    public float moveSpeed = 3f; // 추적 속도
    public Sprite idleSprite; // 가만히 있는 이미지
    public Sprite activeSprite; // 일어서는 이미지
    public Color explosionColor = Color.red; // 폭발 시 색상

    private Transform target; // 추적 대상 (플레이어 또는 공주)
    private bool isActivated = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = idleSprite; // 시작 시 가만히 있는 이미지로 설정
    }

    void Update()
    {
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
            if (hit.CompareTag("Player") || hit.CompareTag("Princess"))
            {
                target = hit.transform; // 첫 번째로 감지된 플레이어 또는 공주를 추적 대상으로 설정
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

        // 폭발 반경에 들어오면 폭발
        if (distanceToTarget <= explosionRadius)
        {
            Explode();
        }
        else
        {
            // 추적 대상 쪽으로 이동
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }

    // 폭발 함수
    void Explode()
    {
        spriteRenderer.color = explosionColor; // 색상 변경

        if (target.CompareTag("Princess"))
        {
            Princess princessScript = target.GetComponent<Princess>();
            if (princessScript != null)
            {
                princessScript.GameOver(); // 공주 게임 오버 함수 호출
            }
        }

        Debug.Log("ExplosiveEnemy 폭발!"); // 콘솔에 폭발 메시지 출력
        Destroy(gameObject, 0.5f); // 적을 0.5초 후에 제거하여 색상 변경이 보이도록 함
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
