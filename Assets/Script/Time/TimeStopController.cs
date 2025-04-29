using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class TimeStopController : MonoBehaviour
{
    public static TimeStopController Instance;

    private bool isTimeStopped = false;
    private List<ITimeAffectable> timeAffectedObjects = new List<ITimeAffectable>();

    public float maxTimeGauge = 100f;
    public float currentTimeGauge;
    public float passiveChargeRate = 0.5f;
    public float timeStopDrainRate = 1f;
    public float enemyKillGain = 5f;

    // ★추가: 외부(RewindManager)에서 접근 가능하도록 프로퍼티
    public bool IsTimeStopped => isTimeStopped;

    //================================================================슬라이더
    public Slider timeGaugeSlider;
    public Image fillImage;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[RewindManager] Awake -> Instance 할당");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        FindTimeAffectedObjects();
        currentTimeGauge = maxTimeGauge;
        UpdateTimeGaugeUI();
    }

    void Update()
    {
        // (1) "게임오버 중" 또는 "되감기 중"이면 플레이어가 시간정지 입력 못하게
        //     RewindManager.Instance가 null일 수도 있으니 체크
        bool isRewinding = (RewindManager.Instance != null && RewindManager.Instance.IsRewinding);
        bool isGameOver = (RewindManager.Instance != null && RewindManager.Instance.IsGameOver());
        // ↑ isGameOver 메서드를 새로 만든다고 가정 (아래 RewindManager 수정 예시 참고)

        // => 둘 중 하나라도 true면 시간정지 입력 무시
        if (!isGameOver && !isRewinding)
        {
            // 기존 시간정지 토글 로직
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleTimeStop();
            }
        }

        // 시간정지 상태일 때 게이지 소모 / 시간정지 아닐 때 패시브 충전
        if (isTimeStopped)
        {
            currentTimeGauge -= timeStopDrainRate * Time.deltaTime;
            if (currentTimeGauge <= 0)
            {
                currentTimeGauge = 0;
                ResumeTime();
            }
        }
        else
        {
            //패시브 자동 충전
            currentTimeGauge += passiveChargeRate * Time.deltaTime;
            currentTimeGauge = Mathf.Clamp(currentTimeGauge, 0, maxTimeGauge);
        }
        // 추가: 손잡기 처리 (시간정지 상태에서 공주 클릭)
        if (isTimeStopped)
        {
            // 손잡기 시작: 마우스 왼쪽 클릭 시 레이캐스트로 공주 확인
            if (!Player.Instance.holdingPrincess && Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.CompareTag("Princess"))
                {
                    Player.Instance.StartHoldingPrincess();
                    Princess.Instance.StartBeingHeld();
                    Debug.Log("공주 손잡기 시작");
                }
            }
        }
        // 시간이 풀리면 손잡기 상태 해제
        else
        {
            if (Player.Instance.holdingPrincess)
            {
                Player.Instance.StopHoldingPrincess();
                Princess.Instance.StopBeingHeld();
                Debug.Log("시간정지 해제 → 공주 손잡기 해제");
            }
        }

        // 시간정지 상태일 때 게이지 소모 (손잡기 상태면 3배 소모)
        if (isTimeStopped)
        {
            float drainRate = Player.Instance.holdingPrincess ? timeStopDrainRate * 3f : timeStopDrainRate;
            currentTimeGauge -= drainRate * Time.deltaTime;
            if (currentTimeGauge <= 0)
            {
                currentTimeGauge = 0;
                ResumeTime();
            }
        }
        else
        {
            currentTimeGauge += passiveChargeRate * Time.deltaTime;
            currentTimeGauge = Mathf.Clamp(currentTimeGauge, 0, maxTimeGauge);
        }
        UpdateTimeGaugeUI();

    }

    public void RemoveTimeAffectedObject(ITimeAffectable obj)
    {
        timeAffectedObjects.Remove(obj);
    }

    void ToggleTimeStop()
    {
        if (isTimeStopped)
        {
            ResumeTime();
        }
        else
        {
            if (currentTimeGauge > 0)
            {
                StopTime();
            }
        }
    }

    public void StopTime()
    {
        // 사운드
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("TimeStopSound");

        isTimeStopped = true;
        foreach (var obj in timeAffectedObjects)
        {
            obj.StopTime();
        }
    }

    public void ResumeTime()
    {
        // 사운드
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("TimeStopRelease");

        isTimeStopped = false;
        foreach (var obj in timeAffectedObjects)
        {
            if (obj != null)
            {
                obj.ResumeTime();
            }
        }
    }

    void FindTimeAffectedObjects()
    {
        var affectables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITimeAffectable>();
        timeAffectedObjects.AddRange(affectables);
    }

    public void AddTimeGauge(float amount)
    {
        currentTimeGauge += amount;
        currentTimeGauge = Mathf.Clamp(currentTimeGauge, 0, maxTimeGauge);
        UpdateTimeGaugeUI();
    }

    void UpdateTimeGaugeUI()
    {
        if (timeGaugeSlider != null)
        {
            timeGaugeSlider.value = currentTimeGauge;
        }
    }

    public void RegisterTimeAffectedObject(ITimeAffectable obj)
    {
        timeAffectedObjects.Add(obj);
    }

    void ReverseTime(float duration)
    {
        // 예제만
        Debug.Log("[Skill] Time Reversal Activated!");
        Vector3 previousPosition = GetPlayerPreviousPosition(duration);
        transform.position = previousPosition;
    }

    Vector3 GetPlayerPreviousPosition(float duration)
    {
        // 임의 예시
        return transform.position - (Vector3.right * 5f);
    }
}
