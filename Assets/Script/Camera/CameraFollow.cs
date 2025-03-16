using UnityEngine;
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
    }

    void LateUpdate()
    {
        if (currentTarget == null)
        {
            currentTarget = defaultTarget;
            Debug.LogWarning("[CameraFollow] currentTarget was null, resetting to defaultTarget: " + defaultTarget.name);
        }

        if (isStoryMode)
        {
            // 스토리 모드에서는 부드럽게 이동
            transform.position = Vector3.Lerp(transform.position, new Vector3(currentTarget.transform.position.x, currentTarget.transform.position.y, transform.position.z), Time.deltaTime * cameraMoveSpeed);
        }
        else
        {
            // 인게임 모드에서는 즉각 이동 (이전 방식 유지)
            transform.position = new Vector3(currentTarget.transform.position.x, currentTarget.transform.position.y, transform.position.z);
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
            currentTarget = defaultTarget;
            Debug.Log("[CameraFollow] SetTarget: newTarget is null, currentTarget set to " + defaultTarget.name);
        }
    }

    public void SetCameraSize(float newSize)
    {
        if (cam != null)
        {
            cam.orthographicSize = newSize;
        }
    }

    // **스토리 모드 활성화/비활성화 함수 추가**
    public void EnableStoryMode(bool enable)
    {
        isStoryMode = enable;
    }
}
