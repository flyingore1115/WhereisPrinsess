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

    [Header("Follow Offset")]
    public Vector3 followOffset = new Vector3(-2f, 1f, 0f);
    public Vector3 storyModeOffset = new Vector3(-2f, 0f, 0f);

    private Camera cam;
    private Transform currentTarget;
    private bool storyMode = false;

    public float GetCurrentSize() => cam.orthographicSize;

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
        SetCameraSize(defaultSize);

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

        // 기존 target 위치 대신 offset을 더한 위치 사용
        Vector3 offset = storyMode ? storyModeOffset : followOffset;
        Vector3 tgt = currentTarget.position + offset;

        tgt.z = transform.position.z;

        if (!storyMode && immediateFollowInGame)
            transform.position = tgt;
        else
            transform.position = Vector3.Lerp(transform.position, tgt, Time.deltaTime * followSmooth);
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
        SetCameraSize(defaultSize);
    }
    //게임오브젝트 버전
   public void SetTarget(GameObject go, float? newSize = null, bool forceDefaultSize = false)
    {
        if (go == null) return;
        SetTarget(go.transform, newSize, forceDefaultSize);
    }
    //트랜스폼 버전
    public void SetTarget(Transform target, float? newSize = null, bool forceDefaultSize = false)
    {
        if (target == null) return;
        currentTarget = target;

        if (forceDefaultSize || !newSize.HasValue)
            SetCameraSize(defaultSize);
        else
            cam.orthographicSize = newSize.Value;
    }


    /// <summary>
    /// StoryMode on/off 설정
    /// </summary>
public void EnableStoryMode(bool enable, bool keepSize = false)
{
    storyMode = enable;
    if (enable && defaultTarget != null)
        currentTarget = defaultTarget.transform;
    else if (!enable)
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        currentTarget = p ? p.transform : null;
        if (!keepSize)                       
            SetCameraSize(defaultSize);
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
