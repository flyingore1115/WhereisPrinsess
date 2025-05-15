using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DropShadowRaycast : MonoBehaviour
{
    [Tooltip("그림자가 따라다닐 대상(플레이어)의 Transform")]
    public Transform target;

    [Tooltip("Raycast를 쏠 LayerMask (Ground 등)")]
    public LayerMask groundLayer;

    [Tooltip("그림자 크기 기본값 (땅에 붙어 있을 때)")]
    public Vector3 baseScale = new Vector3(1f, 1f, 1f);

    [Tooltip("부모(플레이어) 높이 차에 따른 축소 비율")]
    public float scaleFactor = 0.1f;

    [Tooltip("축소 최소값")]
    public float minScale = 0.3f;

    [Tooltip("Raycast를 쏠 길이(플레이어가 맵 위로 점프할 수 있는 높이보다 더 길게)")]
    public float raycastDistance = 10f;

    [Tooltip("플레이어 기준으로 Ray를 어디서 쏠지 오프셋")]
    public Vector2 raycastOffset = new Vector2(0, 0);

    // 내부 변수
    private SpriteRenderer spr;

    void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
        if (!spr)
            spr = gameObject.AddComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (!target) return;

        // (1) 플레이어 기준 아래 방향(RaycastOffset 보정)으로 Raycast
        Vector2 rayOrigin = new Vector2(target.position.x + raycastOffset.x,
                                        target.position.y + raycastOffset.y);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, raycastDistance, groundLayer);
        
        if (hit.collider != null)
        {
            // (2) 실제 땅의 Y좌표를 얻음
            float groundY = hit.point.y;

            // (3) 그림자 위치는 x는 플레이어, y는 groundY
            Vector3 newPos = new Vector3(target.position.x, groundY, transform.position.z);
            transform.position = newPos;

            // (4) 높이 차(= target.position.y - groundY)에 따라 스케일 줄이기
            float heightDiff = target.position.y - groundY;
            float scaleValue = Mathf.Max(minScale, 1f - (scaleFactor * heightDiff));
            transform.localScale = baseScale * scaleValue;
        }
        else
        {
            // 땅을 찾지 못한 경우(플레이어가 맵 위로 많이 날아갔거나…)
            // 예를 들어, 그림자를 숨기거나 특정 처리 가능
            spr.enabled = false; 
        }
    }
}
