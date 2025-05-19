using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("스토리 컷신용 기본 타깃 (예: NPC)")]
    public GameObject defaultTarget;

    private GameObject currentTarget;
    private Camera cam;

    [Tooltip("기본 카메라 크기")]
    public float defaultSize = 5f;

    [Tooltip("스토리 모드 카메라 이동 속도")]
    public float cameraMoveSpeed = 2f;

    public bool isStoryMode = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        currentTarget = defaultTarget;
        if (cam != null) cam.orthographicSize = defaultSize;

        SceneManager.sceneLoaded += OnSceneLoaded;   // 씬 전환 후에도 컷신용으로만 사용
    }

    void LateUpdate()
    {
        //*수정: 스토리 모드일 때만 움직임 적용
        if (!isStoryMode || currentTarget == null) return;

        // 부드럽게 타깃으로 이동 (컷신 카메라)
        transform.position = Vector3.Lerp(
            transform.position,
            new Vector3(currentTarget.transform.position.x,
                        currentTarget.transform.position.y,
                        transform.position.z),
            Time.deltaTime * cameraMoveSpeed);
    }

    public void SetTarget(GameObject newTarget)
    {
        if (newTarget != null) currentTarget = newTarget;
    }

    public void SetCameraSize(float newSize)
    {
        if (cam != null) cam.orthographicSize = newSize;
    }

    public void EnableStoryMode(bool enable)
    {
        isStoryMode = enable;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //*수정: 자동 타깃 검색 제거 → 외부에서 SetTarget 호출
        currentTarget = defaultTarget;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
