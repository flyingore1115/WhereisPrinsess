using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요

public class RestartGame : MonoBehaviour
{

    public void LoadNextScene()
    {
        // 현재 씬의 인덱스를 가져와 +1 씬을 로드
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
    public void RestartScene()
    {
        // 현재 활성화된 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f; // 게임 속도를 정상적으로 복구
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
