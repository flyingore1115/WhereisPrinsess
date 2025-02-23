using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class TimeStopController : MonoBehaviour
{
    private bool isTimeStopped = false;

    // 시간 정지 영향을 받는 오브젝트들을 담는 리스트
    private List<ITimeAffectable> timeAffectedObjects = new List<ITimeAffectable>();

    // 시간 게이지 관련 변수들
    public float maxTimeGauge = 100f;         // 최대 시간 게이지
    public float currentTimeGauge;            // 현재 시간 게이지
    public float passiveChargeRate = 0.5f;    // 자연 충전 속도
    public float timeStopDrainRate = 1f;      // 시간 정지 중 소모 속도
    public float enemyKillGain = 5f;          // 적 처치 시 회복량

    // 외부에서 읽기만 가능하도록 프로퍼티로 제공
    public bool IsTimeStopped => isTimeStopped;

    //================================================================ 슬라이더 UI 관련 변수
    public Slider timeGaugeSlider;   // 시간 게이지 슬라이더
    public Image fillImage;          // 슬라이더 색상 변경용 (Fill 이미지)
    public Color normalColor = Color.green;
    public Color warningColor = Color.red;

    public float warningThreshold = 20f;   // 빨간색 경고 기준
    private bool isBlinking = false;       // 깜빡이는 중인지
    private float blinkTimer = 0f;
    public float blinkInterval = 0.5f;     // 깜빡이는 주기

    void Start()
    {
        FindTimeAffectedObjects();  // ITimeAffectable 붙은 애들 찾기
        currentTimeGauge = maxTimeGauge;   // 게이지 풀로 채우기
        UpdateTimeGaugeUI();               // UI 슬라이더도 초기화
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleTimeStop();
        }

        //시간 정지 중이면 게이지 소모, 0 되면 자동 해제
        if (isTimeStopped)
        {
            currentTimeGauge -= timeStopDrainRate * Time.deltaTime;
            if (currentTimeGauge <= 0)
            {
                currentTimeGauge = 0;
                ResumeTime();
            }
        }
        //시간 정지 아니면 게이지 자연 충전
        else
        {
            currentTimeGauge += passiveChargeRate * Time.deltaTime;
            currentTimeGauge = Mathf.Clamp(currentTimeGauge, 0, maxTimeGauge);
        }

        //Ctrl + 좌클릭 → 특정 오브젝트 시간정지 해제
        if (isTimeStopped && Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.up, 0.1f);

            
            if (hit.collider != null)
            {
                Debug.Log($"[DEBUG] 감지된 오브젝트: {hit.collider.gameObject.name}");

                var affectable = hit.collider.GetComponent<ITimeAffectable>();
                if (affectable != null)
                {
                    Debug.Log("[DEBUG] 시간정지 해제됨!");
                    affectable.ResumeTime();
                    affectable.RestoreColor();
                }
            }
        }

        // 5. UI 업데이트 (게이지 슬라이더)
        UpdateTimeGaugeUI();

        // 6. 깜빡임 처리 (게이지 부족 시)
        if (isBlinking)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                fillImage.color = fillImage.color == normalColor ? warningColor : normalColor;
            }
        }
    }


    // =================================================================== 시간 정지 관련

    // 스페이스바 눌렀을 때 실행 → 시간 정지 or 해제
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

    // 시간 정지 시작
    void StopTime()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("TimeStopSound");
        }

        isTimeStopped = true;

        foreach (var obj in timeAffectedObjects)
        {
            obj.StopTime();
        }
    }

    // 시간 정지 해제
    void ResumeTime()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("TimeStopRelease");
        }

        isTimeStopped = false;

        foreach (var obj in timeAffectedObjects)
        {
            if (obj != null)
            {
                obj.ResumeTime();
                obj.RestoreColor();
            }
        }
    }


    // =================================================================== 시간정지 대상 등록 및 삭제

    // 시간정지 영향을 받는 오브젝트 등록
    public void RegisterTimeAffectedObject(ITimeAffectable obj)
    {
        timeAffectedObjects.Add(obj);
    }

    // 시간정지 영향 대상 삭제 (예: 총알 터지면 삭제)
    public void RemoveTimeAffectedObject(ITimeAffectable obj)
    {
        timeAffectedObjects.Remove(obj);
    }

    // 시작할 때 자동으로 모든 ITimeAffectable 오브젝트 찾아서 리스트에 넣기
    void FindTimeAffectedObjects()
    {
        var affectables = FindObjectsOfType<MonoBehaviour>().OfType<ITimeAffectable>();
        timeAffectedObjects.AddRange(affectables);
    }



    // =================================================================== 시간 게이지 조작
    // 적 처치 등으로 게이지 회복
    public void AddTimeGauge(float amount)
    {
        currentTimeGauge += amount;
        currentTimeGauge = Mathf.Clamp(currentTimeGauge, 0, maxTimeGauge);
        UpdateTimeGaugeUI();
    }

    // =================================================================== UI 슬라이더 업데이트
    void UpdateTimeGaugeUI()
    {
        if (timeGaugeSlider != null)
        {
            timeGaugeSlider.value = currentTimeGauge;

            if (currentTimeGauge <= warningThreshold)
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
                    fillImage.color = normalColor;
                }
            }
        }
    }
}
