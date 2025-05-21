using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 여러 UI 요소(탄약 텍스트, 체력바, 시간정지 게이지)를 한 곳에서 통합 관리.
/// </summary>
public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;

    [Header("Ammo UI (텍스트)")]
    public TMP_Text bulletText;            // 탄약 수를 표시할 TMP 텍스트

    [Header("플레이어 체력 (슬라이더)")]
    public Slider playerHealthSlider;      // 플레이어 체력을 표시할 슬라이더

    [Header("TimeStop Gauge (슬라이더)")]
    public Slider timeStopSlider;         // 타임스톱 게이지


    public GameObject[] gameOnlyUI;    // ← 인스펙터에서 bulletText, 슬라이더 등 등록
    //public GameObject storyUI;      // ← 스토리 씬에서도 유지할 UI만 별도 지정

    private void Awake()
    {
        // 싱글톤
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
    /// 탄약 UI 업데이트 (사격 스크립트에서 호출)
    /// </summary>
    public void UpdateBulletUI(int currentAmmo, int maxAmmo)
    {
        if (bulletText != null)
        {
            bulletText.text = $"{currentAmmo} / {maxAmmo}";
        }
    }

    /// <summary>
    /// 플레이어 체력 UI 업데이트 (PlayerOver에서 호출)
    /// 슬라이더의 최대값과 현재값을 설정합니다.
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
    /// 시간정지 게이지 UI 업데이트(슬라이더). TimeStopController.Instance를 직접 참조해서 처리.
    /// </summary>
    void UpdateTimeStopUI()
    {
        var tsc = TimeStopController.Instance;
        if (tsc == null || timeStopSlider == null) return;

        timeStopSlider.maxValue = tsc.MaxGauge;
        timeStopSlider.value = tsc.CurrentGauge;
    }
    
    public void SetGameUIActive(bool active)
    {
        foreach (var go in gameOnlyUI)
        {
            if (go != null) go.SetActive(active);
        }

        //if (storyUI != null)
            //storyUI.SetActive(true); // 항상 켜짐
    }

}
