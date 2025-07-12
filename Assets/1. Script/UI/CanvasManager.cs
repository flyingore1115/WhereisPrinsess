using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/// <summary>
/// 여러 UI 요소(체력바, 시간정지 게이지)를 한 곳에서 통합 관리.
/// 탄약 UI는 World-Space UI로 분리하여 별도 prefab으로 처리합니다.
/// </summary>
public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;

    [Header("플레이어 체력")]
    public Image[] heartIcons;           // ★추가: 하트 3개
    public Sprite  heartFull;            // ★추가: 꽉 찬 하트
    public Sprite  heartEmpty;           // ★추가: 빈   하트

    private const string HEART_FULL_PATH  = "IMG/Charctor/May/H1";
    private const string HEART_EMPTY_PATH = "IMG/Charctor/May/H2";

    [Header("TimeStop Gauge (슬라이더)")]
    public Slider timeStopSlider;
    public Image   timeGaugeFillImage;   // ★추가: Radial 360 Image
    public TMP_Text timeGaugeText;       // ★추가: 숫자(TMP)

    [Header("Game-only UI Elements")]
    public GameObject[] gameOnlyUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            bool isStory = SceneManager.GetActiveScene().name.Contains("Story");
        SetGameUIActive(!isStory);
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

        if (heartFull == null)
        {
            heartFull = Resources.Load<Sprite>(HEART_FULL_PATH);
            Debug.LogWarning("이미지1없음!!!");
        }
        if (heartEmpty == null)
        {
            heartEmpty = Resources.Load<Sprite>(HEART_EMPTY_PATH);
            Debug.LogWarning("이미지2없음!!!");
        }
        if (heartIcons == null)
        {
            Debug.LogWarning("아이콘없음!!!");
            return;
        }

        for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] == null) continue;   // 씬 전환으로 파괴됐을 때 무시
                heartIcons[i].sprite = (i < currentHealth) ? heartFull : heartEmpty;
            }
    }


    /// <summary>
    /// 시간정지 게이지 UI 업데이트(슬라이더). TimeStopController 참조
    /// </summary>
    public void UpdateTimeStopUI()
    {
        var tsc = TimeStopController.Instance;
        if (tsc == null || timeGaugeFillImage == null || timeGaugeText == null) return;

        float ratio = tsc.CurrentGauge / tsc.MaxGauge;
        timeGaugeFillImage.fillAmount = ratio;                 // 원형 게이지

        // 스택 수 표기 (0이면 “X”)
        if (tsc.RemainingStacks <= 0)
            timeGaugeText.text = "X";
        else if (tsc.RemainingStacks >= 100)
            timeGaugeText.text = "∞";
        else
            timeGaugeText.text = tsc.RemainingStacks.ToString();
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
