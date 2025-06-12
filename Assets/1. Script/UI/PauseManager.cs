using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("Prefabs")]
    public GameObject pauseMenuPrefab;    // 일시정지창 프리팹 (Canvas 포함 Prefab)
    public GameObject settingMenuPrefab;  // 설정창 프리팹 (Canvas 포함 Prefab)

    private GameObject pauseMenuInstance;
    private GameObject settingMenuInstance;

    private bool isPaused = false;
    private bool isPauseAllowed = false;

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
            return;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 타이틀이나 메인 메뉴 씬에서는 일시정지 비허용
        if (scene.name == "TitleScene" || scene.name == "MainMenu")
        {
            isPauseAllowed = false;
            if (pauseMenuInstance != null)    pauseMenuInstance.SetActive(false);
            if (settingMenuInstance != null)  settingMenuInstance.SetActive(false);
            isPaused = false;
            Time.timeScale = 1f;
        }
        else
        {
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
        if (Input.GetKeyDown(KeyCode.Escape) && isPauseAllowed)
        {
            if (!isPaused) Pause();
            else          Resume();
        }
    }

    public void Pause()
    {
        if (pauseMenuInstance == null && pauseMenuPrefab != null)
        {
            // 최상위에 그대로 인스턴스
            pauseMenuInstance = Instantiate(pauseMenuPrefab);

            // Canvas 설정 보강
            var cv = pauseMenuInstance.GetComponent<Canvas>();
            if (cv == null) cv = pauseMenuInstance.AddComponent<Canvas>();
            cv.renderMode   = RenderMode.ScreenSpaceOverlay;
            cv.sortingOrder = 2000;

            // CanvasScaler
            if (!pauseMenuInstance.TryGetComponent<CanvasScaler>(out var scaler))
            {
                scaler = pauseMenuInstance.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            // GraphicRaycaster
            if (!pauseMenuInstance.TryGetComponent<GraphicRaycaster>(out _))
            {
                pauseMenuInstance.AddComponent<GraphicRaycaster>();
            }

            // EventSystem 보장
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }
        }

        if (pauseMenuInstance != null)
            pauseMenuInstance.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuInstance != null)
            pauseMenuInstance.SetActive(false);

        if (settingMenuInstance != null)
            settingMenuInstance.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingMenuInstance == null && settingMenuPrefab != null)
        {
            settingMenuInstance = Instantiate(settingMenuPrefab);

            var cv = settingMenuInstance.GetComponent<Canvas>();
            if (cv == null) cv = settingMenuInstance.AddComponent<Canvas>();
            cv.renderMode   = RenderMode.ScreenSpaceOverlay;
            cv.sortingOrder = 2100; // Pause 메뉴 위

            if (!settingMenuInstance.TryGetComponent<CanvasScaler>(out var scaler))
            {
                scaler = settingMenuInstance.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
            if (!settingMenuInstance.TryGetComponent<GraphicRaycaster>(out _))
                settingMenuInstance.AddComponent<GraphicRaycaster>();
        }

        if (settingMenuInstance != null)
            settingMenuInstance.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingMenuInstance != null)
            settingMenuInstance.SetActive(false);
    }

    public bool IsPaused => isPaused;
}
