using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MySceneManager : MonoBehaviour
{
    public static MySceneManager Instance;

    public static bool IsStoryScene => SceneManager.GetActiveScene().name.Contains("Story");
    public static bool IsMainMenu => SceneManager.GetActiveScene().name.Contains("MainMenu");
    


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (FadeManager.Instance != null)
        {
            //페이드 인 실행
            FadeManager.Instance.StartFadeOut(() =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    public void MainMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    public void LoadNextScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        int curScene = scene.buildIndex;
        int nextScene = curScene + 1;
        SceneManager.LoadScene(nextScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[MySceneManager] 씬 로드 완료됨: " + scene.name);

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.StartFadeIn();
        }

        // UI 활성/비활성 분기
        if (CanvasManager.Instance != null)
        {
            if (scene.name.StartsWith("Story") || scene.name.Contains("Story"))
                CanvasManager.Instance.SetGameUIActive(false);  // 스토리 씬이면 UI 숨김
            else
                CanvasManager.Instance.SetGameUIActive(true);   // 인게임 씬이면 표시
        }
        
        // 0) 이전 체크포인트 무효화
    if (TimePointManager.Instance != null)
        TimePointManager.Instance.ClearCheckpointFlag();

    // 1) 페이드/UI 분기 (기존)

    // 2) 플레이어 위치 리셋
    if (Player.Instance != null)
    {
        Player.Instance.transform.position = Vector3.zero;
        var rb = Player.Instance.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

        // 3) 공주 위치 리셋 (추가)
        if (Princess.Instance != null)
        {
            Princess.Instance.ResetToDefaultPosition();
            // 그리고 모드·애니 초기화
            Princess.Instance.RefreshSceneBehavior();
        }

    }


}
