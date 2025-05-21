using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// [Singleton] 모든 씬에서 Player 또는 지정된 타깃을 부드럽게 따라가며,
/// 다이얼로그 중에는 스토리 모드로 천천히 스무스하게 이동하고,
/// 게임 플레이 시에는 블러 없이 즉시 따라오도록 지원합니다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Tooltip("컷신용 기본 타깃 (예: NPC, 다이얼로그 대상)")]
    public GameObject defaultTarget;

    [Header("Follow Settings")]
    [Tooltip("다이얼로그/스토리 모드 시 스무스 이동 계수")]
    [Range(1f, 20f)] public float followSmooth = 5f;
    [Tooltip("게임 플레이 시 플레이어에 즉시 고정 (블러 방지)")]
    public bool immediateFollowInGame = true;

    [Header("Orthographic Size")]
    [Tooltip("평상시 카메라 크기")]
    public float defaultSize = 5f;

    private Camera cam;
    private Transform currentTarget;
    private bool storyMode = false;

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

        cam = GetComponent<Camera>();
        cam.orthographicSize = defaultSize;

        if (defaultTarget != null)
            currentTarget = defaultTarget.transform;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

void Update()
{
    if (MySceneManager.Instance != null &&
        (MySceneManager.IsStoryScene || MySceneManager.IsMainMenu))
    {
        MouseManager.Instance.SetCursor(CursorModeType.Default);
        return;
    }

    Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    wp.z = 0f;

    RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);

    // Enemy 인식 범위 확장 (부모도 검사)
    bool isEnemy = hit.collider != null &&
        (hit.collider.CompareTag("Enemy") ||
         hit.collider.GetComponentInParent<Transform>()?.CompareTag("Enemy") == true);

    MouseManager.Instance.SetCursor(isEnemy ? CursorModeType.Attack : CursorModeType.Shoot);
}


    void LateUpdate()
    {
        if (currentTarget == null) return;

        Vector3 tgt = currentTarget.position;
        tgt.z = transform.position.z;

        if (!storyMode && immediateFollowInGame)
        {
            // 게임 플레이 모드: 즉시 위치 동기화 (블러/모션블러 방지)
            transform.position = tgt;
        }
        else
        {
            // 스토리/다이얼로그 모드: 부드럽게 보간
            transform.position = Vector3.Lerp(
                transform.position,
                tgt,
                Time.deltaTime * followSmooth);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // StoryMode 유지 중이면 defaultTarget으로 고정
        if (storyMode)
        {
            if (defaultTarget != null)
                currentTarget = defaultTarget.transform;
            return;
        }

        // StoryMode 해제 시 플레이어로 자동 복귀
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        currentTarget = playerObj ? playerObj.transform : null;
        cam.orthographicSize = defaultSize;
    }

    /// <summary>
    /// 임시로 카메라 타깃을 지정 (GameObject 버전)
    /// </summary>
    public void SetTarget(GameObject go, float? newSize = null)
    {
        if (go == null) return;
        SetTarget(go.transform, newSize);
    }

    /// <summary>
    /// 임시로 카메라 타깃을 지정 (Transform 버전)
    /// </summary>
    public void SetTarget(Transform target, float? newSize = null)
    {
        if (target == null) return;
        currentTarget = target;
        if (newSize.HasValue)
            cam.orthographicSize = newSize.Value;
    }

    /// <summary>
    /// StoryMode on/off 설정
    /// </summary>
    public void EnableStoryMode(bool enable)
    {
        storyMode = enable;
        if (enable && defaultTarget != null)
            currentTarget = defaultTarget.transform;
        else if (!enable)
        {
            // 다이얼로그 종료 시 자동 복귀
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            currentTarget = p ? p.transform : null;
            cam.orthographicSize = defaultSize;
        }
    }

    /// <summary>
    /// 즉시 카메라 크기만 변경
    /// </summary>
    public void SetCameraSize(float newSize)
    {
        cam.orthographicSize = newSize;
    }
}
