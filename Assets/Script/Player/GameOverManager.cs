using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyGame; // ITimeAffectable, TimePointManager 등 사용

public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    public GameObject princess;          // 공주 오브젝트 (카메라 이동 기준)
    public GameObject player;            // 플레이어 오브젝트
    public Camera mainCamera;            // 메인 카메라
    public RewindManager rewindManager;  // 되감기 매니저
    public CameraFollow cameraFollow;    // 카메라 팔로우 스크립트

    // 게임오버 시 흑백 효과를 적용할 대상들
    private List<ITimeAffectable> timeAffectedObjects;

    public void TriggerGameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {

        // 총탄표시 UI 비활성화
        P_Shooting shooting = FindObjectOfType<P_Shooting>();
        if (shooting != null)
        {
            shooting.HideBulletUI();
        }
        // 1. 모든 ITimeAffectable 오브젝트에 대해 흑백 효과 적용
        FindTimeAffectedObjects();
        foreach (var obj in timeAffectedObjects)
        {
            // 만약 obj가 Player라면, 강제로 그레이스케일 적용 플래그를 true로 설정
            Player playerObj = obj as Player;
            if (playerObj != null)
            {
                playerObj.applyRewindGrayscale = true;
            }
            obj.StopTime();
        }
        Debug.Log("게임오버: 흑백 효과 적용됨.");

        // 2. 카메라를 공주 쪽으로 부드럽게 전환 (줌 인: 예, 3f)
        if (cameraFollow != null && princess != null)
        {
            cameraFollow.SetTarget(princess);
            yield return StartCoroutine(SmoothCameraTransition(mainCamera, princess.transform.position, 3f, 1f));
            Debug.Log("카메라가 공주 쪽으로 부드럽게 전환됨.");
        }
        else if (mainCamera != null && princess != null)
        {
            yield return StartCoroutine(SmoothCameraTransition(mainCamera, princess.transform.position, 3f, 1f));
        }

        // 3. 게임오버 상태 유지: 흑백 및 카메라 효과가 적용된 상태로 대기
        Debug.Log("게임오버 상태: 클릭하면 되감기 시작합니다.");
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }

        // 4. 클릭 시, 카메라를 플레이어 쪽으로 부드럽게 전환 (줌 아웃: 예, 6f)
        if (cameraFollow != null && player != null)
        {
            cameraFollow.SetTarget(player);
            yield return StartCoroutine(SmoothCameraTransition(mainCamera, player.transform.position, 6f, 1f));
            Debug.Log("카메라가 플레이어 쪽으로 부드럽게 전환됨.");
        }
        else if (mainCamera != null && player != null)
        {
            yield return StartCoroutine(SmoothCameraTransition(mainCamera, player.transform.position, 6f, 1f));
        }

        // 5. 되감기 시작 (RewindManager가 실행됨)
        if (rewindManager != null)
        {
            rewindManager.StartRewind();
            while (rewindManager.IsRewinding)
            {
                yield return null;
            }
        }

        // 6. 되감기 종료 후 모든 ITimeAffectable 오브젝트의 원래 색상 복구
        foreach (var obj in timeAffectedObjects)
        {
            obj.ResumeTime();
        }
        Debug.Log("되감기 종료: 흑백 효과 복구됨.");

        // 7. 카메라를 기본 타겟 및 기본 줌으로 부드럽게 복구
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(cameraFollow.defaultTarget);
            yield return StartCoroutine(SmoothCameraTransition(mainCamera, cameraFollow.defaultTarget.transform.position, cameraFollow.defaultSize, 1f));
            Debug.Log("카메라가 기본 상태로 부드럽게 복귀됨.");
        }

        // 8. 다시 클릭 입력 대기 → 게임 재시작 (씬 재로드)
        Debug.Log("클릭하면 게임을 재시작합니다.");
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
        SceneManager.LoadScene("GameScene");
    }

    IEnumerator SmoothCameraTransition(Camera cam, Vector3 targetPos, float targetSize, float duration)
    {
        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            cam.transform.position = Vector3.Lerp(startPos, new Vector3(targetPos.x, targetPos.y, startPos.z), t);
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }
        cam.transform.position = new Vector3(targetPos.x, targetPos.y, startPos.z);
        cam.orthographicSize = targetSize;
    }

    void FindTimeAffectedObjects()
    {
        timeAffectedObjects = FindObjectsOfType<MonoBehaviour>().OfType<ITimeAffectable>().ToList();
    }
}
