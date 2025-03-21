using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("기본 타겟 (예: 플레이어)")]
    public GameObject defaultTarget;

    private GameObject currentTarget;
    private Camera cam;

    [Tooltip("기본 카메라 크기 (Orthographic Size)")]
    public float defaultSize = 5f;

    [Tooltip("카메라 이동 속도")]
    public float cameraMoveSpeed = 2f;

    [Tooltip("스토리 모드 여부 (스토리에서는 부드럽게 이동)")]
    public bool isStoryMode = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (defaultTarget != null)
        {
            currentTarget = defaultTarget;
        }
        if (cam != null)
            cam.orthographicSize = defaultSize;

        // 씬 전환 시 자동으로 새로운 타겟을 찾도록 설정
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void LateUpdate()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {

            // 새로운 씬에서 자동으로 타겟 재설정
            FindNewTarget();
        }

        if (currentTarget != null)
        {
            if (isStoryMode)
            {
                // 스토리 모드에서는 부드럽게 이동
                transform.position = Vector3.Lerp(transform.position, new Vector3(currentTarget.transform.position.x, currentTarget.transform.position.y, transform.position.z), Time.deltaTime * cameraMoveSpeed);
            }
            else
            {
                // 인게임 모드에서는 즉각 이동
                transform.position = new Vector3(currentTarget.transform.position.x, currentTarget.transform.position.y, transform.position.z);
            }
        }
    }

    public void SetTarget(GameObject newTarget)
    {
        if (newTarget != null)
        {
            currentTarget = newTarget;
        }
        else
        {
            FindNewTarget(); // 타겟이 null이면 기본 타겟을 찾도록 변경
        }
    }

    public void SetCameraSize(float newSize)
    {
        if (cam != null)
        {
            cam.orthographicSize = newSize;
        }
    }

    public void EnableStoryMode(bool enable)
    {
        isStoryMode = enable;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindNewTarget();
    }

    private void FindNewTarget()
    {
        // 플레이어나 공주를 자동으로 찾기 (씬 전환 후에도 유지)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject princess = GameObject.FindGameObjectWithTag("Princess");

        if (player != null)
        {
            SetTarget(player);
        }
        else if (princess != null)
        {
            SetTarget(princess);
        }
        else if (defaultTarget != null)
        {
            SetTarget(defaultTarget);
        }
        else
        {
            Debug.LogError("[CameraFollow] No target found in the scene!");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
