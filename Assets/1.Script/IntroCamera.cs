using UnityEngine;

public class IntroCamera : MonoBehaviour
{
    public Transform princess; // 공주의 Transform
    public Transform player;   // 플레이어의 Transform
    public Camera mainCamera;  // 메인 카메라
    public float followDuration = 3f; // 공주를 따라다니는 시간
    public float cameraMoveSpeed = 5f; // 카메라 이동 속도

    private bool introFinished = false; // 인트로가 끝났는지 여부

    void Start()
    {
        if (princess == null || player == null || mainCamera == null)
        {
            Debug.LogError("Required references (Princess, Player, Camera) are not set!");
            return;
        }

        StartCoroutine(PlayIntro());
    }

    private System.Collections.IEnumerator PlayIntro()
    {
        float elapsedTime = 0f;

        // 공주 따라다니기
        while (elapsedTime < followDuration)
        {
            FollowTarget(princess);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Intro finished, camera is moving back to the Player.");

        // 카메라를 플레이어에게 이동
        while (Vector3.Distance(mainCamera.transform.position, GetTargetPosition(player)) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, GetTargetPosition(player), cameraMoveSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("Camera returned to the Player.");
        introFinished = true;
    }

    void LateUpdate()
    {
        if (introFinished)
        {
            // 이후 다른 카메라 로직 추가 가능
        }
    }

    private void FollowTarget(Transform target)
    {
        Vector3 targetPosition = GetTargetPosition(target);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, cameraMoveSpeed * Time.deltaTime);
    }

    private Vector3 GetTargetPosition(Transform target)
    {
        return target.position + new Vector3(0, 0, -10); // 카메라 Z축 고정
    }
}
