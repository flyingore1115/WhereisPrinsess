using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;  // Cinemachine 3.x namespace

/// <summary>
/// 씬 로드·Awake 시 자동으로 "Player" 태그 객체를 Follow / LookAt 으로 지정합니다.
/// Cinemachine 3.x API에 맞춰 <see cref="CinemachineCamera"/> 사용.
/// </summary>
[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineAutoTarget : MonoBehaviour
{
    private CinemachineCamera vcam;

    void Awake()
    {
        vcam = GetComponent<CinemachineCamera>();
        AssignPlayerTarget(); // 초기 할당
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignPlayerTarget();
    }

    /// <summary>
    /// Player 태그를 가진 오브젝트를 찾아 Follow/LookAt 으로 설정합니다.
    /// </summary>
    private void AssignPlayerTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Transform playerT = playerObj.transform;
            vcam.Follow = playerT;
            vcam.LookAt = playerT;
        }
        else
        {
            Debug.LogWarning("[CinemachineAutoTarget] Player 태그 오브젝트를 찾을 수 없습니다.");
        }
    }

    public static void SetCinemachineTarget(GameObject target)
    {
        CinemachineCamera cam = Object.FindFirstObjectByType<CinemachineCamera>();
        if (cam != null && target != null)
        {
            cam.Follow = target.transform;
            cam.LookAt = target.transform;
            Debug.Log("[CinemachineHelper] 카메라 타겟을 " + target.name + " 으로 변경");
        }
        else
        {
            Debug.LogWarning("[CinemachineHelper] 타겟 지정 실패");
        }
    }
}
