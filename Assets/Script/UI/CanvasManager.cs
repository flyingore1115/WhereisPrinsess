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
    public Image timeStopFillImage;       // 슬라이더의 fill 영역(색상 조절용)

    [Header("Warning Colors")]
    public Color normalColor = Color.green;
    public Color warningColor = Color.red;
    public float warningThreshold = 20f;   // 게이지가 20 이하일 때 경고 깜빡임
    private bool isBlinking = false;
    private float blinkTimer = 0f;
    public float blinkInterval = 0.5f;

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
    private void UpdateTimeStopUI()
    {
        TimeStopController tsc = TimeStopController.Instance;
        if (tsc == null) return;

        if (timeStopSlider != null)
        {
            timeStopSlider.maxValue = tsc.maxTimeGauge;
            timeStopSlider.value    = tsc.currentTimeGauge;

            // 경고 깜빡임 로직
            if (tsc.currentTimeGauge <= warningThreshold)
            {
                if (!isBlinking)
                {
                    isBlinking = true;
                    blinkTimer = 0f;
                }
            }
            else
            {
                if (isBlinking)
                {
                    isBlinking = false;
                    if (timeStopFillImage != null)
                        timeStopFillImage.color = normalColor;
                }
            }

            if (isBlinking && timeStopFillImage != null)
            {
                blinkTimer += Time.unscaledDeltaTime; // 게임 일시정지 무시하는 UI라면 unscaledDeltaTime 사용
                if (blinkTimer >= blinkInterval)
                {
                    blinkTimer = 0f;
                    timeStopFillImage.color = (timeStopFillImage.color == normalColor)
                        ? warningColor
                        : normalColor;
                }
            }
        }
    }
}
