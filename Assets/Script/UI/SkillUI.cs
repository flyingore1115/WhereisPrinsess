using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour
{
    public SkillManager skillManager;
    public GameObject skillPanel;
    public RectTransform skillUIRoot; // UI 전체를 감싸는 부모 오브젝트 (RectTransform)
    public GameObject skillDetailPanel; // 스킬 설명 패널
    public Transform clockCenter; // 시계 중심
    public Transform skillHandHour; // 시침 역할
    public Transform skillHandMinute; // 분침 역할
    public List<Image> skillIcons; // 시계 숫자 위치의 스킬 아이콘들
    public Text skillNameText;
    public Text skillLevelText;
    public Button upgradeButton;

    [Header("Zoom Settings")]
    public float zoomMultiplier = 1.5f; // 인스펙터에서 조절 가능한 확대 배율
    public float zoomDuration = 0.3f;   // 줌 효과 지속 시간

    private int selectedSkillIndex = 0; // 플레이어가 선택한 스킬 (분침 위치)
    private int unlockedSkillIndex = 0; // 해금된 스킬의 최대 위치 (시침 위치)
    private bool isUIActive = false;
    private bool isSkillDetailOpen = false; // 스킬 상세 보기 상태
    private Vector3 defaultScale; // UI 루트의 기본 scale
    private Vector3 defaultPosition; // UI 루트의 기본 anchoredPosition

    void Start()
    {
        skillPanel.SetActive(false);
        skillDetailPanel.SetActive(false); // 상세 패널 비활성화
        defaultScale = skillUIRoot.localScale; // 기본 scale 저장 (예: (1,1,1))
        defaultPosition = skillUIRoot.anchoredPosition; // 기본 위치 저장 (예: (0,0))
        UpdateSkillUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSkillPanel();
        }

        if (!isUIActive)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectPreviousSkill();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SelectNextSkill();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            UpgradeSkill();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleSkillDetail();
        }

        // 숫자키 1~9로 테스트용 스킬 해금
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                UnlockSkills(i);
            }
        }
    }

    void ToggleSkillPanel()
    {
        isUIActive = !isUIActive;
        skillPanel.SetActive(isUIActive);
        Time.timeScale = isUIActive ? 0f : 1f;
    }

    void SelectPreviousSkill()
    {
        selectedSkillIndex = (selectedSkillIndex - 1 + skillIcons.Count) % skillIcons.Count;
        if (selectedSkillIndex == skillIcons.Count - 1 || selectedSkillIndex > unlockedSkillIndex)
        {
            Debug.Log("처음에서 뒤로 이동 -> 마지막 해금된 스킬로 이동");
            selectedSkillIndex = unlockedSkillIndex;
        }
        UpdateSkillUI();
    }

    void SelectNextSkill()
    {
        selectedSkillIndex = (selectedSkillIndex + 1) % skillIcons.Count;
        if (selectedSkillIndex > unlockedSkillIndex)
        {
            Debug.Log("해금되지 않은 스킬을 선택하여 초기 위치로 이동");
            selectedSkillIndex = 0;
        }
        UpdateSkillUI();
    }

    void UpgradeSkill()
    {
        if (selectedSkillIndex > unlockedSkillIndex)
        {
            Debug.Log("아직 해금 X");
            return;
        }
        SkillData skill = skillManager.allSkills[selectedSkillIndex];
        //skillManager.UpgradeSkill(skill);
        UpdateSkillUI();
    }

    void UnlockSkills(int count)
    {
        unlockedSkillIndex = Mathf.Clamp(count - 1, 0, skillIcons.Count - 1);
        UpdateSkillUI();
    }

    void UpdateSkillUI()
    {
        SkillData skill = skillManager.allSkills[selectedSkillIndex];
        skillNameText.text = skill.skillName;
        skillLevelText.text = "Level: " + skillManager.GetSkillLevel(skill);
        bool isUnlocked = selectedSkillIndex <= unlockedSkillIndex;
        upgradeButton.interactable = isUnlocked && skillManager.GetSkillLevel(skill) < skill.maxLevel;
        for (int i = 0; i < skillIcons.Count; i++)
        {
            skillIcons[i].color = (i <= unlockedSkillIndex) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
        RotateClockHands();
    }

    void RotateClockHands()
    {
        if (skillIcons.Count == 0)
            return;
        Vector3 minuteTargetPos = skillIcons[selectedSkillIndex].rectTransform.position;
        Vector3 minuteDirection = (minuteTargetPos - clockCenter.position).normalized;
        float minuteAngle = Mathf.Atan2(minuteDirection.y, minuteDirection.x) * Mathf.Rad2Deg - 90f;
        skillHandMinute.localRotation = Quaternion.Euler(0, 0, minuteAngle);
        Vector3 hourTargetPos = skillIcons[unlockedSkillIndex].rectTransform.position;
        Vector3 hourDirection = (hourTargetPos - clockCenter.position).normalized;
        float hourAngle = Mathf.Atan2(hourDirection.y, hourDirection.x) * Mathf.Rad2Deg - 90f;
        skillHandHour.localRotation = Quaternion.Euler(0, 0, hourAngle);
    }

    // 선택된 스킬 아이콘을 기준으로 확대(줌 인)하도록 UI 루트의 Scale과 anchoredPosition을 조정
    void ToggleSkillDetail()
    {
        if (isSkillDetailOpen)
        {
            skillDetailPanel.SetActive(false);
            StartCoroutine(ZoomAndRepositionUI(skillUIRoot, defaultScale, defaultPosition));
        }
        else
        {
            skillDetailPanel.SetActive(true);
            Vector2 iconPos = skillIcons[selectedSkillIndex].rectTransform.anchoredPosition;
            Vector3 targetScale = defaultScale * zoomMultiplier;
            // 목표 anchoredPosition = - (targetScale * iconPos)
            Vector3 targetPos = - new Vector3(targetScale.x * iconPos.x, targetScale.y * iconPos.y, defaultPosition.z);
            StartCoroutine(ZoomAndRepositionUI(skillUIRoot, targetScale, targetPos));
        }
        isSkillDetailOpen = !isSkillDetailOpen;
    }

    IEnumerator ZoomAndRepositionUI(RectTransform uiRoot, Vector3 targetScale, Vector3 targetPosition)
    {
        float elapsed = 0f;
        Vector3 startScale = uiRoot.localScale;
        Vector3 startPos = uiRoot.anchoredPosition;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            uiRoot.localScale = Vector3.Lerp(startScale, targetScale, elapsed / zoomDuration);
            uiRoot.anchoredPosition = Vector3.Lerp(startPos, targetPosition, elapsed / zoomDuration);
            yield return null;
        }
        uiRoot.localScale = targetScale;
        uiRoot.anchoredPosition = targetPosition;
    }
}
