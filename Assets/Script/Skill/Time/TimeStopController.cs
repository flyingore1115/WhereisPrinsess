using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class TimeStopController : MonoBehaviour
{
    private bool isTimeStopped = false;
    private List<ITimeAffectable> timeAffectedObjects = new List<ITimeAffectable>();

    public float maxTimeGauge = 100f;
    public float currentTimeGauge;
    public float passiveChargeRate = 0.5f;
    public float timeStopDrainRate = 1f;
    public float enemyKillGain = 5f;

    public bool IsTimeStopped => isTimeStopped;


    //================================================================슬라이더
    public Slider timeGaugeSlider; // UI Slider 연결
    public Image fillImage; // 슬라이더 Fill 이미지 참조 추가
    public Color normalColor = Color.green; // 평소 색깔
    public Color warningColor = Color.red; // 부족할 때 색깔

    public float warningThreshold = 20f; // 부족 기준 (20 이하)
    private bool isBlinking = false;
    private float blinkTimer = 0f;
    public float blinkInterval = 0.5f; // 깜빡이는 주기 (초)

    void Start()
    {
        FindTimeAffectedObjects();
        currentTimeGauge = maxTimeGauge;

        // 시작할 때 게이지 UI 업데이트
        UpdateTimeGaugeUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) //시간 멈추는거
        {
            ToggleTimeStop();
        }

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
            if (!isTimeStopped)
            {
                currentTimeGauge += passiveChargeRate * Time.deltaTime;
                currentTimeGauge = Mathf.Clamp(currentTimeGauge, 0, maxTimeGauge);
            }
        }

        if (isTimeStopped && Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl)) //대상 정지 해제
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                var affectable = hit.collider.GetComponent<ITimeAffectable>();
                if (affectable != null)
                {
                    affectable.ResumeTime();
                    affectable.RestoreColor();
                }
            }
        }

        UpdateTimeGaugeUI(); // 매 프레임 UI 업데이트

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
            }
        }
    }

    void FindTimeAffectedObjects()
    {
        var affectables = FindObjectsOfType<MonoBehaviour>().OfType<ITimeAffectable>();
        timeAffectedObjects.AddRange(affectables);
    }

    public void AddTimeGauge(float amount)
    {
        currentTimeGauge += amount;
        currentTimeGauge = Mathf.Clamp(currentTimeGauge, 0, maxTimeGauge);

        UpdateTimeGaugeUI(); // 충전될 때도 UI 업데이트
    }

    void UpdateTimeGaugeUI()
    {
        if (timeGaugeSlider != null)
        {
            timeGaugeSlider.value = currentTimeGauge;

            // 게이지 부족 여부 확인
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
                    fillImage.color = normalColor; // 원래 색상 복구
                }
            }
        }
    }
    public void RegisterTimeAffectedObject(ITimeAffectable obj)
    {
        timeAffectedObjects.Add(obj);
    }

}
