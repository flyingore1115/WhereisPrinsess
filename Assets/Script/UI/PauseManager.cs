using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // 일시정지 창 UI (Panel)
    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f; // 씬이 시작될 때 반드시 게임 속도 복구
        isPaused = false;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
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
        Time.timeScale = 1f; // 게임 속도 복구
        pauseMenuUI.SetActive(false); // 설정 창 숨김
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true); // 설정 창 표시
        Time.timeScale = 0f; // 게임 정지
        isPaused = true;
    }

    public void QuitGame() //이거 씬매니저에 이미 있으니까 바꿔야함
    {
        Debug.Log("Quitting game...");
       Application.Quit();
    }
    //설정창 다 만들면 복붙하고 따로 설정창 여는것도 추가
}
