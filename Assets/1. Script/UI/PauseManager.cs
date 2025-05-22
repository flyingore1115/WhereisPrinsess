using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("Prefabs")]
    public GameObject pauseMenuPrefab;   // 일시정지창 프리팹
    public GameObject settingMenuPrefab; // 설정창 프리팹

    private GameObject pauseMenuInstance;   // 생성된 일시정지창 인스턴스
    private GameObject settingMenuInstance; // 생성된 설정창 인스턴스

    private bool isPaused = false;

    // ★ 추가: 메인메뉴에서는 일시정지 불가
    private bool isPauseAllowed = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 시 마다 콜백
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 메인메뉴 씬 이름이 "TitleScene"이라고 가정
        if (scene.name == "TitleScene" || scene.name == "MainMenu")
        {
            isPauseAllowed = false;
            // 혹시 열려있는 pauseMenu가 있다면 닫기
            if (pauseMenuInstance != null)
                pauseMenuInstance.SetActive(false);
            if (settingMenuInstance != null)
                settingMenuInstance.SetActive(false);
            isPaused = false;
        }
        else
        {
            // 인게임 씬이라면 일시정지 허용
            isPauseAllowed = true;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        // ESC 키 입력
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 메인메뉴(또는 허용되지 않는 씬)에서는 무시
            if (!isPauseAllowed) 
                return;

            if (!isPaused)
                Pause();
            else
                Resume();
        }
    }

    // 게임 일시정지(메뉴 열기)
    public void Pause()
    {
        // 이미 다른 pauseMenuInstance가 없으면 새로 생성
        if (pauseMenuInstance == null && pauseMenuPrefab != null)
        {
            GameObject canvasRoot = GameObject.FindGameObjectWithTag("MainCanvas");
            if (canvasRoot == null)
            {
                Debug.LogWarning("[PauseManager] MainCanvas 태그의 오브젝트가 없음");
                return;
            }
            pauseMenuInstance = Instantiate(pauseMenuPrefab, canvasRoot.transform);
        }

        if (pauseMenuInstance != null)
            pauseMenuInstance.SetActive(true);

        Time.timeScale = 0f; 
        isPaused = true;
    }

    // 게임 재개(메뉴 닫기)
    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuInstance != null)
            pauseMenuInstance.SetActive(false);

        // 설정창도 함께 닫는다
        if (settingMenuInstance != null)
            settingMenuInstance.SetActive(false);
    }

    // 설정창 열기
    public void OpenSettings()
    {
        // 이미 생성된 SettingMenu가 없으면
        if (settingMenuInstance == null && settingMenuPrefab != null)
        {
            GameObject canvasRoot = GameObject.FindGameObjectWithTag("MainCanvas");
            if (canvasRoot == null)
            {
                Debug.LogWarning("[PauseManager] MainCanvas 태그의 오브젝트가 없음");
                return;
            }
            settingMenuInstance = Instantiate(settingMenuPrefab, canvasRoot.transform);
        }

        if (settingMenuInstance != null)
            settingMenuInstance.SetActive(true);
    }

    // 설정창 닫기
    public void CloseSettings()
    {
        if (settingMenuInstance != null)
            settingMenuInstance.SetActive(false);
    }
    
    public bool IsPaused => isPaused;
}
