using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRadius = 1.5f; // 상호작용 반경
    public LayerMask interactableLayer; // Interactable 전용 레이어

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // 플레이어 주변 원형 범위 안의 모든 충돌체 탐색
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactableLayer);

        if (hits.Length == 0)
        {
            Debug.Log("상호작용 가능한 오브젝트가 근처에 없습니다.");
            return;
        }

        // 가장 가까운 Interactable 찾기
        Interactable closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Interactable target = hit.GetComponent<Interactable>();
            if (target != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = target;
                }
            }
        }

        if (closest != null)
        {
            closest.Interact();
        }
    }

    // 씬에서 상호작용 반경 확인용
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
