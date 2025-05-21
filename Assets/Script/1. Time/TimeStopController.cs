using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class TimeStopController : MonoBehaviour
{
    public static TimeStopController Instance;

    [Header("Gauge Settings")]
    public float maxTimeGauge = 1f;
    public float passiveChargeRate = 0.001f;
    public float timeStopDrainRate = 0.01f;
    public float enemyKillGain = 0.3f;

    [Header("UI")]
    public Slider timeGaugeSlider;
    public Image fillImage;

    /*──────────────────────────────*/
    public float CurrentGauge { get; private set; }   // 읽기 전용
    public bool IsTimeStopped => _isTimeStopped;

    bool _isTimeStopped = false;
    bool _inputBlocked = true;               // 스토리 씬 기본 봉인
    readonly List<ITimeAffectable> _objs = new();


    public float MaxGauge => maxTimeGauge;        // 읽기 전용

    /*──────────────────────────────*/
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            CurrentGauge = maxTimeGauge;
            UpdateGaugeUI();
        }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        RefreshList();
    }

    /*──────────────────────────────*/
    void OnSceneLoaded(Scene sc, LoadSceneMode _)
    {
        RefreshList();
        _inputBlocked = sc.name.Contains("Story");
    }

    public void SetInputBlocked(bool v) => _inputBlocked = v;

    /*──────────────────────────────*/
    void Update()
    {
        bool inDialogue = StorySceneManager.Instance != null &&
                          StorySceneManager.Instance.IsDialogueActive;

        bool isRewinding = RewindManager.Instance != null &&
                           RewindManager.Instance.IsRewinding;

        bool isGameOver = RewindManager.Instance != null &&
                           RewindManager.Instance.IsGameOver();

        if (!_inputBlocked && !inDialogue && !isRewinding && !isGameOver &&
            Input.GetKeyDown(KeyCode.Space))
        {
            ToggleTimeStop();
        }

        HandleGauge();
        UpdateGaugeUI();
    }

    /*──────────────────────────────*/
    void HandleGauge()
    {
        if (_isTimeStopped)
        {
            float drain = (Player.Instance != null && Player.Instance.holdingPrincess)
                          ? timeStopDrainRate * 3f
                          : timeStopDrainRate;

            CurrentGauge -= drain * Time.deltaTime;
            if (CurrentGauge <= 0f)
            {
                CurrentGauge = 0f;
                ResumeTime();
            }
        }
        else
        {
            CurrentGauge += passiveChargeRate * Time.deltaTime;
            CurrentGauge = Mathf.Clamp(CurrentGauge, 0f, maxTimeGauge);
        }
    }

    /*──────────────────────────────*/
    void ToggleTimeStop()
    {
        if (_isTimeStopped) ResumeTime();
        else if (CurrentGauge > 0f) StopTime();
    }

    public void StopTime()
    {
        SoundManager.Instance?.PlaySFX("TimeStopSound");
        _isTimeStopped = true;
        foreach (var o in _objs) o.StopTime();
    }

    public void ResumeTime()
    {
        SoundManager.Instance?.PlaySFX("TimeStopRelease");
        _isTimeStopped = false;
        foreach (var o in _objs) if (o != null) o.ResumeTime();

        //손잡기 상태 자동 해제
        if (Player.Instance?.holdingPrincess == true)
        {
            Player.Instance.StopHoldingPrincess();      //① 플레이어 플래그 다운
            Princess.Instance?.StopBeingHeld();         //② 공주 플래그 다운
        }
    }

    /*──────────────────────────────*/
    void RefreshList()
    {
        _objs.Clear();
        _objs.AddRange(FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                       .OfType<ITimeAffectable>());
    }

    public void RegisterTimeAffectedObject(ITimeAffectable obj)
    {
        if (!_objs.Contains(obj)) _objs.Add(obj);
    }

    public void RemoveTimeAffectedObject(ITimeAffectable obj)
    {
        _objs.Remove(obj);
    }

    /*──────────────────────────────*/
    void UpdateGaugeUI()
    {
        if (timeGaugeSlider) timeGaugeSlider.value = CurrentGauge;
    }

    public bool TrySpendGauge(float amount)
    {
        if (CurrentGauge < amount) return false;
        CurrentGauge -= amount;
        UpdateGaugeUI();
        return true;
    }

    public void AddTimeGauge(float amount)   // ±값 모두 허용
    {
        CurrentGauge = Mathf.Clamp(CurrentGauge + amount, 0f, maxTimeGauge);
        UpdateGaugeUI();
    }
    
    public void SetGauge(float value)
{
    CurrentGauge = Mathf.Clamp(value, 0f, maxTimeGauge);
    UpdateGaugeUI(); // 게이지 UI 갱신 함수가 있다면 호출
}

}
