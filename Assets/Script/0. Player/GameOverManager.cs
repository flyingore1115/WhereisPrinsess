using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyGame;

public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    public GameObject princess;          // 공주 오브젝트 (카메라 이동 기준)
    public GameObject player;            // 플레이어 오브젝트
    public Camera mainCamera;            // 메인 카메라
    public CameraFollow cameraFollow;    // 카메라 팔로우 스크립트

    private List<ITimeAffectable> timeAffectedObjects;

    public void TriggerGameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        // 게임오버 시 플레이어 입력 무시 및 게임오버 상태 설정
        // 0) 파괴되었거나 할당 안 됐으면 다시 찾기
    if (player == null || !player)
        player = GameObject.FindGameObjectWithTag("Player");

    Player playerScript = player != null ? player.GetComponent<Player>() : null;
        if (playerScript != null)
        {
            playerScript.ignoreInput = true;
            playerScript.isGameOver = true;               // 애니메이션 정지를 위한 플래그
            playerScript.applyRewindGrayscale = true;       // 강제로 그레이스케일 적용
        }

        if (RewindManager.Instance != null)
        {
            RewindManager.Instance.SetGameOver(true);
        }

        //UI 숨김
        CanvasManager.Instance?.SetGameUIActive(false);


        // 모든 ITimeAffectable 오브젝트에 흑백 효과 적용 (게임오버 모드에서는 플레이어 애니메이션도 멈춤)
        FindTimeAffectedObjects();
        foreach (var obj in timeAffectedObjects)
        {
            obj.StopTime();
        }
        PostProcessingManager.Instance.ApplyGameOver();


        // 카메라를 공주 쪽으로 부드럽게 전환 (줌 인)
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

        Debug.Log("게임오버 상태: 클릭하면 되감기 시작합니다.");
        // 6) 게임오버 상태 안내 메시지 출력
        StatusTextManager.Instance?.ShowMessage("아무 키나 눌러 재시작");

        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        // 카메라를 플레이어 쪽으로 부드럽게 전환 (줌 아웃)
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

        // 되감기 실행
        if (RewindManager.Instance != null)
        {
            RewindManager.Instance.StartRewind();
            while (RewindManager.Instance.IsRewinding)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogError("RewindManager.Instance가 존재하지 않습니다!");
        }

        // 되감기 종료 후 모든 ITimeAffectable 오브젝트 원래 상태 복구
        foreach (var obj in timeAffectedObjects)
        {
            obj.ResumeTime();
        }
        Debug.Log("되감기 종료: 흑백 효과 복구됨.");

        if (RewindManager.Instance != null)
        {
            RewindManager.Instance.SetGameOver(false);
        }
        //캔버스도
        CanvasManager.Instance?.SetGameUIActive(true);

        // 카메라 기본 상태 복귀
        if (cameraFollow != null)
        {
            if (cameraFollow.defaultTarget == null && princess != null)
                cameraFollow.defaultTarget = princess;
            yield return StartCoroutine(SmoothCameraTransition(mainCamera, cameraFollow.defaultTarget.transform.position, cameraFollow.defaultSize, 1f));
            Debug.Log("카메라가 기본 상태로 부드럽게 복귀됨.");
        }

        Debug.Log("클릭하면 게임을 계속 진행합니다.");
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
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
        timeAffectedObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITimeAffectable>().ToList();
    }
}
