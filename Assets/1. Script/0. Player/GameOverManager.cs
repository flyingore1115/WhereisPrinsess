using UnityEngine;
using System.Collections;
using System.Linq;
using MyGame;
using System.Collections.Generic;

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

    private IEnumerator GameOverRoutine()
    {
        // (0) 플레이어 상태 설정
        if (player == null || !player)
            player = GameObject.FindGameObjectWithTag("Player");
        var playerScript = player?.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.ignoreInput = true;
            playerScript.isGameOver = true;
            playerScript.applyRewindGrayscale = true;
        }
        var pAttack = Player.Instance?.GetComponent<P_Attack>();
        pAttack?.CancelAttack();   // 진행 중이던 돌진/공격 즉시 중단

        RewindManager.Instance?.SetGameOver(true);
        CanvasManager.Instance?.SetGameUIActive(false);

        // (1) 모든 ITimeAffectable 오브젝트에 시간을 멈추도록 지시 (플레이어·공주 애니메이션 포함)
        FindTimeAffectedObjects();
        foreach (var obj in timeAffectedObjects)
        {
            // 보스전 중에는 공주도 얼려두지 않도록 예외처리할 수 있지만,
            // 여기는 “게임오버 시에는 공주도 멈추지 않는다”라고 원한다면 아래 continue를 넣고
            // 보통은 공주도 같이 멈추는 게 좋으므로 그대로 StopTime 호출
            if (obj is Princess) continue;
            obj.StopTime();
        }
        PostProcessingManager.Instance.ApplyGameOver();

        // (2) 카메라를 공주 쪽으로 이동(내부에서 스무스하게 줌 인)
        if (cameraFollow != null && princess != null)
        {
            cameraFollow.SetTarget(princess);
            yield return StartCoroutine(
                SmoothCameraTransition(mainCamera, princess.transform.position, 3f, 1f)
            );
        }
        else if (mainCamera != null && princess != null)
        {
            yield return StartCoroutine(
                SmoothCameraTransition(mainCamera, princess.transform.position, 3f, 1f)
            );
        }

        StatusTextManager.Instance?.ShowMessage("아무 키나 눌러 재시작");

        // (3) 사용자의 입력을 기다린다
        while (!Input.anyKeyDown)
            yield return null;

        // ───────────────────────────────────────────────────
        // 여기부터 “보스전일 때” VS “일반 스테이지일 때” 분기
        // ───────────────────────────────────────────────────

        bool isBossScene = false;
        // 방법 A: 씬 이름에 “Boss”가 포함된 경우
        isBossScene = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().name.Contains("Boss");

        // 방법 B: 씬에 살아 있는 Teddy(보스) 오브젝트가 있는지 검사하는 경우
        if (!isBossScene)
        {
            var teddy = FindFirstObjectByType<Teddy>();
            if (teddy != null && !teddy.isDead)
            {
                isBossScene = true;
            }
        }

        if (isBossScene)
        {
            var currentPlayer = player != null ? player.GetComponent<Player>() : null;
        if (currentPlayer != null)
        {
            currentPlayer.ignoreInput = false;
            currentPlayer.isGameOver = false;
            currentPlayer.applyRewindGrayscale = false;
        }
        foreach (var t in timeAffectedObjects)
        {
            t.ResumeTime();
        }

        // (b) RewindManager 상태 해제
            if (RewindManager.Instance != null)
            {
                RewindManager.Instance.SetGameOver(false);
            }

        // (c) 포스트 프로세싱 원상 복구 (게임오버 모드 해제)
        // PostProcessingManager.Instance?.SetDefaultEffects();

        // (d) UI 원상 복구
        CanvasManager.Instance?.SetGameUIActive(true);

            var sceneName = UnityEngine.SceneManagement.SceneManager
                .GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            yield break;
        }

        // ── 일반 스테이지인 경우: 되감기 연출을 수행 ──

        // (4) 카메라를 플레이어 쪽으로 스무스하게 되돌린다
        if (cameraFollow != null && player != null)
        {
            cameraFollow.SetTarget(player);
            yield return StartCoroutine(
                SmoothCameraTransition(mainCamera, player.transform.position, 6f, 1f)
            );
        }
        else if (mainCamera != null && player != null)
        {
            yield return StartCoroutine(
                SmoothCameraTransition(mainCamera, player.transform.position, 6f, 1f)
            );
        }

        // (5) 실제로 되감기 시작
        if (RewindManager.Instance != null)
        {
            RewindManager.Instance.StartRewind();
            while (RewindManager.Instance.IsRewinding)
                yield return null;
        }

        // (6) 되감기 끝난 뒤: 시간을 다시 재개(흑백 해제)
        foreach (var obj in timeAffectedObjects)
            obj.ResumeTime();
        RewindManager.Instance?.SetGameOver(false);
        CanvasManager.Instance?.SetGameUIActive(true);

        // (7) 카메라 기본 상태로 복귀
        if (cameraFollow != null)
        {
            if (cameraFollow.defaultTarget == null && princess != null)
                cameraFollow.defaultTarget = princess;
            yield return StartCoroutine(
                SmoothCameraTransition(
                    mainCamera,
                    cameraFollow.defaultTarget.transform.position,
                    cameraFollow.defaultSize,
                    1f
                )
            );
        }

        // (8) “클릭하면 게임 계속” 메시지 대기
        Debug.Log("클릭하면 계속합니다.");
        while (!Input.GetMouseButtonDown(0))
            yield return null;

        // (9) 최종 복귀: 플레이어 제어, 애니메이터 속도 등 복원
        playerScript.ignoreInput = false;
        playerScript.applyRewindGrayscale = false;
        playerScript.isGameOver = false;

        if (player != null && mainCamera != null)
        {
            if (cameraFollow != null)
                cameraFollow.SetTarget(player);
            // (추가 리셋 로직이 필요하다면 여기에)
        }
    }

    private IEnumerator SmoothCameraTransition(
        Camera cam,
        Vector3 targetPos,
        float targetSize,
        float duration)
    {
        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            CameraFollow.Instance.transform.position =
                Vector3.Lerp(startPos,
                             new Vector3(targetPos.x, targetPos.y, startPos.z),
                             t);
            CameraFollow.Instance.SetCameraSize(
                Mathf.Lerp(startSize, targetSize, t)
            );
            yield return null;
        }
        CameraFollow.Instance.transform.position =
            new Vector3(targetPos.x, targetPos.y, startPos.z);
        CameraFollow.Instance.SetCameraSize(targetSize);
    }

    private void FindTimeAffectedObjects()
    {
        timeAffectedObjects = FindObjectsByType<MonoBehaviour>(
            FindObjectsSortMode.None
        )
        .OfType<ITimeAffectable>()
        .ToList();
    }
}
