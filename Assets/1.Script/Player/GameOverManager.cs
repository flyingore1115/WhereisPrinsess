using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    public IEnumerator TriggerGameOverAfterAnimation(Animator animator, Princess princessScript)
    {
        if (isGameOver) yield break;

        isGameOver = true;

        if (animator != null)
        {
            animator.SetTrigger("isDie");
        }

        if (animator != null)
        {
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                yield return null;
            }
        }

        // 모든 사운드 정지
        if (SoundManager.Instance != null)
        {
            //SoundManager.Instance.StopAllSounds();
            Debug.Log("소리 중단");
        }

        yield return StartCoroutine(MoveCameraAndZoom());
        
        if (gameOverUI != null)
        {
        gameOverUI.SetActive(true); //게임오버 판넬 뜨게
        }

        Time.timeScale = 0f;
    }

    private IEnumerator MoveCameraAndZoom()
    {
        float initialZoom = mainCamera.orthographicSize;
        Vector3 targetPosition = princess.position + new Vector3(0, 0, -10);

        while (Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.1f ||
               Mathf.Abs(mainCamera.orthographicSize - targetZoom) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, cameraMoveSpeed * Time.unscaledDeltaTime);
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, zoomSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
    }

    public void RetryGame()
    {
        // 현재 씬 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f; // 게임 정지 해제
        
    }
}
