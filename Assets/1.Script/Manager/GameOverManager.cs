using UnityEngine;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public Transform princess; // 공주의 Transform
    public Camera mainCamera; // 메인 카메라
    public GameObject gameOverUI; // 게임 오버 UI
    public float cameraMoveSpeed = 5f; // 카메라 이동 속도
    public float zoomSpeed = 2f; // 카메라 줌 속도
    public float targetZoom = 3f; // 게임 오버 시 최종 카메라 줌 크기

    private bool isGameOver = false;

    void Start()
    {
        // 게임 오버 UI 비활성화
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    public IEnumerator TriggerGameOverAfterAnimation(Animator animator, Princess princessScript)
    {
        if (isGameOver) yield break;

        isGameOver = true;

        // 애니메이션 트리거 설정
        if (animator != null)
        {
            animator.SetTrigger("isDie");
        }

        // 애니메이션이 끝날 때까지 대기
        if (animator != null)
        {
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                yield return null;
            }
        }

        Debug.Log("Game Over Animation Finished!");

        // 카메라 이동 및 줌 조정
        yield return StartCoroutine(MoveCameraAndZoom());

        // 게임 오버 UI 활성화
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // 게임 정지
        Time.timeScale = 0f;
    }

    private IEnumerator MoveCameraAndZoom()
    {
        float initialZoom = mainCamera.orthographicSize;
        Vector3 targetPosition = princess.position + new Vector3(0, 0, -10); // 공주 위치로 이동

        while (Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.1f ||
               Mathf.Abs(mainCamera.orthographicSize - targetZoom) > 0.1f)
        {
            // 카메라 위치 이동
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, cameraMoveSpeed * Time.unscaledDeltaTime);

            // 카메라 줌 조정
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, zoomSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        Debug.Log("Camera Moved and Zoom Adjusted!");
    }
}
