using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelClear : MonoBehaviour
{
    public string nextSceneName; // 전환할 다음 씬 이름

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // 플레이어와 충돌 확인
        {
            Debug.Log("Level Cleared!");
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName); // 다음 씬 로드
        }
        else
        {
            Debug.LogError("Next scene name is not set!");
        }
    }
}
