using UnityEngine;

public class CameraFollow : MonoBehaviour 
{
    [Tooltip("기본 타겟 (예: 플레이어)")]
    public GameObject defaultTarget;
    
    private GameObject currentTarget;

    void Start ()
    {
        if (defaultTarget == null)
        {
            Debug.LogError("Default target is not assigned in CameraFollow.");
            return;
        }
        currentTarget = defaultTarget;
        Debug.Log("[CameraFollow] Start: currentTarget set to " + defaultTarget.name);
    }

    void LateUpdate () 
    {
        if (currentTarget == null)
        {
            currentTarget = defaultTarget;
            Debug.LogWarning("[CameraFollow] currentTarget was null, resetting to defaultTarget: " + defaultTarget.name);
        }
        // 로그로 현재 타겟 확인
        Debug.Log("[CameraFollow] Following: " + currentTarget.name);
        transform.position = new Vector3(currentTarget.transform.position.x, currentTarget.transform.position.y, transform.position.z);
    }

    public void SetTarget(GameObject newTarget)
    {
        if (newTarget != null)
        {
            currentTarget = newTarget;
            Debug.Log("[CameraFollow] SetTarget: currentTarget set to " + newTarget.name);
        }
        else
        {
            currentTarget = defaultTarget;
            Debug.Log("[CameraFollow] SetTarget: newTarget is null, currentTarget set to " + defaultTarget.name);
        }
    }
}
