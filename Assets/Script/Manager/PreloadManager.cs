using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloadManager : MonoBehaviour
{
    private void Awake()
    {
        InitializeSingletons();
        LoadNextScene();
    }

    private void InitializeSingletons()
    {
        if (FindFirstObjectByType<GameManager>() == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            DontDestroyOnLoad(gm);
        }

        if (FindFirstObjectByType<SoundManager>() == null)
        {
            GameObject am = new GameObject("AudioManager");
            am.AddComponent<SoundManager>();
            DontDestroyOnLoad(am);
        }

        if (FindFirstObjectByType<RewindManager>() == null)
        {
            GameObject am = new GameObject("RewindManager");
            am.AddComponent<RewindManager>();
            DontDestroyOnLoad(am);
        }

        // 필요하면 다른 매니저도 여기에 추가
    }

    private void LoadNextScene()
    {
        StartCoroutine(LoadMainMenu());
    }

    private System.Collections.IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(1.0f); // 로딩 시간 확보
        SceneManager.LoadScene("MainMenu");
    }
}
