using UnityEngine;
using System.Collections.Generic;

public class SpriteTargetConnector : MonoBehaviour
{
    public static SpriteTargetConnector Instance;

    // 미리보기·고정 선에 쓸 프리팹 (SpriteRenderer.drawMode = Tiled, Pivot = Center)
    public GameObject segmentPrefab;

    // 선택된 적들의 Transform 저장
    private List<Transform> selectedTargets = new List<Transform>();

    // 이미 생성된 고정 선 목록
    private List<GameObject> fixedSegments = new List<GameObject>();

    // 미리보기 선 (마우스 따라 업데이트)
    private GameObject previewSegment;
    private SpriteRenderer previewSprite;
    private float spriteHeight;  // 스프라이트 높이

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (segmentPrefab == null)
        {
            Debug.LogError("[SpriteTargetConnector] segmentPrefab이 할당되지 않았습니다.");
        }
    }

    void Start()
    {
        CreatePreviewSegment();
    }

    void Update()
    {
        // 1) 파괴된(또는 이미 Destroy() 호출된) Transform 제거
        selectedTargets.RemoveAll(t => t == null);

        // 2) 시간정지 아니면 바로 리턴
        if (TimeStopController.Instance == null || !TimeStopController.Instance.IsTimeStopped)
        {
            if (previewSprite != null)
                previewSprite.enabled = false;
            return;
        }
        // 좌클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);
            // (A) 적(Enemy) 클릭 → 연결 처리
            if (hit.collider != null && (hit.collider.CompareTag("Enemy")|| hit.collider.CompareTag("TutorialTarget")))
            {
                Transform enemyTr = hit.collider.transform;
                // 중복 선택 방지
                if (!selectedTargets.Contains(enemyTr))
                {
                    AddTarget(enemyTr);
                }
            }
            // (B) 허공 클릭 → 전체 초기화
            else
            {
                ClearAllSegments();
            }
        }

        // 미리보기 선 업데이트: 마지막 선택된 타겟과 마우스 사이
        if (selectedTargets.Count > 0 && previewSegment != null)
        {
            // 드래그 상태: 처음 드래그 시 효과음 재생
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("Line_Drag");
            if (previewSprite != null)
            {
                // 드래그 선은 회색 상태로 표시
                previewSprite.color = Color.gray;
            }
            
            Transform lastTarget = selectedTargets[selectedTargets.Count - 1];
            Vector3 lastPos = lastTarget.position;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            UpdateSegment(previewSegment, lastPos, mousePos);
        }
        else
        {
            if (previewSprite != null)
                previewSprite.enabled = false;
        }
    }

    /// <summary>
    /// 적 Transform 추가 후, 이전 타겟과의 고정 선 생성 및 연결 효과 재생
    /// </summary>
    private void AddTarget(Transform targetTr)
    {
        if (selectedTargets.Count > 0)
        {
            Transform prevTr = selectedTargets[selectedTargets.Count - 1];
            GameObject fixedSeg = CreateFixedSegment(prevTr.position, targetTr.position);
            fixedSegments.Add(fixedSeg);
            // 연결 효과음 재생
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("Line_Connect");
        }
        selectedTargets.Add(targetTr);

        BaseEnemy enemy = targetTr.GetComponent<BaseEnemy>();
        if (enemy != null && TargetOrderManager.Instance != null)
        {
            TargetOrderManager.Instance.ForceAddTarget(enemy);
        }
    }

    /// <summary>
    /// 두 점을 연결하는 고정 선(세그먼트) 생성 (중앙에 배치)
    /// </summary>
    private GameObject CreateFixedSegment(Vector3 start, Vector3 end)
    {
        GameObject seg = Instantiate(segmentPrefab, transform);
        SpriteRenderer sr = seg.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("segmentPrefab에 SpriteRenderer가 없습니다.");
            return seg;
        }
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.enabled = true;

        float distance = Vector2.Distance(start, end);
        Vector3 midPoint = (start + end) * 0.5f;
        float originalHeight = sr.sprite.bounds.size.y;
        sr.size = new Vector2(distance, originalHeight);

        seg.transform.position = midPoint;
        Vector2 dir = end - start;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        seg.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 연결 선은 기본 색상(흰색)로 복귀
        sr.color = Color.white;
        return seg;
    }

    /// <summary>
    /// 미리보기 선(하나만 존재)을 생성
    /// </summary>
    private void CreatePreviewSegment()
    {
        previewSegment = Instantiate(segmentPrefab, transform);
        previewSprite = previewSegment.GetComponent<SpriteRenderer>();
        if (previewSprite == null)
        {
            Debug.LogError("segmentPrefab에 SpriteRenderer가 없습니다. (미리보기)");
            return;
        }
        previewSprite.drawMode = SpriteDrawMode.Tiled;
        previewSprite.enabled = false;
        spriteHeight = previewSprite.sprite.bounds.size.y;
    }

    /// <summary>
    /// 미리보기 선 갱신 (start와 end 사이 연결)
    /// </summary>
    private void UpdateSegment(GameObject seg, Vector3 start, Vector3 end)
    {
        SpriteRenderer sr = seg.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        sr.enabled = true;

        float distance = Vector2.Distance(start, end);
        Vector3 midPoint = (start + end) * 0.5f;
        seg.transform.position = midPoint;
        Vector2 dir = end - start;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        seg.transform.rotation = Quaternion.Euler(0, 0, angle);
        sr.size = new Vector2(distance, spriteHeight);
    }

    /// <summary>
    /// 전체 고정 선, 미리보기 선, 선택 상태 초기화 및 TargetOrderManager도 취소
    /// </summary>
    public void ClearAllSegments()
    {
        foreach (GameObject seg in fixedSegments)
        {
            Destroy(seg);
        }
        fixedSegments.Clear();
        selectedTargets.Clear();

        if (previewSegment != null)
        {
            Destroy(previewSegment);
        }
        CreatePreviewSegment();

        // TargetOrderManager도 취소하여 타겟 리스트 초기화
        if (TargetOrderManager.Instance != null)
            TargetOrderManager.Instance.CancelTargetOrdering();
    }

    /// <summary>
    /// 외부(예, 공격 완료 시) 호출: 선 및 선택 상태 초기화
    /// </summary>
    public void OnAttackFinished()
    {
        ClearAllSegments();
    }
    public void SetSelectedTargets(List<BaseEnemy> enemies)
    {
        // 기존 선택 상태 초기화
        ClearAllSegments();
        // 새로운 선택 목록 구성
        foreach (BaseEnemy enemy in enemies)
        {
            if (enemy != null)
            {
                AddTarget(enemy.transform);
            }
        }
    }

}
