using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 여러 UI 요소(체력바, 시간정지 게이지)를 한 곳에서 통합 관리.
/// 탄약 UI는 World-Space UI로 분리하여 별도 prefab으로 처리합니다.
/// </summary>
public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;

    [Header("플레이어 체력 (슬라이더)")]
    public Slider playerHealthSlider;

    [Header("TimeStop Gauge (슬라이더)")]
    public Slider timeStopSlider;

    [Header("Game-only UI Elements")]
    public GameObject[] gameOnlyUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Story"))
            SetGameUIActive(true);
    }

    private void Update()
    {
        UpdateTimeStopUI();
    }

    /// <summary>
    /// 플레이어 체력 UI 업데이트 (PlayerOver에서 호출)
    /// </summary>
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (playerHealthSlider != null)
        {
            playerHealthSlider.maxValue = maxHealth;
            playerHealthSlider.value = currentHealth;
        }
    }

    /// <summary>
    /// 시간정지 게이지 UI 업데이트(슬라이더). TimeStopController 참조
    /// </summary>
    private void UpdateTimeStopUI()
    {
        var tsc = TimeStopController.Instance;
        if (tsc == null || timeStopSlider == null) return;

        timeStopSlider.maxValue = tsc.MaxGauge;
        timeStopSlider.value = tsc.CurrentGauge;
    }

    /// <summary>
    /// 게임 전용 UI를 전체 켜거나 끕니다.
    /// </summary>
    public void SetGameUIActive(bool active)
    {
        foreach (var go in gameOnlyUI)
        {
            if (go != null)
                go.SetActive(active);
        }
    }
}
