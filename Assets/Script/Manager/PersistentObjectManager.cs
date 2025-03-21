using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentObjectManager : MonoBehaviour
{
    public static PersistentObjectManager Instance;

    private void Awake()
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            // 플레이어와 공주 싱글톤 삭제
            if (Player.Instance != null)
            {
                Destroy(Player.Instance.gameObject);
                Debug.Log("Persistent Player destroyed on TitleScene load.");
            }
            if (Princess.Instance != null)
            {
                Destroy(Princess.Instance.gameObject);
                Debug.Log("Persistent Princess destroyed on TitleScene load.");
            }
            
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
