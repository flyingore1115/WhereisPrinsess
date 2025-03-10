using UnityEngine;
using UnityEngine.SceneManagement;
using MyGame;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static TimePointData LoadedCheckpoint;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NewGame()
    {
        SaveLoadManager.DeleteCheckpoint();
        LoadedCheckpoint = null;
        MySceneManager.Instance.LoadScene("GameScene"); // 씬매니저를 통해 씬 이동
    }

    public void ContinueGame()
    {
        TimePointData data;
        if (SaveLoadManager.LoadCheckpoint(out data))
        {
            Debug.Log("[GameManager] 체크포인트 불러오기 성공");
            LoadedCheckpoint = data;
            MySceneManager.Instance.LoadScene("New_Game");  // 이어하기 씬 이동
        }
        else
        {
            Debug.Log("저장된 데이터가 없습니다. 새 게임을 시작합니다.");
            NewGame();
        }
    }
}
