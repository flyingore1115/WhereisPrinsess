using UnityEngine;
using UnityEngine.SceneManagement;
using MyGame;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static TimePointData LoadedCheckpoint;

    public int rewindCount = 0;
    public float totalPlayTime = 0f;

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

    public void Update()
    {
         totalPlayTime += Time.unscaledDeltaTime;
    }

    public void NewGame()
    {
        totalPlayTime = 0f;
        rewindCount = 0;

        Debug.Log("[GameManager] 새 게임 시작 → 시간 및 리와인드 수 초기화");
        // 1) 체크포인트 파일 삭제 및 이전 체크포인트 데이터 클리어

        Debug.Log("[GameInitializer] Start, hasCheckpoint=" + TimePointManager.Instance.HasCheckpoint());

        SaveLoadManager.DeleteCheckpoint();
        LoadedCheckpoint = null;
        TimePointManager.Instance.SetCheckpointData(null);
        TimePointManager.Instance.ClearCheckpointFlag();

        // 2) Rewind 스냅샷 전부 삭제
        if (RewindManager.Instance != null)
            RewindManager.Instance.ClearSnapshots();

        // 3) 메인 게임 씬 로드
        MySceneManager.Instance.LoadScene("Boss");//New_Game  Story_1

    }

    public void ContinueGame()
    {
        GameStateData gameStateData;
        if (SaveLoadManager.LoadCheckpoint(out gameStateData))
        {
            Debug.Log("[GameManager] 체크포인트 불러오기 성공");
            LoadedCheckpoint = gameStateData.checkpointData; // 수정된 부분
            TimePointData checkpointData = gameStateData.checkpointData;

            // 메인 게임 씬 로드 (이거 나중에 씬 많아지면 수정해야함)
            MySceneManager.Instance.LoadScene("New_Game");
        }
        else
        {
            Debug.Log("저장된 데이터가 없습니다. 새 게임을 시작합니다.");
            NewGame();
        }
    }

}
