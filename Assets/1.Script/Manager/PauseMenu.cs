using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // 설정 창 UI (Panel)

    private bool isPaused = false;

    void Start()
    {
        // 처음 시작 시 설정 창 비활성화
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        // ESC 키로 게임 정지/재개 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // 설정 창 숨김
        Time.timeScale = 1f; // 게임 속도 복구
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true); // 설정 창 표시
        Time.timeScale = 0f; // 게임 정지
        isPaused = true;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit(); // 에디터에서는 작동하지 않음. 빌드 시 작동.
    }
}
